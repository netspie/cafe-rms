"use client"

import { use } from "react"
import { useQuery } from "@tanstack/react-query"
import { Skeleton } from "@/components/ui/skeleton"
import { ModifierGroupForm } from "@/features/modifier-groups/modifier-group-form"
import { modifierGroupsApi } from "@/features/modifier-groups/api"

interface Props { params: Promise<{ id: string }> }

export default function EditModifierGroupPage({ params }: Props) {
  const { id } = use(params)
  const query = useQuery({ queryKey: ["modifier-group", id], queryFn: () => modifierGroupsApi.get(id) })

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Edit modifier group</h1>
        <p className="text-muted-foreground">Rename this group.</p>
      </div>
      {query.isLoading && <Skeleton className="h-32 w-full max-w-xl" />}
      {query.data && <ModifierGroupForm initial={query.data} />}
    </div>
  )
}
