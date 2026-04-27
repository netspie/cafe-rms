import { api, type PagedResult } from "@/lib/api"

export type EventStatus = "Draft" | "Published" | "Closed" | "Cancelled"

export interface EventListItem {
  id: string
  name: string
  status: EventStatus
  createdAt: string
}

export interface EventDay {
  id: string
  date: string
}

export interface EventDetail {
  id: string
  name: string
  description: string | null
  imageUrl: string | null
  productListId: string | null
  priceGroupId: string | null
  status: EventStatus
  publishedAt: string | null
  closedAt: string | null
  cancelledAt: string | null
  cancellationReason: string | null
  days: EventDay[]
  createdAt: string
}

export interface EventInput {
  name: string
  description: string | null
  imageUrl: string | null
  productListId: string | null
  priceGroupId: string | null
}

export interface ListEventsQuery {
  page: number
  pageSize: number
  sort?: string
  name?: string
}

export const eventsApi = {
  list: (q: ListEventsQuery) => {
    const search = new URLSearchParams()
    search.set("page", String(q.page))
    search.set("pageSize", String(q.pageSize))
    if (q.sort) search.set("sort", q.sort)
    if (q.name) search.set("name", q.name)
    return api.get<PagedResult<EventListItem>>(`/api/events?${search.toString()}`)
  },
  get: (id: string) => api.get<EventDetail>(`/api/events/${id}`),
  create: (body: EventInput) => api.post<{ id: string }>("/api/events", body),
  update: (id: string, body: EventInput) => api.put<void>(`/api/events/${id}`, body),
  addDay: (id: string, date: string) => api.post<{ id: string }>(`/api/events/${id}/days`, { date }),
  removeDay: (id: string, dayId: string) => api.delete<void>(`/api/events/${id}/days/${dayId}`),
  publish: (id: string) => api.post<void>(`/api/events/${id}/publish`),
  close: (id: string) => api.post<void>(`/api/events/${id}/close`),
  cancel: (id: string, reason: string | null) => api.post<void>(`/api/events/${id}/cancel`, { reason }),
}
