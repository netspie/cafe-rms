"use client"

import { useState } from "react"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { X } from "lucide-react"
import { toast } from "sonner"

import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { NativeSelect } from "@/components/ui/native-select"
import { ApiError } from "@/lib/api"
import { tagsApi } from "@/features/tags/api"
import { productsApi } from "./api"

interface Props {
  productId: string
  attachedIds: string[]
}

export function ProductTagsSection({ productId, attachedIds }: Props) {
  const queryClient = useQueryClient()
  const [selectedId, setSelectedId] = useState("")

  const tagsQuery = useQuery({
    queryKey: ["tags", "lookup"],
    queryFn: () => tagsApi.list({ page: 1, pageSize: 100, sort: "name" }),
  })

  const refresh = () => queryClient.invalidateQueries({ queryKey: ["product", productId] })

  const attachMutation = useMutation({
    mutationFn: (tagId: string) => productsApi.attachTag(productId, tagId),
    onSuccess: () => {
      toast.success("Tag attached")
      setSelectedId("")
      refresh()
    },
    onError: (err) => toast.error(err instanceof ApiError ? err.message : "Could not attach tag"),
  })

  const detachMutation = useMutation({
    mutationFn: (tagId: string) => productsApi.detachTag(productId, tagId),
    onSuccess: () => {
      toast.success("Tag removed")
      refresh()
    },
    onError: (err) => toast.error(err instanceof ApiError ? err.message : "Could not remove tag"),
  })

  const allTags = tagsQuery.data?.items ?? []
  const attached = allTags.filter((t) => attachedIds.includes(t.id))
  const available = allTags.filter((t) => !attachedIds.includes(t.id))

  return (
    <div className="space-y-3">
      <div className="flex flex-wrap gap-2">
        {attached.length === 0 && <p className="text-sm text-muted-foreground">No tags attached.</p>}
        {attached.map((t) => (
          <Badge key={t.id} variant="secondary" className="gap-1.5">
            {t.name}
            <button
              type="button"
              onClick={() => detachMutation.mutate(t.id)}
              className="hover:text-destructive"
              aria-label={`Remove ${t.name}`}
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
          <option value="">Add tag…</option>
          {available.map((t) => (
            <option key={t.id} value={t.id}>{t.name}</option>
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
