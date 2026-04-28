"use client"

import { useState } from "react"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { X } from "lucide-react"
import { toast } from "sonner"

import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { NativeSelect } from "@/components/ui/native-select"
import { ApiError } from "@/lib/api"
import { priceGroupsApi } from "@/features/price-groups/api"
import { salesChannelsApi } from "./api"

interface Props {
  channelId: string
  attachedIds: string[]
}

export function SalesChannelPriceGroupsSection({ channelId, attachedIds }: Props) {
  const queryClient = useQueryClient()
  const [selectedId, setSelectedId] = useState("")

  const groupsQuery = useQuery({
    queryKey: ["price-groups", "lookup"],
    queryFn: () => priceGroupsApi.list({ page: 1, pageSize: 100, sort: "name" }),
  })

  const refresh = () => queryClient.invalidateQueries({ queryKey: ["sales-channel", channelId] })

  const linkMutation = useMutation({
    mutationFn: (priceGroupId: string) => salesChannelsApi.linkPriceGroup(channelId, priceGroupId),
    onSuccess: () => {
      toast.success("Price group linked")
      setSelectedId("")
      refresh()
    },
    onError: (err) => toast.error(err instanceof ApiError ? err.message : "Could not link price group"),
  })

  const unlinkMutation = useMutation({
    mutationFn: (priceGroupId: string) => salesChannelsApi.unlinkPriceGroup(channelId, priceGroupId),
    onSuccess: () => {
      toast.success("Price group unlinked")
      refresh()
    },
    onError: (err) => toast.error(err instanceof ApiError ? err.message : "Could not unlink price group"),
  })

  const all = groupsQuery.data?.items ?? []
  const attached = all.filter((g) => attachedIds.includes(g.id))
  const available = all.filter((g) => !attachedIds.includes(g.id))

  return (
    <div className="space-y-3">
      <div className="flex flex-wrap gap-2">
        {attached.length === 0 && <p className="text-sm text-muted-foreground">No price groups linked.</p>}
        {attached.map((g) => (
          <Badge key={g.id} variant="secondary" className="gap-1.5">
            {g.name}
            <button
              type="button"
              onClick={() => unlinkMutation.mutate(g.id)}
              className="hover:text-destructive"
              aria-label={`Unlink ${g.name}`}
            >
              <X className="h-3 w-3" />
            </button>
          </Badge>
        ))}
      </div>
      <div className="flex gap-2">
        <NativeSelect value={selectedId} onChange={(e) => setSelectedId(e.target.value)} className="max-w-xs">
          <option value="">Link price group…</option>
          {available.map((g) => (
            <option key={g.id} value={g.id}>{g.name}</option>
          ))}
        </NativeSelect>
        <Button
          type="button"
          variant="outline"
          size="sm"
          disabled={!selectedId || linkMutation.isPending}
          onClick={() => linkMutation.mutate(selectedId)}
        >
          Link
        </Button>
      </div>
    </div>
  )
}
