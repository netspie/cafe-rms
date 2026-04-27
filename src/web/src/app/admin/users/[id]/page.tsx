"use client"

import { use } from "react"
import { useQuery } from "@tanstack/react-query"

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Skeleton } from "@/components/ui/skeleton"
import { UserForm } from "@/features/users/user-form"
import { UserRolesSection } from "@/features/users/user-roles-section"
import { usersApi } from "@/features/users/api"

interface Props {
  params: Promise<{ id: string }>
}

export default function EditUserPage({ params }: Props) {
  const { id } = use(params)
  const query = useQuery({
    queryKey: ["user", id],
    queryFn: () => usersApi.get(id),
  })

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">
          {query.data ? `${query.data.firstName} ${query.data.lastName}` : "Loading user…"}
        </h1>
        <p className="text-muted-foreground">Staff profile and role assignment.</p>
      </div>

      {query.isLoading && <Skeleton className="h-64 w-full max-w-3xl" />}

      {query.data && (
        <div className="grid gap-6 lg:grid-cols-2">
          <Card>
            <CardHeader>
              <CardTitle className="text-base">Profile</CardTitle>
            </CardHeader>
            <CardContent>
              <UserForm user={query.data} />
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle className="text-base">Roles</CardTitle>
            </CardHeader>
            <CardContent>
              <UserRolesSection userId={query.data.id} attachedRoleNames={query.data.roles} />
            </CardContent>
          </Card>
        </div>
      )}
    </div>
  )
}
