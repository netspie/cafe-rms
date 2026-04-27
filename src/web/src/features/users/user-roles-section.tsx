"use client"

import { useState } from "react"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { X } from "lucide-react"
import { toast } from "sonner"

import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { NativeSelect } from "@/components/ui/native-select"
import { ApiError } from "@/lib/api"
import { rolesApi } from "@/features/roles/api"
import { usersApi } from "./api"

interface Props {
  userId: string
  attachedRoleNames: string[]
}

export function UserRolesSection({ userId, attachedRoleNames }: Props) {
  const queryClient = useQueryClient()
  const [selectedRoleId, setSelectedRoleId] = useState("")

  const rolesQuery = useQuery({
    queryKey: ["roles", "lookup"],
    queryFn: () => rolesApi.list(),
  })

  const refresh = () => {
    queryClient.invalidateQueries({ queryKey: ["user", userId] })
    queryClient.invalidateQueries({ queryKey: ["users"] })
  }

  const assignMutation = useMutation({
    mutationFn: (roleId: string) => usersApi.assignRole(userId, roleId),
    onSuccess: () => {
      toast.success("Role assigned")
      setSelectedRoleId("")
      refresh()
    },
    onError: (err) => toast.error(err instanceof ApiError ? err.message : "Could not assign role"),
  })

  const unassignMutation = useMutation({
    mutationFn: (roleId: string) => usersApi.unassignRole(userId, roleId),
    onSuccess: () => {
      toast.success("Role removed")
      refresh()
    },
    onError: (err) => toast.error(err instanceof ApiError ? err.message : "Could not remove role"),
  })

  const allRoles = rolesQuery.data ?? []
  const attached = allRoles.filter((r) => attachedRoleNames.includes(r.name))
  const available = allRoles.filter((r) => !attachedRoleNames.includes(r.name))

  return (
    <div className="space-y-3">
      <div className="flex flex-wrap gap-2">
        {attached.length === 0 && <p className="text-sm text-muted-foreground">No roles assigned.</p>}
        {attached.map((r) => (
          <Badge key={r.id} variant="secondary" className="gap-1.5">
            {r.name}
            <button
              type="button"
              onClick={() => unassignMutation.mutate(r.id)}
              className="hover:text-destructive"
              aria-label={`Remove ${r.name}`}
            >
              <X className="h-3 w-3" />
            </button>
          </Badge>
        ))}
      </div>
      <div className="flex gap-2">
        <NativeSelect
          value={selectedRoleId}
          onChange={(e) => setSelectedRoleId(e.target.value)}
          className="max-w-xs"
        >
          <option value="">Assign role…</option>
          {available.map((r) => (
            <option key={r.id} value={r.id}>{r.name}</option>
          ))}
        </NativeSelect>
        <Button
          type="button"
          variant="outline"
          size="sm"
          disabled={!selectedRoleId || assignMutation.isPending}
          onClick={() => assignMutation.mutate(selectedRoleId)}
        >
          Assign
        </Button>
      </div>
    </div>
  )
}
