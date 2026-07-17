import Link from "next/link"
import { revalidatePath } from "next/cache"
import { Field } from "@/components/field"
import { ShibaMark } from "@/components/shiba-mark"
import { SortableTh } from "@/components/sortable-th"
import { api, type PagedResult } from "@/lib/server-api"
import { requirePermissionFor } from "@/features/auth/access"

interface LoyaltyEntry { id: string; userId: string; points: number; reason: string | null; createdAt: string }
interface UserRow { id: string; firstName: string; lastName: string; email: string }

const PAGE_SIZE = 20

async function recordAdjustment(formData: FormData) {
  "use server"
  await api.post("/api/loyalty/entries", {
    userId: formData.get("userId") as string,
    points: Number(formData.get("points")),
    reason: formData.get("reason") as string,
  })
  revalidatePath("/admin/loyalty")
}

export default async function LoyaltyPage({ searchParams }: { searchParams: Promise<{ page?: string; user?: string; sort?: string }> }) {
  await requirePermissionFor("/admin/loyalty")
  const sp = await searchParams
  const page = Number(sp.page ?? 1)
  const userId = sp.user ?? ""
  const sort = sp.sort ?? "-createdAt"
  const search = new URLSearchParams({ page: String(page), pageSize: String(PAGE_SIZE), sort })
  if (userId) search.set("userId", userId)
  const preserve: Record<string, string> = {}
  if (userId) preserve.user = userId

  const [entries, users] = await Promise.all([
    api.get<PagedResult<LoyaltyEntry>>(`/api/loyalty/entries?${search}`),
    api.get<UserRow[]>("/api/users?accountType=Guest"),
  ])
  const userName = (id: string) => {
    const u = users.find((x) => x.id === id)
    return u ? `${u.firstName} ${u.lastName}` : `${id.slice(0, 8)}…`
  }
  const totalPages = Math.max(1, Math.ceil(entries.total / PAGE_SIZE))

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Loyalty</h1>
      </div>

      <div className="grid gap-6 lg:grid-cols-3">
        <section className="rounded-lg border bg-card p-4 lg:col-span-2">
          <h2 className="mb-4 text-base font-semibold">Activity</h2>
          <div className="space-y-4">
            <form className="flex items-center gap-3">
              <select name="user" defaultValue={userId} className="h-9 max-w-xs rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30">
                <option value="">All users</option>
                {users.map((u) => <option key={u.id} value={u.id}>{u.firstName} {u.lastName}</option>)}
              </select>
              <button type="submit" className="h-9 rounded-md border px-3 text-sm hover:bg-accent">Filter</button>
            </form>
            <div className="overflow-hidden rounded-md border">
              <table className="w-full text-sm">
                <thead className="bg-muted/50 text-left">
                  <tr>
                    <SortableTh label="When" field="createdAt" currentSort={sort} preserve={preserve} />
                    <th className="px-3 py-2 font-medium">User</th>
                    <th className="px-3 py-2 text-right font-medium">Points</th>
                    <th className="px-3 py-2 font-medium">Reason</th>
                  </tr>
                </thead>
                <tbody>
                  {entries.items.length === 0 && (
                    <tr><td colSpan={4} className="px-3 py-12">
                      <div className="flex flex-col items-center gap-3 text-muted-foreground">
                        <ShibaMark className="h-10 w-10 opacity-60" />
                        <p>No loyalty activity matches this filter.</p>
                      </div>
                    </td></tr>
                  )}
                  {entries.items.map((e) => (
                    <tr key={e.id} className="border-t">
                      <td className="px-3 py-2 text-muted-foreground">{new Date(e.createdAt).toLocaleString()}</td>
                      <td className="px-3 py-2">{userName(e.userId)}</td>
                      <td className="px-3 py-2 text-right">
                        <span className={"inline-flex rounded-full px-2.5 py-0.5 font-mono text-xs " + (e.points >= 0 ? "bg-primary/10 text-primary" : "bg-destructive/10 text-destructive")}>
                          {e.points >= 0 ? `+${e.points}` : e.points}
                        </span>
                      </td>
                      <td className="px-3 py-2 text-muted-foreground">{e.reason ?? "—"}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            <div className="flex items-center justify-between">
              <p className="text-sm text-muted-foreground">{entries.total} total</p>
              <div className="flex items-center gap-2">
                {page > 1 ? <Link href={{ query: { ...preserve, sort, page: page - 1 } }} className="inline-flex h-8 items-center rounded-md border px-3 text-sm hover:bg-accent">Previous</Link> : <span className="inline-flex h-8 items-center rounded-md border px-3 text-sm opacity-50">Previous</span>}
                <span className="text-sm text-muted-foreground">Page {page} of {totalPages}</span>
                {page < totalPages ? <Link href={{ query: { ...preserve, sort, page: page + 1 } }} className="inline-flex h-8 items-center rounded-md border px-3 text-sm hover:bg-accent">Next</Link> : <span className="inline-flex h-8 items-center rounded-md border px-3 text-sm opacity-50">Next</span>}
              </div>
            </div>
          </div>
        </section>

        <section className="rounded-lg border bg-card p-4">
          <h2 className="mb-4 text-base font-semibold">Manual adjustment</h2>
          <form action={recordAdjustment} className="space-y-4">
            <Field label="User">
              <select name="userId" required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30">
                <option value="">Pick a user…</option>
                {users.map((u) => <option key={u.id} value={u.id}>{u.firstName} {u.lastName} — {u.email}</option>)}
              </select>
            </Field>
            <Field label="Points">
              <input name="points" type="number" step="1" required className="h-9 w-32 rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
            </Field>
            <Field label="Reason">
              <input name="reason" required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
            </Field>
            <button type="submit" className="h-9 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90">Record adjustment</button>
          </form>
        </section>
      </div>
    </div>
  )
}
