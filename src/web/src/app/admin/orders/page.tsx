"use client"

import Link from "next/link"
import { useEffect, useState } from "react"
import { Eye } from "lucide-react"
import { toast } from "sonner"

import { ShibaMark } from "@/components/shiba-mark"
import { ApiError, api, type PagedResult } from "@/lib/api"

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

const STATUS_OPTIONS: { value: "" | OrderStatus; label: string }[] = [
  { value: "", label: "All statuses" },
  { value: "Placed", label: "Placed" },
  { value: "Closed", label: "Closed" },
  { value: "Cancelled", label: "Cancelled" },
]

function statusClass(status: OrderStatus): string {
  if (status === "Placed") return "bg-primary/10 text-primary"
  if (status === "Cancelled") return "bg-destructive/10 text-destructive"
  return "bg-secondary text-secondary-foreground"
}

export default function OrdersListPage() {
  const [page, setPage] = useState(1)
  const [status, setStatus] = useState<"" | OrderStatus>("")
  const [data, setData] = useState<PagedResult<OrderItem> | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    setLoading(true)
    const params = new URLSearchParams({ page: String(page), pageSize: String(PAGE_SIZE), sort: "-createdAt" })
    if (status) params.set("status", status)
    api.get<PagedResult<OrderItem>>(`/api/orders?${params}`)
      .then(setData)
      .catch((err) => { if (err instanceof ApiError) toast.error(err.message) })
      .finally(() => setLoading(false))
  }, [page, status])

  const totalPages = data ? Math.max(1, Math.ceil(data.total / PAGE_SIZE)) : 1

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Orders</h1>
        <p className="text-muted-foreground">All orders across outlets, newest first.</p>
      </div>

      <div className="flex items-center gap-3">
        <select
          value={status}
          onChange={(e) => { setPage(1); setStatus(e.target.value as "" | OrderStatus) }}
          className="h-9 max-w-xs rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30"
        >
          {STATUS_OPTIONS.map((o) => <option key={o.value} value={o.value}>{o.label}</option>)}
        </select>
      </div>

      <div className="overflow-hidden rounded-md border">
        <table className="w-full text-sm">
          <thead className="bg-muted/50 text-left">
            <tr>
              <th className="px-3 py-2 font-medium">Status</th>
              <th className="px-3 py-2 font-medium">Order ID</th>
              <th className="px-3 py-2 font-medium">Customer</th>
              <th className="px-3 py-2 font-medium">Created</th>
              <th className="px-3 py-2 font-medium">Closed</th>
              <th className="w-20 px-3 py-2" />
            </tr>
          </thead>
          <tbody>
            {loading && <tr><td colSpan={6} className="px-3 py-8 text-center text-muted-foreground">Loading…</td></tr>}
            {!loading && data?.items.length === 0 && (
              <tr><td colSpan={6} className="px-3 py-12">
                <div className="flex flex-col items-center gap-3 text-muted-foreground">
                  <ShibaMark className="h-10 w-10 opacity-60" />
                  <p>No orders match this filter.</p>
                </div>
              </td></tr>
            )}
            {!loading && data?.items.map((o) => (
              <tr key={o.id} className="border-t">
                <td className="px-3 py-2">
                  <span className={"inline-flex rounded-full px-2.5 py-0.5 text-xs " + statusClass(o.status)}>{o.status}</span>
                </td>
                <td className="px-3 py-2 font-mono text-xs">{o.id.slice(0, 8)}…</td>
                <td className="px-3 py-2 text-muted-foreground">{o.userId ? `${o.userId.slice(0, 8)}…` : "Walk-in"}</td>
                <td className="px-3 py-2 text-muted-foreground">{new Date(o.createdAt).toLocaleString()}</td>
                <td className="px-3 py-2 text-muted-foreground">{o.closedAt ? new Date(o.closedAt).toLocaleString() : "—"}</td>
                <td className="px-3 py-2">
                  <Link href={`/admin/orders/${o.id}`} className="inline-flex h-8 w-8 items-center justify-center rounded-md hover:bg-accent" aria-label="View order">
                    <Eye className="h-4 w-4" />
                  </Link>
                </td>
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
  )
}
