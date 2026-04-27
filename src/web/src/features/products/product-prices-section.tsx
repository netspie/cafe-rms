"use client"

import { useState } from "react"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { toast } from "sonner"

import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { ApiError } from "@/lib/api"
import { priceGroupsApi } from "@/features/price-groups/api"
import { productsApi, type ProductPrice } from "./api"

interface Props {
  productId: string
  prices: ProductPrice[]
}

export function ProductPricesSection({ productId, prices }: Props) {
  const queryClient = useQueryClient()
  const [drafts, setDrafts] = useState<Record<string, string>>({})

  const groupsQuery = useQuery({
    queryKey: ["price-groups", "lookup"],
    queryFn: () => priceGroupsApi.list({ page: 1, pageSize: 100, sort: "name" }),
  })

  const refresh = () => queryClient.invalidateQueries({ queryKey: ["product", productId] })

  const setMutation = useMutation({
    mutationFn: ({ priceGroupId, net }: { priceGroupId: string; net: number }) =>
      productsApi.setPrice(productId, priceGroupId, net),
    onSuccess: () => {
      toast.success("Price saved")
      refresh()
    },
    onError: (err) => toast.error(err instanceof ApiError ? err.message : "Could not save price"),
  })

  const groups = groupsQuery.data?.items ?? []
  const priceFor = (groupId: string) =>
    prices.find((p) => p.priceGroupId === groupId)?.net.toString() ?? ""

  const valueFor = (groupId: string) =>
    drafts[groupId] !== undefined ? drafts[groupId] : priceFor(groupId)

  return (
    <div className="space-y-3">
      {groups.length === 0 && (
        <p className="text-sm text-muted-foreground">No price groups defined yet.</p>
      )}
      {groups.map((g) => (
        <div key={g.id} className="flex items-center gap-3">
          <span className="w-40 text-sm font-medium">{g.name}</span>
          <Input
            type="number"
            step="0.01"
            min="0"
            placeholder="0.00"
            value={valueFor(g.id)}
            onChange={(e) => setDrafts((d) => ({ ...d, [g.id]: e.target.value }))}
            className="max-w-32"
          />
          <Button
            type="button"
            variant="outline"
            size="sm"
            disabled={setMutation.isPending}
            onClick={() => {
              const raw = valueFor(g.id)
              const net = Number(raw)
              if (!Number.isFinite(net) || net < 0) {
                toast.error("Enter a non-negative number")
                return
              }
              setMutation.mutate({ priceGroupId: g.id, net })
            }}
          >
            Save
          </Button>
        </div>
      ))}
    </div>
  )
}
