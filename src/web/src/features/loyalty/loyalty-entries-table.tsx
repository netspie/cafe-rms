"use client"

import { useState } from "react"
import { useQuery } from "@tanstack/react-query"

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
import { usersApi } from "@/features/users/api"
import { loyaltyApi } from "./api"

const PAGE_SIZE = 20

export function LoyaltyEntriesTable() {
  const [page, setPage] = useState(1)
  const [userId, setUserId] = useState("")

  const usersQuery = useQuery({
    queryKey: ["users"],
    queryFn: () => usersApi.list(),
  })

  const query = useQuery({
    queryKey: ["loyalty-entries", { page, userId }],
    queryFn: () =>
      loyaltyApi.list({
        page,
        pageSize: PAGE_SIZE,
        userId: userId || undefined,
        sort: "-createdAt",
      }),
  })

  const userName = (id: string) => {
    const u = usersQuery.data?.find((u) => u.id === id)
    return u ? `${u.firstName} ${u.lastName}` : id.slice(0, 8) + "…"
  }

  const totalPages = query.data ? Math.max(1, Math.ceil(query.data.total / PAGE_SIZE)) : 1

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-3">
        <NativeSelect
          value={userId}
          onChange={(e) => {
            setPage(1)
            setUserId(e.target.value)
          }}
          className="max-w-xs"
        >
          <option value="">All users</option>
          {usersQuery.data?.map((u) => (
            <option key={u.id} value={u.id}>
              {u.firstName} {u.lastName}
            </option>
          ))}
        </NativeSelect>
      </div>

      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>When</TableHead>
              <TableHead>User</TableHead>
              <TableHead className="text-right">Points</TableHead>
              <TableHead>Reason</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {query.isLoading && (
              <>
                {[0, 1, 2].map((i) => (
                  <TableRow key={i}>
                    <TableCell><Skeleton className="h-4 w-24" /></TableCell>
                    <TableCell><Skeleton className="h-4 w-32" /></TableCell>
                    <TableCell><Skeleton className="h-4 w-12" /></TableCell>
                    <TableCell><Skeleton className="h-4 w-48" /></TableCell>
                  </TableRow>
                ))}
              </>
            )}
            {query.data?.items.length === 0 && (
              <TableRow>
                <TableCell colSpan={4} className="py-12">
                  <div className="flex flex-col items-center gap-3 text-muted-foreground">
                    <ShibaMark className="h-10 w-10 opacity-60" />
                    <p className="text-sm">No loyalty activity matches this filter.</p>
                  </div>
                </TableCell>
              </TableRow>
            )}
            {query.data?.items.map((e) => (
              <TableRow key={e.id}>
                <TableCell className="text-sm text-muted-foreground">
                  {new Date(e.createdAt).toLocaleString()}
                </TableCell>
                <TableCell className="text-sm">{userName(e.userId)}</TableCell>
                <TableCell className="text-right">
                  <Badge variant={e.points >= 0 ? "default" : "destructive"} className="font-mono">
                    {e.points >= 0 ? `+${e.points}` : e.points}
                  </Badge>
                </TableCell>
                <TableCell className="text-sm text-muted-foreground">{e.reason ?? "—"}</TableCell>
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
