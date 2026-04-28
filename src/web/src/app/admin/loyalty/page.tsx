"use client"

// Loyalty — single page. Two inline sections: activity log (paged, filterable
// by user) and a manual adjustment form (positive = bonus, negative = withdrawal).

import { useEffect, useState } from "react"
import { toast } from "sonner"

import { Field } from "@/components/field"
import { ShibaMark } from "@/components/shiba-mark"
import { ApiError, api, type PagedResult } from "@/lib/api"

interface LoyaltyEntry { id: string; userId: string; points: number; reason: string | null; createdAt: string }
interface UserRow { id: string; firstName: string; lastName: string; email: string }

const PAGE_SIZE = 20

export default function LoyaltyPage() {
  const [users, setUsers] = useState<UserRow[]>([])

  useEffect(() => {
    api.get<UserRow[]>("/api/users")
      .then(setUsers)
      .catch(() => {})
  }, [])

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Loyalty</h1>
        <p className="text-muted-foreground">Point activity across customers — and manual adjustments.</p>
      </div>

      <div className="grid gap-6 lg:grid-cols-3">
        <div className="lg:col-span-2">
          <ActivitySection users={users} />
        </div>
        <AdjustmentSection users={users} />
      </div>
    </div>
  )
}

function ActivitySection({ users }: { users: UserRow[] }) {
  const [page, setPage] = useState(1)
  const [userId, setUserId] = useState("")
  const [data, setData] = useState<PagedResult<LoyaltyEntry> | null>(null)
  const [loading, setLoading] = useState(true)
  const [refreshKey, setRefreshKey] = useState(0)

  useEffect(() => {
    setLoading(true)
    const params = new URLSearchParams({ page: String(page), pageSize: String(PAGE_SIZE), sort: "-createdAt" })
    if (userId) params.set("userId", userId)
    api.get<PagedResult<LoyaltyEntry>>(`/api/loyalty/entries?${params}`)
      .then(setData)
      .catch((err) => { if (err instanceof ApiError) toast.error(err.message) })
      .finally(() => setLoading(false))
  }, [page, userId, refreshKey])

  const userName = (id: string) => {
    const u = users.find((x) => x.id === id)
    return u ? `${u.firstName} ${u.lastName}` : `${id.slice(0, 8)}…`
  }
  const totalPages = data ? Math.max(1, Math.ceil(data.total / PAGE_SIZE)) : 1

  return (
    <section className="rounded-lg border bg-card p-4">
      <h2 className="text-base font-semibold">Activity</h2>
      <p className="mb-4 text-xs text-muted-foreground">Latest first. Filter by customer to audit a single account.</p>

      <div className="space-y-4">
        <select value={userId} onChange={(e) => { setPage(1); setUserId(e.target.value) }} className="h-9 max-w-xs rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30">
          <option value="">All users</option>
          {users.map((u) => <option key={u.id} value={u.id}>{u.firstName} {u.lastName}</option>)}
        </select>

        <div className="overflow-hidden rounded-md border">
          <table className="w-full text-sm">
            <thead className="bg-muted/50 text-left">
              <tr>
                <th className="px-3 py-2 font-medium">When</th>
                <th className="px-3 py-2 font-medium">User</th>
                <th className="px-3 py-2 text-right font-medium">Points</th>
                <th className="px-3 py-2 font-medium">Reason</th>
              </tr>
            </thead>
            <tbody>
              {loading && <tr><td colSpan={4} className="px-3 py-8 text-center text-muted-foreground">Loading…</td></tr>}
              {!loading && data?.items.length === 0 && (
                <tr><td colSpan={4} className="px-3 py-12">
                  <div className="flex flex-col items-center gap-3 text-muted-foreground">
                    <ShibaMark className="h-10 w-10 opacity-60" />
                    <p>No loyalty activity matches this filter.</p>
                  </div>
                </td></tr>
              )}
              {!loading && data?.items.map((e) => (
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
          <p className="text-sm text-muted-foreground">{data ? `${data.total} total` : ""}</p>
          <div className="flex items-center gap-2">
            <button onClick={() => setPage((p) => Math.max(1, p - 1))} disabled={page === 1 || loading} className="h-8 rounded-md border px-3 text-sm hover:bg-accent disabled:opacity-50">Previous</button>
            <span className="text-sm text-muted-foreground">Page {page} of {totalPages}</span>
            <button onClick={() => setPage((p) => Math.min(totalPages, p + 1))} disabled={page >= totalPages || loading} className="h-8 rounded-md border px-3 text-sm hover:bg-accent disabled:opacity-50">Next</button>
          </div>
        </div>
      </div>
    </section>
  )
}

function AdjustmentSection({ users }: { users: UserRow[] }) {
  const [userId, setUserId] = useState("")
  const [points, setPoints] = useState("")
  const [reason, setReason] = useState("")
  const [submitting, setSubmitting] = useState(false)
  const [errors, setErrors] = useState<Record<string, string>>({})

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setSubmitting(true)
    setErrors({})
    try {
      await api.post("/api/loyalty/entries", { userId, points: Number(points), reason })
      toast.success("Adjustment recorded")
      setUserId("")
      setPoints("")
      setReason("")
    } catch (err) {
      if (err instanceof ApiError) {
        if (err.problem.errors) {
          const flat: Record<string, string> = {}
          for (const [k, v] of Object.entries(err.problem.errors)) flat[k.toLowerCase()] = v[0]
          setErrors(flat)
        } else toast.error(err.message)
      }
    } finally { setSubmitting(false) }
  }

  return (
    <section className="rounded-lg border bg-card p-4">
      <h2 className="text-base font-semibold">Manual adjustment</h2>
      <p className="mb-4 text-xs text-muted-foreground">Bonus or withdrawal — leaves an entry in the activity log.</p>
      <form onSubmit={handleSubmit} className="space-y-4">
        <Field label="User" error={errors.userid}>
          <select value={userId} onChange={(e) => setUserId(e.target.value)} required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30">
            <option value="">Pick a user…</option>
            {users.map((u) => <option key={u.id} value={u.id}>{u.firstName} {u.lastName} — {u.email}</option>)}
          </select>
        </Field>
        <Field label="Points" hint="Positive for a bonus, negative to withdraw." error={errors.points}>
          <input type="number" step="1" value={points} onChange={(e) => setPoints(e.target.value)} required className="h-9 w-32 rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <Field label="Reason" error={errors.reason}>
          <input value={reason} onChange={(e) => setReason(e.target.value)} required placeholder="e.g. complaint resolution, birthday gift" className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <button type="submit" disabled={submitting} className="h-9 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90 disabled:opacity-50">
          {submitting ? "Saving…" : "Record adjustment"}
        </button>
      </form>
    </section>
  )
}
