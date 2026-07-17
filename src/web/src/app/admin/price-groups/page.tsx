import Link from "next/link"
import { Pencil, Plus, Trash2 } from "lucide-react"
import { revalidatePath } from "next/cache"
import { Field } from "@/components/field"
import { ShibaMark } from "@/components/shiba-mark"
import { SortableTh } from "@/components/sortable-th"
import { api, type PagedResult } from "@/lib/server-api"
import { requirePermissionFor } from "@/features/auth/access"

interface PriceGroupItem { id: string; name: string; createdAt: string }

const PAGE_SIZE = 20

async function createPriceGroup(formData: FormData) {
  "use server"
  await api.post("/api/price-groups", { name: formData.get("name") as string })
  revalidatePath("/admin/price-groups")
}

async function deletePriceGroup(id: string) {
  "use server"
  await api.delete(`/api/price-groups/${id}`)
  revalidatePath("/admin/price-groups")
}

export default async function PriceGroupsListPage({ searchParams }: { searchParams: Promise<{ page?: string; q?: string; sort?: string }> }) {
  await requirePermissionFor("/admin/price-groups")
  const sp = await searchParams
  const page = Number(sp.page ?? 1)
  const filter = sp.q ?? ""
  const sort = sp.sort ?? "name"
  const search = new URLSearchParams({ page: String(page), pageSize: String(PAGE_SIZE), sort })
  if (filter) search.set("name", filter)
  const data = await api.get<PagedResult<PriceGroupItem>>(`/api/price-groups?${search}`)
  const totalPages = Math.max(1, Math.ceil(data.total / PAGE_SIZE))
  const preserve: Record<string, string> = {}
  if (filter) preserve.q = filter

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Price groups</h1>
      </div>

      <form className="flex items-center gap-3">
        <input name="q" defaultValue={filter} className="h-9 max-w-xs rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        <button type="submit" className="h-9 rounded-md border px-3 text-sm hover:bg-accent">Search</button>
      </form>

      <form action={createPriceGroup} className="flex items-end gap-3 rounded-md border bg-card p-3">
        <Field label="Name">
          <input name="name" required className="h-9 w-48 rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <button type="submit" className="inline-flex h-9 items-center gap-2 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90">
          <Plus className="h-4 w-4" />Add price group
        </button>
      </form>

      <div className="overflow-hidden rounded-md border">
        <table className="w-full text-sm">
          <thead className="bg-muted/50 text-left">
            <tr>
              <SortableTh label="Name" field="name" currentSort={sort} preserve={preserve} />
              <SortableTh label="Created" field="createdAt" currentSort={sort} preserve={preserve} />
              <th className="w-20 px-3 py-2" />
            </tr>
          </thead>
          <tbody>
            {data.items.length === 0 && (
              <tr><td colSpan={3} className="px-3 py-12">
                <div className="flex flex-col items-center gap-3 text-muted-foreground">
                  <ShibaMark className="h-10 w-10 opacity-60" />
                  <p>No price groups match this filter.</p>
                </div>
              </td></tr>
            )}
            {data.items.map((it) => (
              <tr key={it.id} className="border-t">
                <td className="px-3 py-2 font-medium">{it.name}</td>
                <td className="px-3 py-2 text-muted-foreground">{new Date(it.createdAt).toLocaleDateString()}</td>
                <td className="px-3 py-2">
                  <div className="flex justify-end gap-1">
                    <Link href={`/admin/price-groups/${it.id}`} className="inline-flex h-8 w-8 items-center justify-center rounded-md hover:bg-accent" aria-label="Edit"><Pencil className="h-4 w-4" /></Link>
                    <form action={deletePriceGroup.bind(null, it.id)}>
                      <button type="submit" className="inline-flex h-8 w-8 items-center justify-center rounded-md text-destructive hover:bg-destructive/10" aria-label="Delete"><Trash2 className="h-4 w-4" /></button>
                    </form>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <div className="flex items-center justify-between">
        <p className="text-sm text-muted-foreground">{data.total} total</p>
        <div className="flex items-center gap-2">
          {page > 1 ? <Link href={{ query: { ...preserve, sort, page: page - 1 } }} className="inline-flex h-8 items-center rounded-md border px-3 text-sm hover:bg-accent">Previous</Link> : <span className="inline-flex h-8 items-center rounded-md border px-3 text-sm opacity-50">Previous</span>}
          <span className="text-sm text-muted-foreground">Page {page} of {totalPages}</span>
          {page < totalPages ? <Link href={{ query: { ...preserve, sort, page: page + 1 } }} className="inline-flex h-8 items-center rounded-md border px-3 text-sm hover:bg-accent">Next</Link> : <span className="inline-flex h-8 items-center rounded-md border px-3 text-sm opacity-50">Next</span>}
        </div>
      </div>
    </div>
  )
}
