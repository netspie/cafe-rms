"use client"

import { useState } from "react"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { X } from "lucide-react"
import { toast } from "sonner"

import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { NativeSelect } from "@/components/ui/native-select"
import { ApiError } from "@/lib/api"
import { allergensApi } from "@/features/allergens/api"
import { productsApi } from "./api"

interface Props {
  productId: string
  attachedIds: string[]
}

export function ProductAllergensSection({ productId, attachedIds }: Props) {
  const queryClient = useQueryClient()
  const [selectedId, setSelectedId] = useState("")

  const allergensQuery = useQuery({
    queryKey: ["allergens", "lookup"],
    queryFn: () => allergensApi.list({ page: 1, pageSize: 100, sort: "name" }),
  })

  const refresh = () => queryClient.invalidateQueries({ queryKey: ["product", productId] })

  const attachMutation = useMutation({
    mutationFn: (allergenId: string) => productsApi.attachAllergen(productId, allergenId),
    onSuccess: () => {
      toast.success("Allergen attached")
      setSelectedId("")
      refresh()
    },
    onError: (err) => toast.error(err instanceof ApiError ? err.message : "Could not attach allergen"),
  })

  const detachMutation = useMutation({
    mutationFn: (allergenId: string) => productsApi.detachAllergen(productId, allergenId),
    onSuccess: () => {
      toast.success("Allergen removed")
      refresh()
    },
    onError: (err) => toast.error(err instanceof ApiError ? err.message : "Could not remove allergen"),
  })

  const allAllergens = allergensQuery.data?.items ?? []
  const attached = allAllergens.filter((a) => attachedIds.includes(a.id))
  const available = allAllergens.filter((a) => !attachedIds.includes(a.id))

  return (
    <div className="space-y-3">
      <div className="flex flex-wrap gap-2">
        {attached.length === 0 && <p className="text-sm text-muted-foreground">No allergens attached.</p>}
        {attached.map((a) => (
          <Badge key={a.id} variant="secondary" className="gap-1.5">
            {a.name}
            <button
              type="button"
              onClick={() => detachMutation.mutate(a.id)}
              className="hover:text-destructive"
              aria-label={`Remove ${a.name}`}
            >
              <X className="h-3 w-3" />
            </button>
          </Badge>
        ))}
      </div>
      <div className="flex gap-2">
        <NativeSelect
          value={selectedId}
          onChange={(e) => setSelectedId(e.target.value)}
          className="max-w-xs"
        >
          <option value="">Add allergen…</option>
          {available.map((a) => (
            <option key={a.id} value={a.id}>{a.name}</option>
          ))}
        </NativeSelect>
        <Button
          type="button"
          variant="outline"
          size="sm"
          disabled={!selectedId || attachMutation.isPending}
          onClick={() => attachMutation.mutate(selectedId)}
        >
          Add
        </Button>
      </div>
    </div>
  )
}
