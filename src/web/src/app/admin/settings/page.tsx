import { revalidatePath } from "next/cache"
import { Field } from "@/components/field"
import { api, type PagedResult } from "@/lib/server-api"
import { requirePermissionFor } from "@/features/auth/access"

interface MeResult { outletId: string | null }
interface NamedRow { id: string; name: string }
interface OutletDetail {
  id: string
  displayName: string
  streetAddress: string
  phone: string
  timeZone: string
  currency: string
  logoUrl: string | null
  legalName: string
  taxId: string
  invoicingAddress: string
  billingEmail: string
  billingPhone: string
  defaultPriceGroupId: string | null
  defaultProductListId: string | null
}

const CURRENCIES = ["PLN", "EUR", "USD", "GBP", "CZK"]
const inputClass = "h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30"

export default async function SettingsPage() {
  await requirePermissionFor("/admin/settings")
  const me = await api.get<MeResult>("/api/me")
  if (!me.outletId) return <p className="text-sm text-muted-foreground">No outlet is configured yet.</p>

  const [outlet, priceGroups, productLists] = await Promise.all([
    api.get<OutletDetail>(`/api/outlets/${me.outletId}`),
    api.get<PagedResult<NamedRow>>(`/api/price-groups?page=1&pageSize=100&sort=name`),
    api.get<PagedResult<NamedRow>>(`/api/product-lists?page=1&pageSize=100&sort=name`),
  ])

  async function updateOutlet(formData: FormData) {
    "use server"
    await api.put(`/api/outlets/${outlet.id}`, {
      displayName: formData.get("displayName") as string,
      streetAddress: formData.get("streetAddress") as string,
      phone: formData.get("phone") as string,
      timeZone: formData.get("timeZone") as string,
      currency: formData.get("currency") as string,
      logoUrl: (formData.get("logoUrl") as string) || null,
      legalName: formData.get("legalName") as string,
      taxId: formData.get("taxId") as string,
      invoicingAddress: formData.get("invoicingAddress") as string,
      billingEmail: formData.get("billingEmail") as string,
      billingPhone: formData.get("billingPhone") as string,
      defaultPriceGroupId: (formData.get("defaultPriceGroupId") as string) || null,
      defaultProductListId: (formData.get("defaultProductListId") as string) || null,
    })
    revalidatePath("/admin/settings")
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Company &amp; outlet</h1>
      </div>

      <form action={updateOutlet} className="grid gap-6 lg:grid-cols-2">
        <section className="rounded-lg border bg-card p-4">
          <h2 className="mb-4 text-base font-semibold">Outlet</h2>
          <div className="space-y-4">
            <Field label="Display name">
              <input name="displayName" defaultValue={outlet.displayName} required className={inputClass} />
            </Field>
            <Field label="Street address">
              <input name="streetAddress" defaultValue={outlet.streetAddress} required className={inputClass} />
            </Field>
            <div className="grid grid-cols-2 gap-3">
              <Field label="Phone">
                <input name="phone" defaultValue={outlet.phone} required className={inputClass} />
              </Field>
              <Field label="Time zone">
                <input name="timeZone" defaultValue={outlet.timeZone} required className={inputClass} />
              </Field>
            </div>
            <Field label="Currency">
              <select name="currency" defaultValue={outlet.currency} required className={`${inputClass} max-w-xs`}>
                {CURRENCIES.map((c) => <option key={c} value={c}>{c}</option>)}
              </select>
            </Field>
            <Field label="Logo URL">
              <input name="logoUrl" defaultValue={outlet.logoUrl ?? ""} className={inputClass} />
            </Field>
          </div>
        </section>

        <section className="rounded-lg border bg-card p-4">
          <h2 className="mb-4 text-base font-semibold">Legal & billing</h2>
          <div className="space-y-4">
            <Field label="Legal name">
              <input name="legalName" defaultValue={outlet.legalName} required className={inputClass} />
            </Field>
            <div className="grid grid-cols-2 gap-3">
              <Field label="Tax ID">
                <input name="taxId" defaultValue={outlet.taxId} required className={inputClass} />
              </Field>
              <Field label="Billing phone">
                <input name="billingPhone" defaultValue={outlet.billingPhone} required className={inputClass} />
              </Field>
            </div>
            <Field label="Billing email">
              <input name="billingEmail" type="email" defaultValue={outlet.billingEmail} required className={inputClass} />
            </Field>
            <Field label="Invoicing address">
              <input name="invoicingAddress" defaultValue={outlet.invoicingAddress} required className={inputClass} />
            </Field>
          </div>
        </section>

        <section className="rounded-lg border bg-card p-4 lg:col-span-2">
          <h2 className="mb-4 text-base font-semibold">Customer menu</h2>
          <div className="grid gap-4 sm:grid-cols-2">
            <Field label="Default price group">
              <select key={`pg-${outlet.defaultPriceGroupId ?? "none"}`} name="defaultPriceGroupId" defaultValue={outlet.defaultPriceGroupId ?? ""} className={inputClass}>
                <option value="">— none —</option>
                {priceGroups.items.map((g) => <option key={g.id} value={g.id}>{g.name}</option>)}
              </select>
            </Field>
            <Field label="Default menu (product list)">
              <select key={`pl-${outlet.defaultProductListId ?? "none"}`} name="defaultProductListId" defaultValue={outlet.defaultProductListId ?? ""} className={inputClass}>
                <option value="">All products</option>
                {productLists.items.map((l) => <option key={l.id} value={l.id}>{l.name}</option>)}
              </select>
            </Field>
          </div>
          <p className="mt-3 text-xs text-muted-foreground">Controls what the mobile app shows: the menu is limited to the selected list (or all products), priced at the default group. Event days override the price for their products.</p>
        </section>

        <div className="lg:col-span-2">
          <button type="submit" className="h-9 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90">Save settings</button>
        </div>
      </form>
    </div>
  )
}
