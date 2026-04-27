"use client"

import { use } from "react"
import { useQuery } from "@tanstack/react-query"
import { Skeleton } from "@/components/ui/skeleton"
import { PriceGroupForm } from "@/features/price-groups/price-group-form"
import { priceGroupsApi } from "@/features/price-groups/api"

interface Props {
  params: Promise<{ id: string }>
}

export default function EditPriceGroupPage({ params }: Props) {
  const { id } = use(params)
  const query = useQuery({
    queryKey: ["price-group", id],
    queryFn: () => priceGroupsApi.get(id),
  })

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Edit price group</h1>
        <p className="text-muted-foreground">Rename a pricing tier.</p>
      </div>
      {query.isLoading && <Skeleton className="h-32 w-full max-w-xl" />}
      {query.data && <PriceGroupForm initial={query.data} />}
    </div>
  )
}
