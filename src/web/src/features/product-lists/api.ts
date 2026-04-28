import { api, type PagedResult } from "@/lib/api"

export interface ProductListListItem {
  id: string
  name: string
  createdAt: string
}

export interface ProductListDetail {
  id: string
  name: string
  productIds: string[]
  createdAt: string
  updatedAt: string | null
}

export interface ProductListInput {
  name: string
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
  get: (id: string) => api.get<ProductListDetail>(`/api/product-lists/${id}`),
  create: (body: ProductListInput) => api.post<{ id: string }>("/api/product-lists", body),
  update: (id: string, body: ProductListInput) => api.put<void>(`/api/product-lists/${id}`, body),
  remove: (id: string) => api.delete<void>(`/api/product-lists/${id}`),
  addProduct: (id: string, productId: string) => api.post<void>(`/api/product-lists/${id}/items`, { productId }),
  removeProduct: (id: string, productId: string) => api.delete<void>(`/api/product-lists/${id}/items/${productId}`),
}
