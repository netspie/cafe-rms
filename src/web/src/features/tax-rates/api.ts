import { api, type PagedResult } from "@/lib/api"

export interface TaxRateListItem {
  id: string
  name: string
  description: string
  rate: number
  createdAt: string
}

export interface TaxRateDetail {
  id: string
  name: string
  description: string
  rate: number
  createdAt: string
  updatedAt: string | null
}

export interface TaxRateInput {
  name: string
  description: string
  rate: number
}

export interface ListTaxRatesQuery {
  page: number
  pageSize: number
  sort?: string
  name?: string
}

export const taxRatesApi = {
  list: (q: ListTaxRatesQuery) => {
    const search = new URLSearchParams()
    search.set("page", String(q.page))
    search.set("pageSize", String(q.pageSize))
    if (q.sort) search.set("sort", q.sort)
    if (q.name) search.set("name", q.name)
    return api.get<PagedResult<TaxRateListItem>>(`/api/tax-rates?${search.toString()}`)
  },
  get: (id: string) => api.get<TaxRateDetail>(`/api/tax-rates/${id}`),
  create: (body: TaxRateInput) => api.post<{ id: string }>("/api/tax-rates", body),
  update: (id: string, body: TaxRateInput) => api.put<void>(`/api/tax-rates/${id}`, body),
  remove: (id: string) => api.delete<void>(`/api/tax-rates/${id}`),
}
