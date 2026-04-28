"use client"

import Link from "next/link"
import { useState } from "react"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { MoreHorizontal, Pencil, Plus, Trash2 } from "lucide-react"
import { toast } from "sonner"

import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from "@/components/ui/dropdown-menu"
import { Skeleton } from "@/components/ui/skeleton"
import { ApiError } from "@/lib/api"
import { ShibaMark } from "@/components/shiba-mark"
import { salesChannelsApi } from "./api"

const PAGE_SIZE = 20

export function SalesChannelsTable() {
  const queryClient = useQueryClient()
  const [page, setPage] = useState(1)
  const [filter, setFilter] = useState("")
  const [confirmDelete, setConfirmDelete] = useState<{ id: string; name: string } | null>(null)

  const query = useQuery({
    queryKey: ["sales-channels", { page, filter }],
    queryFn: () => salesChannelsApi.list({ page, pageSize: PAGE_SIZE, name: filter || undefined, sort: "name" }),
  })

  const deleteMutation = useMutation({
    mutationFn: (id: string) => salesChannelsApi.remove(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["sales-channels"] })
      toast.success("Sales channel deleted")
      setConfirmDelete(null)
    },
    onError: (err) => toast.error(err instanceof ApiError ? err.message : "Could not delete sales channel"),
  })

  const totalPages = query.data ? Math.max(1, Math.ceil(query.data.total / PAGE_SIZE)) : 1

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-3">
        <Input placeholder="Filter by name…" value={filter} onChange={(e) => { setPage(1); setFilter(e.target.value) }} className="max-w-xs" />
        <div className="flex-1" />
        <Button render={<Link href="/admin/sales-channels/new" />} nativeButton={false}>
          <Plus className="mr-2 h-4 w-4" />New sales channel
        </Button>
      </div>
      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Name</TableHead>
              <TableHead>Type</TableHead>
              <TableHead>Created</TableHead>
              <TableHead className="w-[60px]" />
            </TableRow>
          </TableHeader>
          <TableBody>
            {query.isLoading && [0, 1, 2].map((i) => (
              <TableRow key={i}>
                <TableCell><Skeleton className="h-4 w-32" /></TableCell>
                <TableCell><Skeleton className="h-4 w-16" /></TableCell>
                <TableCell><Skeleton className="h-4 w-24" /></TableCell>
                <TableCell />
              </TableRow>
            ))}
            {query.data?.items.length === 0 && (
              <TableRow>
                <TableCell colSpan={4} className="py-12">
                  <div className="flex flex-col items-center gap-3 text-muted-foreground">
                    <ShibaMark className="h-10 w-10 opacity-60" />
                    <p className="text-sm">No sales channels yet.</p>
                  </div>
                </TableCell>
              </TableRow>
            )}
            {query.data?.items.map((c) => (
              <TableRow key={c.id}>
                <TableCell className="font-medium">{c.name}</TableCell>
                <TableCell>
                  <Badge variant={c.isTakeout ? "default" : "secondary"}>
                    {c.isTakeout ? "Takeout" : "Dine-in"}
                  </Badge>
                </TableCell>
                <TableCell className="text-sm text-muted-foreground">{new Date(c.createdAt).toLocaleDateString()}</TableCell>
                <TableCell>
                  <DropdownMenu>
                    <DropdownMenuTrigger render={<Button variant="ghost" size="icon" className="h-8 w-8"><MoreHorizontal className="h-4 w-4" /></Button>} />
                    <DropdownMenuContent align="end">
                      <DropdownMenuItem render={<Link href={`/admin/sales-channels/${c.id}`} />}>
                        <Pencil className="mr-2 h-4 w-4" />Edit
                      </DropdownMenuItem>
                      <DropdownMenuItem className="text-destructive focus:text-destructive" onClick={() => setConfirmDelete({ id: c.id, name: c.name })}>
                        <Trash2 className="mr-2 h-4 w-4" />Delete
                      </DropdownMenuItem>
                    </DropdownMenuContent>
                  </DropdownMenu>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>
      <div className="flex items-center justify-between">
        <p className="text-sm text-muted-foreground">{query.data ? `${query.data.total} total` : ""}</p>
        <div className="flex items-center gap-2">
          <Button variant="outline" size="sm" onClick={() => setPage((p) => Math.max(1, p - 1))} disabled={page === 1 || query.isLoading}>Previous</Button>
          <span className="text-sm text-muted-foreground">Page {page} of {totalPages}</span>
          <Button variant="outline" size="sm" onClick={() => setPage((p) => Math.min(totalPages, p + 1))} disabled={page >= totalPages || query.isLoading}>Next</Button>
        </div>
      </div>
      <Dialog open={!!confirmDelete} onOpenChange={(open) => !open && setConfirmDelete(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Delete sales channel?</DialogTitle>
            <DialogDescription>Removes &ldquo;{confirmDelete?.name}&rdquo;. Past orders keep the channel reference.</DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="ghost" onClick={() => setConfirmDelete(null)}>Cancel</Button>
            <Button variant="destructive" onClick={() => confirmDelete && deleteMutation.mutate(confirmDelete.id)} disabled={deleteMutation.isPending}>
              {deleteMutation.isPending ? "Deleting…" : "Delete"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  )
}
