"use client"

import { use } from "react"
import { useQuery } from "@tanstack/react-query"
import { Skeleton } from "@/components/ui/skeleton"
import { AllergenForm } from "@/features/allergens/allergen-form"
import { allergensApi } from "@/features/allergens/api"

interface Props {
  params: Promise<{ id: string }>
}

export default function EditAllergenPage({ params }: Props) {
  const { id } = use(params)
  const query = useQuery({
    queryKey: ["allergen", id],
    queryFn: () => allergensApi.get(id),
  })

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Edit allergen</h1>
        <p className="text-muted-foreground">Rename an allergen.</p>
      </div>
      {query.isLoading && <Skeleton className="h-32 w-full max-w-xl" />}
      {query.data && <AllergenForm initial={query.data} />}
    </div>
  )
}
