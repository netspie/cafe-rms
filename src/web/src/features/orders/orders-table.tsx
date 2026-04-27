"use client"

import Link from "next/link"
import { useState } from "react"
import { useQuery } from "@tanstack/react-query"
import { Eye } from "lucide-react"

import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { NativeSelect } from "@/components/ui/native-select"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import { Skeleton } from "@/components/ui/skeleton"
import { ShibaMark } from "@/components/shiba-mark"
import { ordersApi, type OrderStatus } from "./api"

const PAGE_SIZE = 20

const STATUS_OPTIONS: { value: "" | OrderStatus; label: string }[] = [
  { value: "", label: "All statuses" },
  { value: "Placed", label: "Placed" },
  { value: "Closed", label: "Closed" },
  { value: "Cancelled", label: "Cancelled" },
]

function statusVariant(status: OrderStatus): "default" | "secondary" | "destructive" {
  if (status === "Placed") return "default"
  if (status === "Cancelled") return "destructive"
  return "secondary"
}

export function OrdersTable() {
  const [page, setPage] = useState(1)
  const [status, setStatus] = useState<"" | OrderStatus>("")

  const query = useQuery({
    queryKey: ["orders", { page, status }],
    queryFn: () =>
      ordersApi.list({ page, pageSize: PAGE_SIZE, status: status || undefined, sort: "-createdAt" }),
  })

  const totalPages = query.data ? Math.max(1, Math.ceil(query.data.total / PAGE_SIZE)) : 1

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-3">
        <NativeSelect
          value={status}
          onChange={(e) => {
            setPage(1)
            setStatus(e.target.value as "" | OrderStatus)
          }}
          className="max-w-xs"
        >
          {STATUS_OPTIONS.map((o) => (
            <option key={o.value} value={o.value}>{o.label}</option>
          ))}
        </NativeSelect>
      </div>

      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Status</TableHead>
              <TableHead>Order ID</TableHead>
              <TableHead>Customer</TableHead>
              <TableHead>Created</TableHead>
              <TableHead>Closed</TableHead>
              <TableHead className="w-[60px]" />
            </TableRow>
          </TableHeader>
          <TableBody>
            {query.isLoading && (
              <>
                {[0, 1, 2].map((i) => (
                  <TableRow key={i}>
                    <TableCell><Skeleton className="h-4 w-16" /></TableCell>
                    <TableCell><Skeleton className="h-4 w-40" /></TableCell>
                    <TableCell><Skeleton className="h-4 w-24" /></TableCell>
                    <TableCell><Skeleton className="h-4 w-24" /></TableCell>
                    <TableCell><Skeleton className="h-4 w-24" /></TableCell>
                    <TableCell />
                  </TableRow>
                ))}
              </>
            )}
            {query.data?.items.length === 0 && (
              <TableRow>
                <TableCell colSpan={6} className="py-12">
                  <div className="flex flex-col items-center gap-3 text-muted-foreground">
                    <ShibaMark className="h-10 w-10 opacity-60" />
                    <p className="text-sm">No orders match this filter.</p>
                  </div>
                </TableCell>
              </TableRow>
            )}
            {query.data?.items.map((o) => (
              <TableRow key={o.id}>
                <TableCell>
                  <Badge variant={statusVariant(o.status)}>{o.status}</Badge>
                </TableCell>
                <TableCell className="font-mono text-xs">{o.id.slice(0, 8)}…</TableCell>
                <TableCell className="text-sm text-muted-foreground">
                  {o.userId ? `${o.userId.slice(0, 8)}…` : "Walk-in"}
                </TableCell>
                <TableCell className="text-sm text-muted-foreground">
                  {new Date(o.createdAt).toLocaleString()}
                </TableCell>
                <TableCell className="text-sm text-muted-foreground">
                  {o.closedAt ? new Date(o.closedAt).toLocaleString() : "—"}
                </TableCell>
                <TableCell>
                  <Button
                    variant="ghost"
                    size="icon"
                    className="h-8 w-8"
                    render={<Link href={`/admin/orders/${o.id}`} />}
                    nativeButton={false}
                    aria-label="View order"
                  >
                    <Eye className="h-4 w-4" />
                  </Button>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>

      <div className="flex items-center justify-between">
        <p className="text-sm text-muted-foreground">
          {query.data ? `${query.data.total} total` : ""}
        </p>
        <div className="flex items-center gap-2">
          <Button
            variant="outline"
            size="sm"
            onClick={() => setPage((p) => Math.max(1, p - 1))}
            disabled={page === 1 || query.isLoading}
          >
            Previous
          </Button>
          <span className="text-sm text-muted-foreground">
            Page {page} of {totalPages}
          </span>
          <Button
            variant="outline"
            size="sm"
            onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
            disabled={page >= totalPages || query.isLoading}
          >
            Next
          </Button>
        </div>
      </div>
    </div>
  )
}
