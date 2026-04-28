"use client"

// Order detail. Shows status badge + header timestamps, lines table with
// totals, and Close / Cancel buttons (only when the order is still Placed).
// Cancel takes an optional reason via inline modal.

import { use, useEffect, useState } from "react"
import { useRouter } from "next/navigation"
import { toast } from "sonner"

import { ApiError, api, type PagedResult } from "@/lib/api"

type OrderStatus = "Placed" | "Closed" | "Cancelled"

interface OrderLine {
  id: string
  productId: string
  quantity: number
  netPerOne: number
  vatPerOne: number
}

interface OrderDetail {
  id: string
  outletId: string
  status: OrderStatus
  closedAt: string | null
  cancelledAt: string | null
  cancellationReason: string | null
  discount: number
  loyaltyPointsUsed: number
  lines: OrderLine[]
  createdAt: string
}

interface ProductRow { id: string; name: string }

function statusClass(status: OrderStatus): string {
  if (status === "Placed") return "bg-primary/10 text-primary"
  if (status === "Cancelled") return "bg-destructive/10 text-destructive"
  return "bg-secondary text-secondary-foreground"
}

export default function OrderDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params)
  const router = useRouter()

  const [order, setOrder] = useState<OrderDetail | null>(null)
  const [products, setProducts] = useState<ProductRow[]>([])
  const [loading, setLoading] = useState(true)
  const [refreshKey, setRefreshKey] = useState(0)
  const [showCancel, setShowCancel] = useState(false)
  const [cancelReason, setCancelReason] = useState("")
  const [closing, setClosing] = useState(false)
  const [cancelling, setCancelling] = useState(false)

  useEffect(() => {
    async function load() {
      try {
        const [o, prods] = await Promise.all([
          api.get<OrderDetail>(`/api/orders/${id}`),
          api.get<PagedResult<ProductRow>>(`/api/products?page=1&pageSize=200&sort=name`),
        ])
        setOrder(o)
        setProducts(prods.items)
      } catch (err) {
        if (err instanceof ApiError) toast.error(err.message)
      } finally { setLoading(false) }
    }
    load()
  }, [id, refreshKey])

  if (loading || !order) return <p className="text-sm text-muted-foreground">Loading…</p>

  const productName = (productId: string) => products.find((p) => p.id === productId)?.name ?? `${productId.slice(0, 8)}…`
  const isPlaced = order.status === "Placed"
  const subtotal = order.lines.reduce((s, l) => s + l.quantity * (l.netPerOne + l.vatPerOne), 0)
  const total = Math.max(0, subtotal - order.discount - order.loyaltyPointsUsed)

  async function handleClose() {
    setClosing(true)
    try {
      await api.post(`/api/orders/${id}/close`)
      toast.success("Order closed")
      setRefreshKey((k) => k + 1)
    } catch (err) {
      if (err instanceof ApiError) toast.error(err.message)
    } finally { setClosing(false) }
  }

  async function handleCancel() {
    setCancelling(true)
    try {
      await api.post(`/api/orders/${id}/cancel`, { reason: cancelReason || null })
      toast.success("Order cancelled")
      setShowCancel(false)
      setCancelReason("")
      setRefreshKey((k) => k + 1)
    } catch (err) {
      if (err instanceof ApiError) toast.error(err.message)
    } finally { setCancelling(false) }
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Order</h1>
        <p className="text-muted-foreground">Lines, totals, and lifecycle actions.</p>
      </div>

      <div className="flex items-start justify-between gap-4">
        <div>
          <div className="flex items-center gap-3">
            <span className={"inline-flex rounded-full px-2.5 py-0.5 text-sm " + statusClass(order.status)}>{order.status}</span>
            <span className="font-mono text-xs text-muted-foreground">{order.id}</span>
          </div>
          <p className="mt-1 text-sm text-muted-foreground">
            Placed {new Date(order.createdAt).toLocaleString()}
            {order.closedAt && ` · Closed ${new Date(order.closedAt).toLocaleString()}`}
            {order.cancelledAt && ` · Cancelled ${new Date(order.cancelledAt).toLocaleString()}`}
          </p>
        </div>
        {isPlaced && (
          <div className="flex gap-2">
            <button
              onClick={handleClose}
              disabled={closing}
              className="h-9 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90 disabled:opacity-50"
            >
              {closing ? "Closing…" : "Close order"}
            </button>
            <button
              onClick={() => setShowCancel(true)}
              className="h-9 rounded-md bg-destructive px-3 text-sm font-medium text-destructive-foreground hover:opacity-90"
            >
              Cancel order
            </button>
          </div>
        )}
      </div>

      <section className="overflow-hidden rounded-md border bg-card">
        <h2 className="border-b px-4 py-3 text-base font-semibold">Lines</h2>
        <table className="w-full text-sm">
          <thead className="bg-muted/50 text-left">
            <tr>
              <th className="px-3 py-2 font-medium">Product</th>
              <th className="px-3 py-2 text-right font-medium">Qty</th>
              <th className="px-3 py-2 text-right font-medium">Net / unit</th>
              <th className="px-3 py-2 text-right font-medium">VAT / unit</th>
              <th className="px-3 py-2 text-right font-medium">Line total</th>
            </tr>
          </thead>
          <tbody>
            {order.lines.map((l) => (
              <tr key={l.id} className="border-t">
                <td className="px-3 py-2">{productName(l.productId)}</td>
                <td className="px-3 py-2 text-right font-mono">{l.quantity}</td>
                <td className="px-3 py-2 text-right font-mono">{l.netPerOne.toFixed(2)}</td>
                <td className="px-3 py-2 text-right font-mono">{l.vatPerOne.toFixed(2)}</td>
                <td className="px-3 py-2 text-right font-mono">{(l.quantity * (l.netPerOne + l.vatPerOne)).toFixed(2)}</td>
              </tr>
            ))}
            <tr className="border-t">
              <td colSpan={4} className="px-3 py-2 text-right font-medium">Subtotal</td>
              <td className="px-3 py-2 text-right font-mono font-medium">{subtotal.toFixed(2)}</td>
            </tr>
            {order.discount > 0 && (
              <tr>
                <td colSpan={4} className="px-3 py-2 text-right text-muted-foreground">Discount</td>
                <td className="px-3 py-2 text-right font-mono text-muted-foreground">-{order.discount.toFixed(2)}</td>
              </tr>
            )}
            {order.loyaltyPointsUsed > 0 && (
              <tr>
                <td colSpan={4} className="px-3 py-2 text-right text-muted-foreground">Loyalty points</td>
                <td className="px-3 py-2 text-right font-mono text-muted-foreground">-{order.loyaltyPointsUsed}</td>
              </tr>
            )}
            <tr className="border-t">
              <td colSpan={4} className="px-3 py-2 text-right font-semibold">Total</td>
              <td className="px-3 py-2 text-right font-mono font-semibold">{total.toFixed(2)}</td>
            </tr>
          </tbody>
        </table>
      </section>

      {order.cancellationReason && (
        <section className="rounded-lg border bg-card p-4">
          <h2 className="mb-2 text-base font-semibold">Cancellation reason</h2>
          <p className="text-sm text-muted-foreground">{order.cancellationReason}</p>
        </section>
      )}

      <button type="button" onClick={() => router.push("/admin/orders")} className="h-9 rounded-md px-3 text-sm hover:bg-accent">
        Back to list
      </button>

      {showCancel && (
        <Modal title="Cancel this order?" onClose={() => setShowCancel(false)}>
          <p className="text-sm text-muted-foreground">Optional reason — appears on the order record afterwards.</p>
          <input
            value={cancelReason}
            onChange={(e) => setCancelReason(e.target.value)}
            placeholder="Reason (optional)"
            className="mt-4 h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30"
          />
          <div className="mt-6 flex justify-end gap-2">
            <button onClick={() => setShowCancel(false)} className="h-9 rounded-md px-3 text-sm hover:bg-accent">Keep order</button>
            <button
              onClick={handleCancel}
              disabled={cancelling}
              className="h-9 rounded-md bg-destructive px-3 text-sm font-medium text-destructive-foreground hover:opacity-90 disabled:opacity-50"
            >
              {cancelling ? "Cancelling…" : "Cancel order"}
            </button>
          </div>
        </Modal>
      )}
    </div>
  )
}

function Modal({ title, onClose, children }: { title: string; onClose: () => void; children: React.ReactNode }) {
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4" onClick={onClose}>
      <div onClick={(e) => e.stopPropagation()} className="w-full max-w-md rounded-lg border bg-card p-6 shadow-lg">
        <h2 className="text-lg font-semibold">{title}</h2>
        <div className="mt-4">{children}</div>
      </div>
    </div>
  )
}
