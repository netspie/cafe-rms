"use client"

import { use } from "react"
import { useQuery } from "@tanstack/react-query"
import { Skeleton } from "@/components/ui/skeleton"
import { PromotionCodeForm } from "@/features/promotion-codes/promotion-code-form"
import { promotionCodesApi } from "@/features/promotion-codes/api"

interface Props { params: Promise<{ id: string }> }

export default function EditPromotionCodePage({ params }: Props) {
  const { id } = use(params)
  const query = useQuery({ queryKey: ["promotion-code", id], queryFn: () => promotionCodesApi.get(id) })

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Edit promotion code</h1>
        <p className="text-muted-foreground">Adjust discount, window, or cap.</p>
      </div>
      {query.isLoading && <Skeleton className="h-64 w-full max-w-xl" />}
      {query.data && <PromotionCodeForm initial={query.data} />}
    </div>
  )
}
