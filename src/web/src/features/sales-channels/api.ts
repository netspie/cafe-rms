import { api, type PagedResult } from "@/lib/api"

export interface SalesChannelListItem {
  id: string
  name: string
  isTakeout: boolean
  createdAt: string
}

export interface SalesChannelDetail {
  id: string
  name: string
  isTakeout: boolean
  priceGroupIds: string[]
  createdAt: string
  updatedAt: string | null
}

export interface SalesChannelInput {
  name: string
  isTakeout: boolean
}

export interface ListSalesChannelsQuery {
  page: number
  pageSize: number
  sort?: string
  name?: string
}

export const salesChannelsApi = {
  list: (q: ListSalesChannelsQuery) => {
    const search = new URLSearchParams()
    search.set("page", String(q.page))
    search.set("pageSize", String(q.pageSize))
    if (q.sort) search.set("sort", q.sort)
    if (q.name) search.set("name", q.name)
    return api.get<PagedResult<SalesChannelListItem>>(`/api/sales-channels?${search.toString()}`)
  },
  get: (id: string) => api.get<SalesChannelDetail>(`/api/sales-channels/${id}`),
  create: (body: SalesChannelInput) => api.post<{ id: string }>("/api/sales-channels", body),
  update: (id: string, body: SalesChannelInput) => api.put<void>(`/api/sales-channels/${id}`, body),
  remove: (id: string) => api.delete<void>(`/api/sales-channels/${id}`),
  linkPriceGroup: (id: string, priceGroupId: string) =>
    api.post<void>(`/api/sales-channels/${id}/price-groups/${priceGroupId}`),
  unlinkPriceGroup: (id: string, priceGroupId: string) =>
    api.delete<void>(`/api/sales-channels/${id}/price-groups/${priceGroupId}`),
}
