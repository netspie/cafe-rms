import { api, type PagedResult } from "@/lib/api"

export interface ProductListListItem {
  id: string
  name: string
  createdAt: string
}

export interface ListProductListsQuery {
  page: number
  pageSize: number
  sort?: string
  name?: string
}

export const productListsApi = {
  list: (q: ListProductListsQuery) => {
    const search = new URLSearchParams()
    search.set("page", String(q.page))
    search.set("pageSize", String(q.pageSize))
    if (q.sort) search.set("sort", q.sort)
    if (q.name) search.set("name", q.name)
    return api.get<PagedResult<ProductListListItem>>(`/api/product-lists?${search.toString()}`)
  },
}
