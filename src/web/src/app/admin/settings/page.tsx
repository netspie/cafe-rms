import { revalidatePath } from "next/cache"
import { Field } from "@/components/field"
import { api } from "@/lib/server-api"

interface MeResult { companyId: string | null; outletId: string | null }
interface CompanyDetail {
  id: string
  legalName: string
  taxId: string
  invoicingAddress: string
  billingEmail: string
  billingPhone: string
  isPublic: boolean
}
interface OutletDetail {
  id: string
  displayName: string
  streetAddress: string
  phone: string
  timeZone: string
  currency: string
  logoUrl: string | null
}

const CURRENCIES = ["PLN", "EUR", "USD", "GBP", "CZK"]

export default async function SettingsPage() {
  const me = await api.get<MeResult>("/api/me")
  if (!me.companyId) return <p className="text-sm text-muted-foreground">No company is associated with your account.</p>

  const [company, outlet] = await Promise.all([
    api.get<CompanyDetail>(`/api/companies/${me.companyId}`),
    me.outletId ? api.get<OutletDetail>(`/api/outlets/${me.outletId}`) : Promise.resolve(null),
  ])

  async function updateCompany(formData: FormData) {
    "use server"
    await api.put(`/api/companies/${company.id}`, {
      legalName: formData.get("legalName") as string,
      taxId: formData.get("taxId") as string,
      invoicingAddress: formData.get("invoicingAddress") as string,
      billingEmail: formData.get("billingEmail") as string,
      billingPhone: formData.get("billingPhone") as string,
      isPublic: formData.get("isPublic") === "on",
    })
    revalidatePath("/admin/settings")
  }

  async function updateOutlet(formData: FormData) {
    "use server"
    if (!outlet) return
    await api.put(`/api/outlets/${outlet.id}`, {
      displayName: formData.get("displayName") as string,
      streetAddress: formData.get("streetAddress") as string,
      phone: formData.get("phone") as string,
      timeZone: formData.get("timeZone") as string,
      currency: formData.get("currency") as string,
      logoUrl: (formData.get("logoUrl") as string) || null,
    })
    revalidatePath("/admin/settings")
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Settings</h1>
        <p className="text-muted-foreground">Company and outlet details — visible on receipts and listings.</p>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <section className="rounded-lg border bg-card p-4">
          <h2 className="text-base font-semibold">Company</h2>
          <p className="mb-4 text-xs text-muted-foreground">Legal and billing information.</p>
          <form action={updateCompany} className="space-y-4">
            <Field label="Legal name">
              <input name="legalName" defaultValue={company.legalName} required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
            </Field>
            <div className="grid grid-cols-2 gap-3">
              <Field label="Tax ID">
                <input name="taxId" defaultValue={company.taxId} required placeholder="NIP" className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
              </Field>
              <Field label="Billing phone">
                <input name="billingPhone" defaultValue={company.billingPhone} required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
              </Field>
            </div>
            <Field label="Billing email">
              <input name="billingEmail" type="email" defaultValue={company.billingEmail} required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
            </Field>
            <Field label="Invoicing address">
              <input name="invoicingAddress" defaultValue={company.invoicingAddress} required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
            </Field>
            <label className="flex cursor-pointer items-center gap-2 text-sm">
              <input type="checkbox" name="isPublic" defaultChecked={company.isPublic} className="h-4 w-4" />
              <span>Listed publicly in the CafeRMS public directory</span>
            </label>
            <button type="submit" className="h-9 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90">Save company</button>
          </form>
        </section>

        {outlet && (
          <section className="rounded-lg border bg-card p-4">
            <h2 className="text-base font-semibold">Outlet</h2>
            <p className="mb-4 text-xs text-muted-foreground">Customer-facing café details.</p>
            <form action={updateOutlet} className="space-y-4">
              <Field label="Display name">
                <input name="displayName" defaultValue={outlet.displayName} required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
              </Field>
              <Field label="Street address">
                <input name="streetAddress" defaultValue={outlet.streetAddress} required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
              </Field>
              <div className="grid grid-cols-2 gap-3">
                <Field label="Phone">
                  <input name="phone" defaultValue={outlet.phone} required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
                </Field>
                <Field label="Time zone">
                  <input name="timeZone" defaultValue={outlet.timeZone} required placeholder="Europe/Warsaw" className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
                </Field>
              </div>
              <Field label="Currency">
                <select name="currency" defaultValue={outlet.currency} required className="h-9 max-w-xs rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30">
                  {CURRENCIES.map((c) => <option key={c} value={c}>{c}</option>)}
                </select>
              </Field>
              <Field label="Logo URL" hint="Shown on receipts and the public listing.">
                <input name="logoUrl" defaultValue={outlet.logoUrl ?? ""} placeholder="https://… (optional)" className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
              </Field>
              <button type="submit" className="h-9 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90">Save outlet</button>
            </form>
          </section>
        )}
      </div>
    </div>
  )
}
