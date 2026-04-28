import { api, type PagedResult } from "@/lib/api"

export interface ModifierListItem {
  id: string
  name: string
  priceDelta: number
  modifierGroupId: string
  createdAt: string
}

export interface ModifierDetail {
  id: string
  name: string
  priceDelta: number
  modifierGroupId: string
  createdAt: string
  updatedAt: string | null
}

export interface ModifierCreateInput {
  modifierGroupId: string
  name: string
  priceDelta: number
}

export interface ModifierUpdateInput {
  name: string
  priceDelta: number
}

export interface ListModifiersQuery {
  page: number
  pageSize: number
  sort?: string
  name?: string
  modifierGroupId?: string
}

export const modifiersApi = {
  list: (q: ListModifiersQuery) => {
    const search = new URLSearchParams()
    search.set("page", String(q.page))
    search.set("pageSize", String(q.pageSize))
    if (q.sort) search.set("sort", q.sort)
    if (q.name) search.set("name", q.name)
    if (q.modifierGroupId) search.set("modifierGroupId", q.modifierGroupId)
    return api.get<PagedResult<ModifierListItem>>(`/api/modifiers?${search.toString()}`)
  },
  get: (id: string) => api.get<ModifierDetail>(`/api/modifiers/${id}`),
  create: (body: ModifierCreateInput) => api.post<{ id: string }>("/api/modifiers", body),
  update: (id: string, body: ModifierUpdateInput) => api.put<void>(`/api/modifiers/${id}`, body),
  remove: (id: string) => api.delete<void>(`/api/modifiers/${id}`),
}
