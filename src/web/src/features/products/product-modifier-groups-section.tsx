"use client"

import { useState } from "react"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { X } from "lucide-react"
import { toast } from "sonner"

import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { NativeSelect } from "@/components/ui/native-select"
import { ApiError } from "@/lib/api"
import { modifierGroupsApi } from "@/features/modifier-groups/api"
import { productsApi } from "./api"

interface Props {
  productId: string
  attachedIds: string[]
}

export function ProductModifierGroupsSection({ productId, attachedIds }: Props) {
  const queryClient = useQueryClient()
  const [selectedId, setSelectedId] = useState("")

  const groupsQuery = useQuery({
    queryKey: ["modifier-groups", "lookup"],
    queryFn: () => modifierGroupsApi.list({ page: 1, pageSize: 100, sort: "name" }),
  })

  const refresh = () => queryClient.invalidateQueries({ queryKey: ["product", productId] })

  const attachMutation = useMutation({
    mutationFn: (groupId: string) => productsApi.attachModifierGroup(productId, groupId),
    onSuccess: () => {
      toast.success("Modifier group attached")
      setSelectedId("")
      refresh()
    },
    onError: (err) => toast.error(err instanceof ApiError ? err.message : "Could not attach modifier group"),
  })

  const detachMutation = useMutation({
    mutationFn: (groupId: string) => productsApi.detachModifierGroup(productId, groupId),
    onSuccess: () => {
      toast.success("Modifier group removed")
      refresh()
    },
    onError: (err) => toast.error(err instanceof ApiError ? err.message : "Could not remove modifier group"),
  })

  const allGroups = groupsQuery.data?.items ?? []
  const attached = allGroups.filter((g) => attachedIds.includes(g.id))
  const available = allGroups.filter((g) => !attachedIds.includes(g.id))

  return (
    <div className="space-y-3">
      <div className="flex flex-wrap gap-2">
        {attached.length === 0 && <p className="text-sm text-muted-foreground">No modifier groups attached.</p>}
        {attached.map((g) => (
          <Badge key={g.id} variant="secondary" className="gap-1.5">
            {g.name}
            <button
              type="button"
              onClick={() => detachMutation.mutate(g.id)}
              className="hover:text-destructive"
              aria-label={`Remove ${g.name}`}
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
          <option value="">Add modifier group…</option>
          {available.map((g) => (
            <option key={g.id} value={g.id}>{g.name}</option>
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
