import { revalidatePath } from "next/cache"
import { api, type PagedResult } from "@/lib/server-api"
import { requirePermissionFor } from "@/features/auth/access"

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
  tableId: string | null
  tableName: string | null
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

export default async function OrderDetailPage({ params }: { params: Promise<{ id: string }> }) {
  await requirePermissionFor("/admin/orders")
  const { id } = await params
  const [order, products] = await Promise.all([
    api.get<OrderDetail>(`/api/orders/${id}`),
    api.get<PagedResult<ProductRow>>(`/api/products?page=1&pageSize=200&sort=name`),
  ])
  const productName = (productId: string) => products.items.find((p) => p.id === productId)?.name ?? `${productId.slice(0, 8)}…`
  const isPlaced = order.status === "Placed"
  const subtotal = order.lines.reduce((s, l) => s + l.quantity * (l.netPerOne + l.vatPerOne), 0)
  const total = Math.max(0, subtotal - order.discount - order.loyaltyPointsUsed)

  async function closeOrder() {
    "use server"
    await api.post(`/api/orders/${id}/close`)
    revalidatePath(`/admin/orders/${id}`)
  }

  async function cancelOrder(formData: FormData) {
    "use server"
    const reason = (formData.get("reason") as string) || null
    await api.post(`/api/orders/${id}/cancel`, { reason })
    revalidatePath(`/admin/orders/${id}`)
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
            {order.tableName && <span className="inline-flex rounded-full bg-secondary px-2.5 py-0.5 text-sm text-secondary-foreground">Table {order.tableName}</span>}
            <span className="font-mono text-xs text-muted-foreground">{order.id}</span>
          </div>
          <p className="mt-1 text-sm text-muted-foreground">
            Placed {new Date(order.createdAt).toLocaleString()}
            {order.closedAt && `, Closed ${new Date(order.closedAt).toLocaleString()}`}
            {order.cancelledAt && `, Cancelled ${new Date(order.cancelledAt).toLocaleString()}`}
          </p>
        </div>
        {isPlaced && (
          <div className="flex gap-2">
            <form action={closeOrder}>
              <button type="submit" className="h-9 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90">Close order</button>
            </form>
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

      {isPlaced && (
        <section className="rounded-lg border bg-card p-4">
          <h2 className="mb-2 text-base font-semibold">Cancel this order</h2>
          <p className="mb-4 text-sm text-muted-foreground">Optional reason — appears on the order record afterwards.</p>
          <form action={cancelOrder} className="flex gap-2">
            <input name="reason" className="h-9 max-w-md flex-1 rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
            <button type="submit" className="h-9 rounded-md bg-destructive px-3 text-sm font-medium text-destructive-foreground hover:opacity-90">Cancel order</button>
          </form>
        </section>
      )}

      {order.cancellationReason && (
        <section className="rounded-lg border bg-card p-4">
          <h2 className="mb-2 text-base font-semibold">Cancellation reason</h2>
          <p className="text-sm text-muted-foreground">{order.cancellationReason}</p>
        </section>
      )}
    </div>
  )
}
