import { OrdersTable } from "@/features/orders/orders-table"

export default function OrdersListPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Orders</h1>
        <p className="text-muted-foreground">All orders across outlets, newest first.</p>
      </div>
      <OrdersTable />
    </div>
  )
}
