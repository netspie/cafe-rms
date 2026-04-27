"use client"

import Link from "next/link"
import { useState } from "react"
import { useQuery } from "@tanstack/react-query"
import { Eye, Plus } from "lucide-react"

import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
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
import { ShibaMark } from "@/components/shiba-mark"
import { eventsApi, type EventStatus } from "./api"

const PAGE_SIZE = 20

function statusVariant(status: EventStatus): "default" | "secondary" | "destructive" | "outline" {
  if (status === "Published") return "default"
  if (status === "Cancelled") return "destructive"
  if (status === "Closed") return "secondary"
  return "outline"
}

export function EventsTable() {
  const [page, setPage] = useState(1)
  const [filter, setFilter] = useState("")

  const query = useQuery({
    queryKey: ["events", { page, filter }],
    queryFn: () =>
      eventsApi.list({ page, pageSize: PAGE_SIZE, name: filter || undefined, sort: "-createdAt" }),
  })

  const totalPages = query.data ? Math.max(1, Math.ceil(query.data.total / PAGE_SIZE)) : 1

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-3">
        <Input
          placeholder="Filter by name…"
          value={filter}
          onChange={(e) => {
            setPage(1)
            setFilter(e.target.value)
          }}
          className="max-w-xs"
        />
        <div className="flex-1" />
        <Button render={<Link href="/admin/events/new" />} nativeButton={false}>
          <Plus className="mr-2 h-4 w-4" />
          New event
        </Button>
      </div>

      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Status</TableHead>
              <TableHead>Name</TableHead>
              <TableHead>Created</TableHead>
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
                    <TableCell />
                  </TableRow>
                ))}
              </>
            )}
            {query.data?.items.length === 0 && (
              <TableRow>
                <TableCell colSpan={4} className="py-12">
                  <div className="flex flex-col items-center gap-3 text-muted-foreground">
                    <ShibaMark className="h-10 w-10 opacity-60" />
                    <p className="text-sm">No events yet — let&apos;s plan one.</p>
                  </div>
                </TableCell>
              </TableRow>
            )}
            {query.data?.items.map((e) => (
              <TableRow key={e.id}>
                <TableCell>
                  <Badge variant={statusVariant(e.status)}>{e.status}</Badge>
                </TableCell>
                <TableCell className="font-medium">{e.name}</TableCell>
                <TableCell className="text-sm text-muted-foreground">
                  {new Date(e.createdAt).toLocaleDateString()}
                </TableCell>
                <TableCell>
                  <Button
                    variant="ghost"
                    size="icon"
                    className="h-8 w-8"
                    render={<Link href={`/admin/events/${e.id}`} />}
                    nativeButton={false}
                    aria-label="Open event"
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
