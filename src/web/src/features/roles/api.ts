import { api } from "@/lib/api"

export interface RoleListItem {
  id: string
  name: string
  permissions: string[]
}

export interface RoleInput {
  name: string
  permissions?: string[]
}

export const rolesApi = {
  list: () => api.get<RoleListItem[]>("/api/roles"),
  create: (body: RoleInput) => api.post<{ roleId: string }>("/api/roles", body),
  update: (id: string, body: RoleInput) => api.put<void>(`/api/roles/${id}`, body),
  remove: (id: string) => api.delete<void>(`/api/roles/${id}`),
}
