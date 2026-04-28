import { api, type PagedResult } from "@/lib/api"

export interface PromotionCodeListItem {
  id: string
  code: string
  discountPercentage: number
  validFrom: string | null
  validUntil: string | null
  maxUses: number | null
  usesCount: number
  createdAt: string
}

export interface PromotionCodeDetail {
  id: string
  code: string
  discountPercentage: number
  validFrom: string | null
  validUntil: string | null
  maxUses: number | null
  usesCount: number
  createdAt: string
  updatedAt: string | null
}

export interface PromotionCodeInput {
  code: string
  discountPercentage: number
  validFrom: string | null
  validUntil: string | null
  maxUses: number | null
}

export interface ListPromotionCodesQuery {
  page: number
  pageSize: number
  sort?: string
  code?: string
}

export const promotionCodesApi = {
  list: (q: ListPromotionCodesQuery) => {
    const search = new URLSearchParams()
    search.set("page", String(q.page))
    search.set("pageSize", String(q.pageSize))
    if (q.sort) search.set("sort", q.sort)
    if (q.code) search.set("code", q.code)
    return api.get<PagedResult<PromotionCodeListItem>>(`/api/promotion-codes?${search.toString()}`)
  },
  get: (id: string) => api.get<PromotionCodeDetail>(`/api/promotion-codes/${id}`),
  create: (body: PromotionCodeInput) => api.post<{ id: string }>("/api/promotion-codes", body),
  update: (id: string, body: PromotionCodeInput) => api.put<void>(`/api/promotion-codes/${id}`, body),
  remove: (id: string) => api.delete<void>(`/api/promotion-codes/${id}`),
}
