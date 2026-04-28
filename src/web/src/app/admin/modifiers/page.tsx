import Link from "next/link"
import { Pencil, Plus, Trash2 } from "lucide-react"
import { revalidatePath } from "next/cache"
import { Field } from "@/components/field"
import { ShibaMark } from "@/components/shiba-mark"
import { api, type PagedResult } from "@/lib/server-api"

interface ModifierItem { id: string; name: string; priceDelta: number; modifierGroupId: string; createdAt: string }
interface NamedRow { id: string; name: string }

const PAGE_SIZE = 20

async function createModifier(formData: FormData) {
  "use server"
  await api.post("/api/modifiers", {
    modifierGroupId: formData.get("modifierGroupId") as string,
    name: formData.get("name") as string,
    priceDelta: Number(formData.get("priceDelta")),
  })
  revalidatePath("/admin/modifiers")
}

async function deleteModifier(id: string) {
  "use server"
  await api.delete(`/api/modifiers/${id}`)
  revalidatePath("/admin/modifiers")
}

export default async function ModifiersListPage({ searchParams }: { searchParams: Promise<{ page?: string; q?: string; group?: string }> }) {
  const sp = await searchParams
  const page = Number(sp.page ?? 1)
  const filter = sp.q ?? ""
  const groupId = sp.group ?? ""

  const search = new URLSearchParams({ page: String(page), pageSize: String(PAGE_SIZE), sort: "name" })
  if (filter) search.set("name", filter)
  if (groupId) search.set("modifierGroupId", groupId)

  const [data, groups] = await Promise.all([
    api.get<PagedResult<ModifierItem>>(`/api/modifiers?${search}`),
    api.get<PagedResult<NamedRow>>(`/api/modifier-groups?page=1&pageSize=100&sort=name`),
  ])

  const groupName = (id: string) => groups.items.find((g) => g.id === id)?.name ?? "—"
  const totalPages = Math.max(1, Math.ceil(data.total / PAGE_SIZE))

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Modifiers</h1>
        <p className="text-muted-foreground">Customizations attached to products via groups.</p>
      </div>

      <form className="flex items-center gap-3">
        <input name="q" defaultValue={filter} placeholder="Filter by name…" className="h-9 max-w-xs rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        <select name="group" defaultValue={groupId} className="h-9 max-w-xs rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30">
          <option value="">All groups</option>
          {groups.items.map((g) => <option key={g.id} value={g.id}>{g.name}</option>)}
        </select>
        <button type="submit" className="h-9 rounded-md border px-3 text-sm hover:bg-accent">Search</button>
      </form>

      <form action={createModifier} className="grid gap-3 rounded-md border bg-card p-3 sm:grid-cols-[1fr_1fr_auto_auto] sm:items-end">
        <Field label="Group">
          <select name="modifierGroupId" required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30">
            <option value="">Pick a group…</option>
            {groups.items.map((g) => <option key={g.id} value={g.id}>{g.name}</option>)}
          </select>
        </Field>
        <Field label="Name">
          <input name="name" required placeholder="Oat milk" className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <Field label="Price delta">
          <input name="priceDelta" type="number" step="0.01" defaultValue="0" required className="h-9 w-24 rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <button type="submit" className="inline-flex h-9 items-center gap-2 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90">
          <Plus className="h-4 w-4" />Add
        </button>
      </form>

      <div className="overflow-hidden rounded-md border">
        <table className="w-full text-sm">
          <thead className="bg-muted/50 text-left">
            <tr>
              <th className="px-3 py-2 font-medium">Name</th>
              <th className="px-3 py-2 font-medium">Group</th>
              <th className="px-3 py-2 text-right font-medium">Price delta</th>
              <th className="w-20 px-3 py-2" />
            </tr>
          </thead>
          <tbody>
            {data.items.length === 0 && (
              <tr><td colSpan={4} className="px-3 py-12">
                <div className="flex flex-col items-center gap-3 text-muted-foreground">
                  <ShibaMark className="h-10 w-10 opacity-60" />
                  <p>No modifiers match this filter.</p>
                </div>
              </td></tr>
            )}
            {data.items.map((it) => (
              <tr key={it.id} className="border-t">
                <td className="px-3 py-2 font-medium">{it.name}</td>
                <td className="px-3 py-2 text-muted-foreground">{groupName(it.modifierGroupId)}</td>
                <td className="px-3 py-2 text-right font-mono">{it.priceDelta >= 0 ? `+${it.priceDelta.toFixed(2)}` : it.priceDelta.toFixed(2)}</td>
                <td className="px-3 py-2">
                  <div className="flex justify-end gap-1">
                    <Link href={`/admin/modifiers/${it.id}`} className="inline-flex h-8 w-8 items-center justify-center rounded-md hover:bg-accent" aria-label="Edit"><Pencil className="h-4 w-4" /></Link>
                    <form action={deleteModifier.bind(null, it.id)}>
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
          {page > 1 ? <Link href={{ query: { ...(filter ? { q: filter } : {}), ...(groupId ? { group: groupId } : {}), page: page - 1 } }} className="inline-flex h-8 items-center rounded-md border px-3 text-sm hover:bg-accent">Previous</Link> : <span className="inline-flex h-8 items-center rounded-md border px-3 text-sm opacity-50">Previous</span>}
          <span className="text-sm text-muted-foreground">Page {page} of {totalPages}</span>
          {page < totalPages ? <Link href={{ query: { ...(filter ? { q: filter } : {}), ...(groupId ? { group: groupId } : {}), page: page + 1 } }} className="inline-flex h-8 items-center rounded-md border px-3 text-sm hover:bg-accent">Next</Link> : <span className="inline-flex h-8 items-center rounded-md border px-3 text-sm opacity-50">Next</span>}
        </div>
      </div>
    </div>
  )
}
