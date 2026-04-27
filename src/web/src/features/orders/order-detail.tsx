"use client"

import { useState } from "react"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { toast } from "sonner"

import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog"
import { Input } from "@/components/ui/input"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import { Skeleton } from "@/components/ui/skeleton"
import { ApiError } from "@/lib/api"
import { productsApi } from "@/features/products/api"
import { ordersApi, type OrderStatus } from "./api"

interface Props {
  id: string
}

function statusVariant(status: OrderStatus): "default" | "secondary" | "destructive" {
  if (status === "Placed") return "default"
  if (status === "Cancelled") return "destructive"
  return "secondary"
}

export function OrderDetail({ id }: Props) {
  const queryClient = useQueryClient()
  const [confirmCancel, setConfirmCancel] = useState(false)
  const [cancelReason, setCancelReason] = useState("")

  const orderQuery = useQuery({
    queryKey: ["order", id],
    queryFn: () => ordersApi.get(id),
  })

  const productsQuery = useQuery({
    queryKey: ["products", "lookup"],
    queryFn: () => productsApi.list({ page: 1, pageSize: 200, sort: "name" }),
  })

  const productName = (productId: string) =>
    productsQuery.data?.items.find((p) => p.id === productId)?.name ?? productId.slice(0, 8) + "…"

  const refresh = () => queryClient.invalidateQueries({ queryKey: ["order", id] })

  const closeMutation = useMutation({
    mutationFn: () => ordersApi.close(id),
    onSuccess: () => {
      toast.success("Order closed")
      refresh()
    },
    onError: (err) => toast.error(err instanceof ApiError ? err.message : "Could not close order"),
  })

  const cancelMutation = useMutation({
    mutationFn: () => ordersApi.cancel(id, cancelReason || null),
    onSuccess: () => {
      toast.success("Order cancelled")
      setConfirmCancel(false)
      setCancelReason("")
      refresh()
    },
    onError: (err) => toast.error(err instanceof ApiError ? err.message : "Could not cancel order"),
  })

  if (orderQuery.isLoading) return <Skeleton className="h-64 w-full max-w-3xl" />
  if (!orderQuery.data) return <p className="text-sm text-muted-foreground">Order not found.</p>

  const order = orderQuery.data
  const isPlaced = order.status === "Placed"
  const subtotal = order.lines.reduce(
    (sum, l) => sum + l.quantity * (l.netPerOne + l.vatPerOne),
    0,
  )

  return (
    <div className="space-y-6">
      <div className="flex items-start justify-between gap-4">
        <div>
          <div className="flex items-center gap-3">
            <Badge variant={statusVariant(order.status)} className="text-sm">{order.status}</Badge>
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
            <Button
              variant="default"
              onClick={() => closeMutation.mutate()}
              disabled={closeMutation.isPending}
            >
              {closeMutation.isPending ? "Closing…" : "Close order"}
            </Button>
            <Button variant="destructive" onClick={() => setConfirmCancel(true)}>
              Cancel order
            </Button>
          </div>
        )}
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="text-base">Lines</CardTitle>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Product</TableHead>
                <TableHead className="text-right">Qty</TableHead>
                <TableHead className="text-right">Net / unit</TableHead>
                <TableHead className="text-right">VAT / unit</TableHead>
                <TableHead className="text-right">Line total</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {order.lines.map((l) => (
                <TableRow key={l.id}>
                  <TableCell>{productName(l.productId)}</TableCell>
                  <TableCell className="text-right font-mono">{l.quantity}</TableCell>
                  <TableCell className="text-right font-mono">{l.netPerOne.toFixed(2)}</TableCell>
                  <TableCell className="text-right font-mono">{l.vatPerOne.toFixed(2)}</TableCell>
                  <TableCell className="text-right font-mono">
                    {(l.quantity * (l.netPerOne + l.vatPerOne)).toFixed(2)}
                  </TableCell>
                </TableRow>
              ))}
              <TableRow>
                <TableCell colSpan={4} className="text-right font-medium">Subtotal</TableCell>
                <TableCell className="text-right font-mono font-medium">{subtotal.toFixed(2)}</TableCell>
              </TableRow>
              {order.discount > 0 && (
                <TableRow>
                  <TableCell colSpan={4} className="text-right text-muted-foreground">Discount</TableCell>
                  <TableCell className="text-right font-mono text-muted-foreground">
                    -{order.discount.toFixed(2)}
                  </TableCell>
                </TableRow>
              )}
              {order.loyaltyPointsUsed > 0 && (
                <TableRow>
                  <TableCell colSpan={4} className="text-right text-muted-foreground">Loyalty points</TableCell>
                  <TableCell className="text-right font-mono text-muted-foreground">
                    -{order.loyaltyPointsUsed}
                  </TableCell>
                </TableRow>
              )}
              <TableRow>
                <TableCell colSpan={4} className="text-right font-semibold">Total</TableCell>
                <TableCell className="text-right font-mono font-semibold">
                  {Math.max(0, subtotal - order.discount - order.loyaltyPointsUsed).toFixed(2)}
                </TableCell>
              </TableRow>
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      {order.cancellationReason && (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Cancellation reason</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-sm text-muted-foreground">{order.cancellationReason}</p>
          </CardContent>
        </Card>
      )}

      <Dialog open={confirmCancel} onOpenChange={setConfirmCancel}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Cancel this order?</DialogTitle>
            <DialogDescription>
              Optional reason — appears on the order record afterwards.
            </DialogDescription>
          </DialogHeader>
          <Input
            placeholder="Reason (optional)"
            value={cancelReason}
            onChange={(e) => setCancelReason(e.target.value)}
          />
          <DialogFooter>
            <Button variant="ghost" onClick={() => setConfirmCancel(false)}>Keep order</Button>
            <Button
              variant="destructive"
              onClick={() => cancelMutation.mutate()}
              disabled={cancelMutation.isPending}
            >
              {cancelMutation.isPending ? "Cancelling…" : "Cancel order"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  )
}
