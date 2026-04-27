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

export interface OrderLine {
  id: string
  productId: string
  quantity: number
  netPerOne: number
  vatPerOne: number
}

export interface OrderDetail {
  id: string
  outletId: string
  tableId: string | null
  salesChannelId: string | null
  userId: string | null
  eventId: string | null
  promotionCodeId: string | null
  discount: number
  loyaltyPointsUsed: number
  status: OrderStatus
  closedAt: string | null
  cancelledAt: string | null
  cancellationReason: string | null
  lines: OrderLine[]
  createdAt: string
}

export interface ListOrdersQuery {
  page: number
  pageSize: number
  sort?: string
  status?: OrderStatus
  outletId?: string
  fromDate?: string
  toDate?: string
}

export const ordersApi = {
  list: (q: ListOrdersQuery) => {
    const search = new URLSearchParams()
    search.set("page", String(q.page))
    search.set("pageSize", String(q.pageSize))
    if (q.sort) search.set("sort", q.sort)
    if (q.status) search.set("status", q.status)
    if (q.outletId) search.set("outletId", q.outletId)
    if (q.fromDate) search.set("fromDate", q.fromDate)
    if (q.toDate) search.set("toDate", q.toDate)
    return api.get<PagedResult<OrderListItem>>(`/api/orders?${search.toString()}`)
  },
  get: (id: string) => api.get<OrderDetail>(`/api/orders/${id}`),
  close: (id: string) => api.post<void>(`/api/orders/${id}/close`),
  cancel: (id: string, reason: string | null) =>
    api.post<void>(`/api/orders/${id}/cancel`, { reason }),
}
