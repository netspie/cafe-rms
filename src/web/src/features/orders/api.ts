// Compatibility shim — kept only so the untranslated dashboard still resolves
// `@/features/orders/api`. Translated pages (orders list + detail) call
// `/api/orders` directly. Delete this file in S6.

import { api, type PagedResult } from "@/lib/api"

export type OrderStatus = "Placed" | "Closed" | "Cancelled"

export interface OrderListItem {
  id: string
  outletId: string
  userId: string | null
  status: OrderStatus
  createdAt: string
  closedAt: string | null
}

export interface ListOrdersQuery {
  page: number
  pageSize: number
  sort?: string
  status?: OrderStatus
}

export const ordersApi = {
  list: (q: ListOrdersQuery) => {
    const search = new URLSearchParams()
    search.set("page", String(q.page))
    search.set("pageSize", String(q.pageSize))
    if (q.sort) search.set("sort", q.sort)
    if (q.status) search.set("status", q.status)
    return api.get<PagedResult<OrderListItem>>(`/api/orders?${search.toString()}`)
  },
}
