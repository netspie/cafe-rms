"use client"

import { useState } from "react"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { X } from "lucide-react"
import { toast } from "sonner"

import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { NativeSelect } from "@/components/ui/native-select"
import { ApiError } from "@/lib/api"
import { productsApi } from "@/features/products/api"
import { productListsApi } from "./api"

interface Props {
  listId: string
  productIds: string[]
}

export function ProductListItemsSection({ listId, productIds }: Props) {
  const queryClient = useQueryClient()
  const [selectedId, setSelectedId] = useState("")

  const productsQuery = useQuery({
    queryKey: ["products", "lookup"],
    queryFn: () => productsApi.list({ page: 1, pageSize: 200, sort: "name" }),
  })

  const refresh = () => queryClient.invalidateQueries({ queryKey: ["product-list", listId] })

  const addMutation = useMutation({
    mutationFn: (productId: string) => productListsApi.addProduct(listId, productId),
    onSuccess: () => {
      toast.success("Product added")
      setSelectedId("")
      refresh()
    },
    onError: (err) => toast.error(err instanceof ApiError ? err.message : "Could not add product"),
  })

  const removeMutation = useMutation({
    mutationFn: (productId: string) => productListsApi.removeProduct(listId, productId),
    onSuccess: () => {
      toast.success("Product removed")
      refresh()
    },
    onError: (err) => toast.error(err instanceof ApiError ? err.message : "Could not remove product"),
  })

  const all = productsQuery.data?.items ?? []
  const attached = all.filter((p) => productIds.includes(p.id))
  const available = all.filter((p) => !productIds.includes(p.id))

  return (
    <div className="space-y-3">
      <div className="flex flex-wrap gap-2">
        {attached.length === 0 && <p className="text-sm text-muted-foreground">No products in this list yet.</p>}
        {attached.map((p) => (
          <Badge key={p.id} variant="secondary" className="gap-1.5">
            {p.name}
            <button
              type="button"
              onClick={() => removeMutation.mutate(p.id)}
              className="hover:text-destructive"
              aria-label={`Remove ${p.name}`}
            >
              <X className="h-3 w-3" />
            </button>
          </Badge>
        ))}
      </div>
      <div className="flex gap-2">
        <NativeSelect value={selectedId} onChange={(e) => setSelectedId(e.target.value)} className="max-w-xs">
          <option value="">Add product…</option>
          {available.map((p) => (
            <option key={p.id} value={p.id}>{p.name}</option>
          ))}
        </NativeSelect>
        <Button
          type="button"
          variant="outline"
          size="sm"
          disabled={!selectedId || addMutation.isPending}
          onClick={() => addMutation.mutate(selectedId)}
        >
          Add
        </Button>
      </div>
    </div>
  )
}
