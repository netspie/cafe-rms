import Link from "next/link"
import { Eye } from "lucide-react"
import { ShibaMark } from "@/components/shiba-mark"
import { SortableTh } from "@/components/sortable-th"
import { api, type PagedResult } from "@/lib/server-api"

type OrderStatus = "Placed" | "Closed" | "Cancelled"

interface OrderItem {
  id: string
  outletId: string
  userId: string | null
  status: OrderStatus
  createdAt: string
  closedAt: string | null
}

const PAGE_SIZE = 20
const STATUSES: ("" | OrderStatus)[] = ["", "Placed", "Closed", "Cancelled"]

function statusClass(status: OrderStatus): string {
  if (status === "Placed") return "bg-primary/10 text-primary"
  if (status === "Cancelled") return "bg-destructive/10 text-destructive"
  return "bg-secondary text-secondary-foreground"
}

export default async function OrdersListPage({ searchParams }: { searchParams: Promise<{ page?: string; status?: string; sort?: string }> }) {
  const sp = await searchParams
  const page = Number(sp.page ?? 1)
  const status = (sp.status as OrderStatus | "") ?? ""
  const sort = sp.sort ?? "-createdAt"

  const search = new URLSearchParams({ page: String(page), pageSize: String(PAGE_SIZE), sort })
  if (status) search.set("status", status)
  const data = await api.get<PagedResult<OrderItem>>(`/api/orders?${search}`)
  const totalPages = Math.max(1, Math.ceil(data.total / PAGE_SIZE))
  const preserve: Record<string, string> = {}
  if (status) preserve.status = status

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Orders</h1>
        <p className="text-muted-foreground">All orders across outlets, newest first.</p>
      </div>

      <form className="flex items-center gap-3">
        <select name="status" defaultValue={status} className="h-9 max-w-xs rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30">
          {STATUSES.map((s) => <option key={s} value={s}>{s || "All statuses"}</option>)}
        </select>
        <button type="submit" className="h-9 rounded-md border px-3 text-sm hover:bg-accent">Filter</button>
      </form>

      <div className="overflow-hidden rounded-md border">
        <table className="w-full text-sm">
          <thead className="bg-muted/50 text-left">
            <tr>
              <th className="px-3 py-2 font-medium">Status</th>
              <th className="px-3 py-2 font-medium">Order ID</th>
              <th className="px-3 py-2 font-medium">Customer</th>
              <SortableTh label="Created" field="createdAt" currentSort={sort} preserve={preserve} />
              <th className="px-3 py-2 font-medium">Closed</th>
              <th className="w-20 px-3 py-2" />
            </tr>
          </thead>
          <tbody>
            {data.items.length === 0 && (
              <tr><td colSpan={6} className="px-3 py-12">
                <div className="flex flex-col items-center gap-3 text-muted-foreground">
                  <ShibaMark className="h-10 w-10 opacity-60" />
                  <p>No orders match this filter.</p>
                </div>
              </td></tr>
            )}
            {data.items.map((o) => (
              <tr key={o.id} className="border-t">
                <td className="px-3 py-2"><span className={"inline-flex rounded-full px-2.5 py-0.5 text-xs " + statusClass(o.status)}>{o.status}</span></td>
                <td className="px-3 py-2 font-mono text-xs">{o.id.slice(0, 8)}…</td>
                <td className="px-3 py-2 text-muted-foreground">{o.userId ? `${o.userId.slice(0, 8)}…` : "Walk-in"}</td>
                <td className="px-3 py-2 text-muted-foreground">{new Date(o.createdAt).toLocaleString()}</td>
                <td className="px-3 py-2 text-muted-foreground">{o.closedAt ? new Date(o.closedAt).toLocaleString() : "—"}</td>
                <td className="px-3 py-2">
                  <Link href={`/admin/orders/${o.id}`} className="inline-flex h-8 w-8 items-center justify-center rounded-md hover:bg-accent" aria-label="View order"><Eye className="h-4 w-4" /></Link>
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
