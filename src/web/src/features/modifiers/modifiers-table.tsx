"use client"

import Link from "next/link"
import { useState } from "react"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { MoreHorizontal, Pencil, Plus, Trash2 } from "lucide-react"
import { toast } from "sonner"

import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { NativeSelect } from "@/components/ui/native-select"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from "@/components/ui/dropdown-menu"
import { Skeleton } from "@/components/ui/skeleton"
import { ApiError } from "@/lib/api"
import { ShibaMark } from "@/components/shiba-mark"
import { modifierGroupsApi } from "@/features/modifier-groups/api"
import { modifiersApi } from "./api"

const PAGE_SIZE = 20

export function ModifiersTable() {
  const queryClient = useQueryClient()
  const [page, setPage] = useState(1)
  const [filter, setFilter] = useState("")
  const [groupId, setGroupId] = useState("")
  const [confirmDelete, setConfirmDelete] = useState<{ id: string; name: string } | null>(null)

  const groupsQuery = useQuery({
    queryKey: ["modifier-groups", "lookup"],
    queryFn: () => modifierGroupsApi.list({ page: 1, pageSize: 100, sort: "name" }),
  })

  const query = useQuery({
    queryKey: ["modifiers", { page, filter, groupId }],
    queryFn: () => modifiersApi.list({ page, pageSize: PAGE_SIZE, name: filter || undefined, modifierGroupId: groupId || undefined, sort: "name" }),
  })

  const groupName = (id: string) => groupsQuery.data?.items.find((g) => g.id === id)?.name ?? "—"

  const deleteMutation = useMutation({
    mutationFn: (id: string) => modifiersApi.remove(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["modifiers"] })
      toast.success("Modifier deleted")
      setConfirmDelete(null)
    },
    onError: (err) => toast.error(err instanceof ApiError ? err.message : "Could not delete modifier"),
  })

  const totalPages = query.data ? Math.max(1, Math.ceil(query.data.total / PAGE_SIZE)) : 1

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-3">
        <Input placeholder="Filter by name…" value={filter} onChange={(e) => { setPage(1); setFilter(e.target.value) }} className="max-w-xs" />
        <NativeSelect value={groupId} onChange={(e) => { setPage(1); setGroupId(e.target.value) }} className="max-w-xs">
          <option value="">All groups</option>
          {groupsQuery.data?.items.map((g) => (
            <option key={g.id} value={g.id}>{g.name}</option>
          ))}
        </NativeSelect>
        <div className="flex-1" />
        <Button render={<Link href="/admin/modifiers/new" />} nativeButton={false}>
          <Plus className="mr-2 h-4 w-4" />New modifier
        </Button>
      </div>
      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Name</TableHead>
              <TableHead>Group</TableHead>
              <TableHead className="text-right">Price delta</TableHead>
              <TableHead className="w-[60px]" />
            </TableRow>
          </TableHeader>
          <TableBody>
            {query.isLoading && [0, 1, 2].map((i) => (
              <TableRow key={i}>
                <TableCell><Skeleton className="h-4 w-32" /></TableCell>
                <TableCell><Skeleton className="h-4 w-24" /></TableCell>
                <TableCell><Skeleton className="h-4 w-12" /></TableCell>
                <TableCell />
              </TableRow>
            ))}
            {query.data?.items.length === 0 && (
              <TableRow>
                <TableCell colSpan={4} className="py-12">
                  <div className="flex flex-col items-center gap-3 text-muted-foreground">
                    <ShibaMark className="h-10 w-10 opacity-60" />
                    <p className="text-sm">No modifiers match this filter.</p>
                  </div>
                </TableCell>
              </TableRow>
            )}
            {query.data?.items.map((m) => (
              <TableRow key={m.id}>
                <TableCell className="font-medium">{m.name}</TableCell>
                <TableCell className="text-sm text-muted-foreground">{groupName(m.modifierGroupId)}</TableCell>
                <TableCell className="text-right font-mono">{m.priceDelta >= 0 ? `+${m.priceDelta.toFixed(2)}` : m.priceDelta.toFixed(2)}</TableCell>
                <TableCell>
                  <DropdownMenu>
                    <DropdownMenuTrigger render={<Button variant="ghost" size="icon" className="h-8 w-8"><MoreHorizontal className="h-4 w-4" /></Button>} />
                    <DropdownMenuContent align="end">
                      <DropdownMenuItem render={<Link href={`/admin/modifiers/${m.id}`} />}>
                        <Pencil className="mr-2 h-4 w-4" />Edit
                      </DropdownMenuItem>
                      <DropdownMenuItem className="text-destructive focus:text-destructive" onClick={() => setConfirmDelete({ id: m.id, name: m.name })}>
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
            <DialogTitle>Delete modifier?</DialogTitle>
            <DialogDescription>Removes &ldquo;{confirmDelete?.name}&rdquo;.</DialogDescription>
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
