"use client"

import { use } from "react"
import { OrderDetail } from "@/features/orders/order-detail"

interface Props {
  params: Promise<{ id: string }>
}

export default function OrderDetailPage({ params }: Props) {
  const { id } = use(params)
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Order</h1>
        <p className="text-muted-foreground">Lines, totals, and lifecycle actions.</p>
      </div>
      <OrderDetail id={id} />
    </div>
  )
}
