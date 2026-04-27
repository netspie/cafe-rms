import { api, type PagedResult } from "@/lib/api"

export interface AllergenListItem {
  id: string
  name: string
  createdAt: string
}

export interface AllergenDetail {
  id: string
  name: string
  createdAt: string
  updatedAt: string | null
}

export interface AllergenInput {
  name: string
}

export interface ListAllergensQuery {
  page: number
  pageSize: number
  sort?: string
  name?: string
}

export const allergensApi = {
  list: (q: ListAllergensQuery) => {
    const search = new URLSearchParams()
    search.set("page", String(q.page))
    search.set("pageSize", String(q.pageSize))
    if (q.sort) search.set("sort", q.sort)
    if (q.name) search.set("name", q.name)
    return api.get<PagedResult<AllergenListItem>>(`/api/allergens?${search.toString()}`)
  },
  get: (id: string) => api.get<AllergenDetail>(`/api/allergens/${id}`),
  create: (body: AllergenInput) => api.post<{ id: string }>("/api/allergens", body),
  update: (id: string, body: AllergenInput) => api.put<void>(`/api/allergens/${id}`, body),
  remove: (id: string) => api.delete<void>(`/api/allergens/${id}`),
}
