"use client"

import { use } from "react"
import { useQuery } from "@tanstack/react-query"
import { Skeleton } from "@/components/ui/skeleton"
import { TagForm } from "@/features/tags/tag-form"
import { tagsApi } from "@/features/tags/api"

interface Props {
  params: Promise<{ id: string }>
}

export default function EditTagPage({ params }: Props) {
  const { id } = use(params)
  const query = useQuery({
    queryKey: ["tag", id],
    queryFn: () => tagsApi.get(id),
  })

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Edit tag</h1>
        <p className="text-muted-foreground">Rename a tag or change its image.</p>
      </div>
      {query.isLoading && <Skeleton className="h-32 w-full max-w-xl" />}
      {query.data && <TagForm initial={query.data} />}
    </div>
  )
}
