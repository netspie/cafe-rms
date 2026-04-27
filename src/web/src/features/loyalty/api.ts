import { api, type PagedResult } from "@/lib/api"

export interface LoyaltyEntryItem {
  id: string
  userId: string
  points: number
  reason: string | null
  createdAt: string
}

export interface AdjustmentInput {
  userId: string
  points: number
  reason: string
}

export interface ListLoyaltyEntriesQuery {
  page: number
  pageSize: number
  sort?: string
  userId?: string
}

export const loyaltyApi = {
  list: (q: ListLoyaltyEntriesQuery) => {
    const search = new URLSearchParams()
    search.set("page", String(q.page))
    search.set("pageSize", String(q.pageSize))
    if (q.sort) search.set("sort", q.sort)
    if (q.userId) search.set("userId", q.userId)
    return api.get<PagedResult<LoyaltyEntryItem>>(`/api/loyalty/entries?${search.toString()}`)
  },
  addAdjustment: (body: AdjustmentInput) =>
    api.post<{ id: string }>("/api/loyalty/entries", body),
}
