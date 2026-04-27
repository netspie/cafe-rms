import { api, type PagedResult } from "@/lib/api"

export interface PriceGroupListItem {
  id: string
  name: string
  createdAt: string
}

export interface PriceGroupDetail {
  id: string
  name: string
  createdAt: string
  updatedAt: string | null
}

export interface PriceGroupInput {
  name: string
}

export interface ListPriceGroupsQuery {
  page: number
  pageSize: number
  sort?: string
  name?: string
}

export const priceGroupsApi = {
  list: (q: ListPriceGroupsQuery) => {
    const search = new URLSearchParams()
    search.set("page", String(q.page))
    search.set("pageSize", String(q.pageSize))
    if (q.sort) search.set("sort", q.sort)
    if (q.name) search.set("name", q.name)
    return api.get<PagedResult<PriceGroupListItem>>(`/api/price-groups?${search.toString()}`)
  },
  get: (id: string) => api.get<PriceGroupDetail>(`/api/price-groups/${id}`),
  create: (body: PriceGroupInput) => api.post<{ id: string }>("/api/price-groups", body),
  update: (id: string, body: PriceGroupInput) => api.put<void>(`/api/price-groups/${id}`, body),
  remove: (id: string) => api.delete<void>(`/api/price-groups/${id}`),
}
