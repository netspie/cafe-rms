"use client"

import { use } from "react"
import { useQuery } from "@tanstack/react-query"
import { Skeleton } from "@/components/ui/skeleton"
import { RoleForm } from "@/features/roles/role-form"
import { rolesApi } from "@/features/roles/api"

interface Props {
  params: Promise<{ id: string }>
}

export default function EditRolePage({ params }: Props) {
  const { id } = use(params)
  const query = useQuery({
    queryKey: ["roles"],
    queryFn: () => rolesApi.list(),
  })

  const role = query.data?.find((r) => r.id === id)

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Edit role</h1>
        <p className="text-muted-foreground">Rename, or change which permissions this role grants.</p>
      </div>
      {query.isLoading && <Skeleton className="h-64 w-full max-w-2xl" />}
      {!query.isLoading && !role && (
        <p className="text-sm text-muted-foreground">Role not found.</p>
      )}
      {role && <RoleForm initial={role} />}
    </div>
  )
}
