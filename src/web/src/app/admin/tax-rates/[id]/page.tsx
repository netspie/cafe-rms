"use client"

import { use } from "react"
import { useQuery } from "@tanstack/react-query"
import { Skeleton } from "@/components/ui/skeleton"
import { TaxRateForm } from "@/features/tax-rates/tax-rate-form"
import { taxRatesApi } from "@/features/tax-rates/api"

interface Props {
  params: Promise<{ id: string }>
}

export default function EditTaxRatePage({ params }: Props) {
  const { id } = use(params)
  const query = useQuery({
    queryKey: ["tax-rate", id],
    queryFn: () => taxRatesApi.get(id),
  })

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Edit tax rate</h1>
        <p className="text-muted-foreground">Update name, description, or rate.</p>
      </div>
      {query.isLoading && <Skeleton className="h-32 w-full max-w-xl" />}
      {query.data && <TaxRateForm initial={query.data} />}
    </div>
  )
}
