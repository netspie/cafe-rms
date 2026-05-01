import Link from "next/link"
import { Pencil, Plus, Trash2 } from "lucide-react"
import { revalidatePath } from "next/cache"
import { Field } from "@/components/field"
import { ShibaMark } from "@/components/shiba-mark"
import { SortableTh } from "@/components/sortable-th"
import { api, type PagedResult } from "@/lib/server-api"

interface PromotionCodeItem {
  id: string
  code: string
  discountPercentage: number
  validFrom: string | null
  validUntil: string | null
  maxUses: number | null
  usesCount: number
  createdAt: string
}

const PAGE_SIZE = 20

async function createPromotionCode(formData: FormData) {
  "use server"
  const validFrom = formData.get("validFrom") as string
  const validUntil = formData.get("validUntil") as string
  const maxUses = formData.get("maxUses") as string
  await api.post("/api/promotion-codes", {
    code: formData.get("code") as string,
    discountPercentage: Number(formData.get("discountPercentage")),
    validFrom: validFrom ? new Date(validFrom).toISOString() : null,
    validUntil: validUntil ? new Date(validUntil).toISOString() : null,
    maxUses: maxUses ? Number(maxUses) : null,
  })
  revalidatePath("/admin/promotion-codes")
}

async function deletePromotionCode(id: string) {
  "use server"
  await api.delete(`/api/promotion-codes/${id}`)
  revalidatePath("/admin/promotion-codes")
}

export default async function PromotionCodesListPage({ searchParams }: { searchParams: Promise<{ page?: string; q?: string; sort?: string }> }) {
  const sp = await searchParams
  const page = Number(sp.page ?? 1)
  const filter = sp.q ?? ""
  const sort = sp.sort ?? "-createdAt"
  const search = new URLSearchParams({ page: String(page), pageSize: String(PAGE_SIZE), sort })
  if (filter) search.set("code", filter)
  const data = await api.get<PagedResult<PromotionCodeItem>>(`/api/promotion-codes?${search}`)
  const totalPages = Math.max(1, Math.ceil(data.total / PAGE_SIZE))
  const preserve: Record<string, string> = {}
  if (filter) preserve.q = filter

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Promotion codes</h1>
      </div>

      <form className="flex items-center gap-3">
        <input name="q" defaultValue={filter} className="h-9 max-w-xs rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        <button type="submit" className="h-9 rounded-md border px-3 text-sm hover:bg-accent">Search</button>
      </form>

      <form action={createPromotionCode} className="grid gap-3 rounded-md border bg-card p-3 sm:grid-cols-[1fr_auto_auto_auto_auto_auto] sm:items-end">
        <Field label="Code">
          <input name="code" required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <Field label="Discount %">
          <input name="discountPercentage" type="number" step="0.01" min="0" max="100" defaultValue="10" required className="h-9 w-24 rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <Field label="Valid from">
          <input name="validFrom" type="date" className="h-9 rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <Field label="Valid until">
          <input name="validUntil" type="date" className="h-9 rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <Field label="Max uses">
          <input name="maxUses" type="number" min="1" step="1" className="h-9 w-20 rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <button type="submit" className="inline-flex h-9 items-center gap-2 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90">
          <Plus className="h-4 w-4" />Add
        </button>
      </form>

      <div className="overflow-hidden rounded-md border">
        <table className="w-full text-sm">
          <thead className="bg-muted/50 text-left">
            <tr>
              <SortableTh label="Code" field="code" currentSort={sort} preserve={preserve} />
              <th className="px-3 py-2 text-right font-medium">Discount</th>
              <SortableTh label="Valid window" field="validUntil" currentSort={sort} preserve={preserve} />
              <th className="px-3 py-2 text-right font-medium">Used</th>
              <th className="w-20 px-3 py-2" />
            </tr>
          </thead>
          <tbody>
            {data.items.length === 0 && (
              <tr><td colSpan={5} className="px-3 py-12">
                <div className="flex flex-col items-center gap-3 text-muted-foreground">
                  <ShibaMark className="h-10 w-10 opacity-60" />
                  <p>No promotion codes match this filter.</p>
                </div>
              </td></tr>
            )}
            {data.items.map((p) => {
              const window = `${p.validFrom ? new Date(p.validFrom).toLocaleDateString() : "any time"} → ${p.validUntil ? new Date(p.validUntil).toLocaleDateString() : "no expiry"}`
              return (
                <tr key={p.id} className="border-t">
                  <td className="px-3 py-2 font-mono font-medium">{p.code}</td>
                  <td className="px-3 py-2 text-right font-mono">{p.discountPercentage}%</td>
                  <td className="px-3 py-2 text-muted-foreground">{window}</td>
                  <td className="px-3 py-2 text-right font-mono text-sm">{p.usesCount}{p.maxUses != null ? ` / ${p.maxUses}` : ""}</td>
                  <td className="px-3 py-2">
                    <div className="flex justify-end gap-1">
                      <Link href={`/admin/promotion-codes/${p.id}`} className="inline-flex h-8 w-8 items-center justify-center rounded-md hover:bg-accent" aria-label="Edit"><Pencil className="h-4 w-4" /></Link>
                      <form action={deletePromotionCode.bind(null, p.id)}>
                        <button type="submit" className="inline-flex h-8 w-8 items-center justify-center rounded-md text-destructive hover:bg-destructive/10" aria-label="Delete"><Trash2 className="h-4 w-4" /></button>
                      </form>
                    </div>
                  </td>
                </tr>
              )
            })}
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
