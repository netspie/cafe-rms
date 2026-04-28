"use client"

import { use } from "react"
import { useQuery } from "@tanstack/react-query"
import { Skeleton } from "@/components/ui/skeleton"
import { ModifierForm } from "@/features/modifiers/modifier-form"
import { modifiersApi } from "@/features/modifiers/api"

interface Props { params: Promise<{ id: string }> }

export default function EditModifierPage({ params }: Props) {
  const { id } = use(params)
  const query = useQuery({ queryKey: ["modifier", id], queryFn: () => modifiersApi.get(id) })

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Edit modifier</h1>
        <p className="text-muted-foreground">Rename or adjust the price delta.</p>
      </div>
      {query.isLoading && <Skeleton className="h-32 w-full max-w-xl" />}
      {query.data && <ModifierForm initial={query.data} />}
    </div>
  )
}
