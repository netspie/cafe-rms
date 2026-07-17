import { revalidatePath } from "next/cache"
import { X } from "lucide-react"
import { Field } from "@/components/field"
import { api, type PagedResult } from "@/lib/server-api"
import { requirePermissionFor } from "@/features/auth/access"

interface SalesChannelDetail { id: string; name: string; isTakeout: boolean; priceGroupIds: string[] }
interface PriceGroupRow { id: string; name: string }

export default async function EditSalesChannelPage({ params }: { params: Promise<{ id: string }> }) {
  await requirePermissionFor("/admin/sales-channels")
  const { id } = await params
  const [channel, priceGroups] = await Promise.all([
    api.get<SalesChannelDetail>(`/api/sales-channels/${id}`),
    api.get<PagedResult<PriceGroupRow>>(`/api/price-groups?page=1&pageSize=100&sort=name`),
  ])
  const attached = priceGroups.items.filter((g) => channel.priceGroupIds.includes(g.id))
  const available = priceGroups.items.filter((g) => !channel.priceGroupIds.includes(g.id))

  async function updateBasics(formData: FormData) {
    "use server"
    await api.put(`/api/sales-channels/${id}`, {
      name: formData.get("name") as string,
      isTakeout: formData.get("isTakeout") === "on",
    })
    revalidatePath(`/admin/sales-channels/${id}`)
  }

  async function linkPriceGroup(formData: FormData) {
    "use server"
    const priceGroupId = formData.get("priceGroupId") as string
    if (!priceGroupId) return
    await api.post(`/api/sales-channels/${id}/price-groups/${priceGroupId}`)
    revalidatePath(`/admin/sales-channels/${id}`)
  }

  async function unlinkPriceGroup(priceGroupId: string) {
    "use server"
    await api.delete(`/api/sales-channels/${id}/price-groups/${priceGroupId}`)
    revalidatePath(`/admin/sales-channels/${id}`)
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">{channel.name}</h1>
        <p className="text-muted-foreground">Rename, toggle takeout flag, link price groups.</p>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <section className="rounded-lg border bg-card p-4">
          <h2 className="mb-4 text-base font-semibold">Basics</h2>
          <form action={updateBasics} className="space-y-4">
            <Field label="Name">
              <input name="name" defaultValue={channel.name} required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
            </Field>
            <label className="flex cursor-pointer items-center gap-2 text-sm">
              <input type="checkbox" name="isTakeout" defaultChecked={channel.isTakeout} className="h-4 w-4" />
              <span>This channel is takeout (off-premises)</span>
            </label>
            <button type="submit" className="h-9 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90">Save changes</button>
          </form>
        </section>

        <section className="rounded-lg border bg-card p-4">
          <h2 className="mb-4 text-base font-semibold">Price groups</h2>
          <div className="space-y-3">
            <div className="flex flex-wrap gap-2">
              {attached.length === 0 && <p className="text-sm text-muted-foreground">No price groups linked.</p>}
              {attached.map((g) => (
                <form key={g.id} action={unlinkPriceGroup.bind(null, g.id)} className="inline-flex">
                  <button type="submit" className="inline-flex items-center gap-1.5 rounded-full bg-secondary px-2.5 py-0.5 text-xs hover:text-destructive" aria-label={`Unlink ${g.name}`}>
                    {g.name}<X className="h-3 w-3" />
                  </button>
                </form>
              ))}
            </div>
            <form action={linkPriceGroup} className="flex gap-2">
              <select name="priceGroupId" className="h-9 max-w-xs flex-1 rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30">
                <option value="">Link price group…</option>
                {available.map((g) => <option key={g.id} value={g.id}>{g.name}</option>)}
              </select>
              <button type="submit" className="h-9 rounded-md border px-3 text-sm hover:bg-accent">Link</button>
            </form>
          </div>
        </section>
      </div>
    </div>
  )
}
