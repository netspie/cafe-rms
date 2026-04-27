import { api } from "@/lib/api"

export interface UserListItem {
  id: string
  email: string
  firstName: string
  lastName: string
  roles: string[]
}

export interface UserDetail {
  id: string
  email: string
  firstName: string
  lastName: string
  roles: string[]
}

export interface RegisterStaffInput {
  email: string
  password: string
  firstName: string
  lastName: string
  roles?: string[]
}

export interface UpdateUserInput {
  firstName: string
  lastName: string
}

export const usersApi = {
  list: () => api.get<UserListItem[]>("/api/users"),
  get: (id: string) => api.get<UserDetail>(`/api/users/${id}`),
  registerStaff: (body: RegisterStaffInput) =>
    api.post<{ userId: string; email: string; accountType: string; roles: string[] }>(
      "/api/auth/register/staff",
      body,
    ),
  update: (id: string, body: UpdateUserInput) => api.put<void>(`/api/users/${id}`, body),
  remove: (id: string) => api.delete<void>(`/api/users/${id}`),
  assignRole: (userId: string, roleId: string) =>
    api.post<void>(`/api/users/${userId}/roles/${roleId}`),
  unassignRole: (userId: string, roleId: string) =>
    api.delete<void>(`/api/users/${userId}/roles/${roleId}`),
}
