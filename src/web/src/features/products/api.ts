// Compatibility shim — kept only so untranslated pages on this branch
// (Orders detail, dashboard) still resolve `@/features/products/api`. The
// new Products pages call `/api/products` directly via `lib/api`. Delete
// this file in S6.

import { api, type PagedResult } from "@/lib/api"

export interface ProductListItem {
  id: string
  name: string
  barcode: string | null
  taxRateId: string
  createdAt: string
}

export interface ListProductsQuery {
  page: number
  pageSize: number
  sort?: string
  name?: string
}

export const productsApi = {
  list: (q: ListProductsQuery) => {
    const search = new URLSearchParams()
    search.set("page", String(q.page))
    search.set("pageSize", String(q.pageSize))
    if (q.sort) search.set("sort", q.sort)
    if (q.name) search.set("name", q.name)
    return api.get<PagedResult<ProductListItem>>(`/api/products?${search.toString()}`)
  },
}
