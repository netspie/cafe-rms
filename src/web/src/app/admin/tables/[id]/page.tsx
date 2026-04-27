"use client"

import { use } from "react"
import { useQuery } from "@tanstack/react-query"
import { Skeleton } from "@/components/ui/skeleton"
import { TableForm } from "@/features/tables/table-form"
import { tablesApi } from "@/features/tables/api"

interface Props {
  params: Promise<{ id: string }>
}

export default function EditTablePage({ params }: Props) {
  const { id } = use(params)
  const query = useQuery({
    queryKey: ["table", id],
    queryFn: () => tablesApi.get(id),
  })

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Edit table</h1>
        <p className="text-muted-foreground">Rename a table.</p>
      </div>
      {query.isLoading && <Skeleton className="h-32 w-full max-w-xl" />}
      {query.data && <TableForm initial={query.data} />}
    </div>
  )
}
