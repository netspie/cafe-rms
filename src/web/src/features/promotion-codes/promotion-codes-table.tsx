"use client"

import Link from "next/link"
import { useState } from "react"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { MoreHorizontal, Pencil, Plus, Trash2 } from "lucide-react"
import { toast } from "sonner"

import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from "@/components/ui/dropdown-menu"
import { Skeleton } from "@/components/ui/skeleton"
import { ApiError } from "@/lib/api"
import { ShibaMark } from "@/components/shiba-mark"
import { promotionCodesApi } from "./api"

const PAGE_SIZE = 20

export function PromotionCodesTable() {
  const queryClient = useQueryClient()
  const [page, setPage] = useState(1)
  const [filter, setFilter] = useState("")
  const [confirmDelete, setConfirmDelete] = useState<{ id: string; code: string } | null>(null)

  const query = useQuery({
    queryKey: ["promotion-codes", { page, filter }],
    queryFn: () => promotionCodesApi.list({ page, pageSize: PAGE_SIZE, code: filter || undefined, sort: "-createdAt" }),
  })

  const deleteMutation = useMutation({
    mutationFn: (id: string) => promotionCodesApi.remove(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["promotion-codes"] })
      toast.success("Promotion code deleted")
      setConfirmDelete(null)
    },
    onError: (err) => toast.error(err instanceof ApiError ? err.message : "Could not delete promotion code"),
  })

  const totalPages = query.data ? Math.max(1, Math.ceil(query.data.total / PAGE_SIZE)) : 1

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-3">
        <Input placeholder="Filter by code…" value={filter} onChange={(e) => { setPage(1); setFilter(e.target.value) }} className="max-w-xs" />
        <div className="flex-1" />
        <Button render={<Link href="/admin/promotion-codes/new" />} nativeButton={false}>
          <Plus className="mr-2 h-4 w-4" />New code
        </Button>
      </div>
      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Code</TableHead>
              <TableHead className="text-right">Discount</TableHead>
              <TableHead>Valid window</TableHead>
              <TableHead className="text-right">Used</TableHead>
              <TableHead className="w-[60px]" />
            </TableRow>
          </TableHeader>
          <TableBody>
            {query.isLoading && [0, 1, 2].map((i) => (
              <TableRow key={i}>
                <TableCell><Skeleton className="h-4 w-24" /></TableCell>
                <TableCell><Skeleton className="h-4 w-12" /></TableCell>
                <TableCell><Skeleton className="h-4 w-40" /></TableCell>
                <TableCell><Skeleton className="h-4 w-12" /></TableCell>
                <TableCell />
              </TableRow>
            ))}
            {query.data?.items.length === 0 && (
              <TableRow>
                <TableCell colSpan={5} className="py-12">
                  <div className="flex flex-col items-center gap-3 text-muted-foreground">
                    <ShibaMark className="h-10 w-10 opacity-60" />
                    <p className="text-sm">No promotion codes yet.</p>
                  </div>
                </TableCell>
              </TableRow>
            )}
            {query.data?.items.map((p) => {
              const window = [
                p.validFrom ? new Date(p.validFrom).toLocaleDateString() : "any time",
                "→",
                p.validUntil ? new Date(p.validUntil).toLocaleDateString() : "no expiry",
              ].join(" ")
              return (
                <TableRow key={p.id}>
                  <TableCell className="font-mono font-medium">{p.code}</TableCell>
                  <TableCell className="text-right font-mono">{p.discountPercentage}%</TableCell>
                  <TableCell className="text-sm text-muted-foreground">{window}</TableCell>
                  <TableCell className="text-right font-mono text-sm">
                    {p.usesCount}{p.maxUses != null ? ` / ${p.maxUses}` : ""}
                  </TableCell>
                  <TableCell>
                    <DropdownMenu>
                      <DropdownMenuTrigger render={<Button variant="ghost" size="icon" className="h-8 w-8"><MoreHorizontal className="h-4 w-4" /></Button>} />
                      <DropdownMenuContent align="end">
                        <DropdownMenuItem render={<Link href={`/admin/promotion-codes/${p.id}`} />}>
                          <Pencil className="mr-2 h-4 w-4" />Edit
                        </DropdownMenuItem>
                        <DropdownMenuItem className="text-destructive focus:text-destructive" onClick={() => setConfirmDelete({ id: p.id, code: p.code })}>
                          <Trash2 className="mr-2 h-4 w-4" />Delete
                        </DropdownMenuItem>
                      </DropdownMenuContent>
                    </DropdownMenu>
                  </TableCell>
                </TableRow>
              )
            })}
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
            <DialogTitle>Delete promotion code?</DialogTitle>
            <DialogDescription>Removes &ldquo;{confirmDelete?.code}&rdquo; — past uses stay recorded on orders.</DialogDescription>
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
