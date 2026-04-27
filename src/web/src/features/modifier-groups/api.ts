import { api, type PagedResult } from "@/lib/api"

export interface ModifierGroupListItem {
  id: string
  name: string
  createdAt: string
}

export interface ModifierGroupDetail {
  id: string
  name: string
  createdAt: string
  updatedAt: string | null
}

export interface ModifierGroupInput {
  name: string
}

export interface ListModifierGroupsQuery {
  page: number
  pageSize: number
  sort?: string
  name?: string
}

export const modifierGroupsApi = {
  list: (q: ListModifierGroupsQuery) => {
    const search = new URLSearchParams()
    search.set("page", String(q.page))
    search.set("pageSize", String(q.pageSize))
    if (q.sort) search.set("sort", q.sort)
    if (q.name) search.set("name", q.name)
    return api.get<PagedResult<ModifierGroupListItem>>(`/api/modifier-groups?${search.toString()}`)
  },
  get: (id: string) => api.get<ModifierGroupDetail>(`/api/modifier-groups/${id}`),
  create: (body: ModifierGroupInput) => api.post<{ id: string }>("/api/modifier-groups", body),
  update: (id: string, body: ModifierGroupInput) => api.put<void>(`/api/modifier-groups/${id}`, body),
  remove: (id: string) => api.delete<void>(`/api/modifier-groups/${id}`),
}
