import { api, type PagedResult } from "@/lib/api"

export interface ProductListItem {
  id: string
  name: string
  barcode: string | null
  taxRateId: string
  createdAt: string
}

export interface ProductImage {
  id: string
  url: string
}

export interface ProductPrice {
  priceGroupId: string
  net: number
}

export interface ProductDetail {
  id: string
  name: string
  description: string | null
  barcode: string | null
  taxRateId: string
  tagIds: string[]
  allergenIds: string[]
  modifierGroupIds: string[]
  images: ProductImage[]
  prices: ProductPrice[]
  createdAt: string
  updatedAt: string | null
}

export interface ProductInput {
  name: string
  description: string | null
  barcode: string | null
  taxRateId: string
}

export interface ListProductsQuery {
  page: number
  pageSize: number
  sort?: string
  name?: string
  barcode?: string
  taxRateId?: string
}

export const productsApi = {
  list: (q: ListProductsQuery) => {
    const search = new URLSearchParams()
    search.set("page", String(q.page))
    search.set("pageSize", String(q.pageSize))
    if (q.sort) search.set("sort", q.sort)
    if (q.name) search.set("name", q.name)
    if (q.barcode) search.set("barcode", q.barcode)
    if (q.taxRateId) search.set("taxRateId", q.taxRateId)
    return api.get<PagedResult<ProductListItem>>(`/api/products?${search.toString()}`)
  },
  get: (id: string) => api.get<ProductDetail>(`/api/products/${id}`),
  create: (body: ProductInput) => api.post<{ id: string }>("/api/products", body),
  update: (id: string, body: ProductInput) => api.put<void>(`/api/products/${id}`, body),
  remove: (id: string) => api.delete<void>(`/api/products/${id}`),

  addImage: (id: string, url: string) => api.post<{ id: string }>(`/api/products/${id}/images`, { url }),
  removeImage: (id: string, imageId: string) => api.delete<void>(`/api/products/${id}/images/${imageId}`),

  setPrice: (id: string, priceGroupId: string, net: number) =>
    api.put<void>(`/api/products/${id}/prices/${priceGroupId}`, { net }),

  attachTag: (id: string, tagId: string) => api.post<void>(`/api/products/${id}/tags/${tagId}`),
  detachTag: (id: string, tagId: string) => api.delete<void>(`/api/products/${id}/tags/${tagId}`),

  attachAllergen: (id: string, allergenId: string) => api.post<void>(`/api/products/${id}/allergens/${allergenId}`),
  detachAllergen: (id: string, allergenId: string) => api.delete<void>(`/api/products/${id}/allergens/${allergenId}`),

  attachModifierGroup: (id: string, modifierGroupId: string) =>
    api.post<void>(`/api/products/${id}/modifier-groups/${modifierGroupId}`),
  detachModifierGroup: (id: string, modifierGroupId: string) =>
    api.delete<void>(`/api/products/${id}/modifier-groups/${modifierGroupId}`),
}
