"use client"

import { use } from "react"
import { useQuery } from "@tanstack/react-query"
import { Skeleton } from "@/components/ui/skeleton"
import { PrintoutTemplateForm } from "@/features/printout-templates/printout-template-form"
import { printoutTemplatesApi } from "@/features/printout-templates/api"

interface Props { params: Promise<{ id: string }> }

export default function EditPrintoutTemplatePage({ params }: Props) {
  const { id } = use(params)
  const query = useQuery({ queryKey: ["printout-template", id], queryFn: () => printoutTemplatesApi.get(id) })

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Edit template</h1>
        <p className="text-muted-foreground">Update name or file URL.</p>
      </div>
      {query.isLoading && <Skeleton className="h-32 w-full max-w-xl" />}
      {query.data && <PrintoutTemplateForm initial={query.data} />}
    </div>
  )
}
