// Compatibility shim — kept only so the untranslated dashboard still resolves
// `@/features/events/api`. Translated pages (events list + edit) call
// `/api/events` directly. Delete this file in S6.

import { api, type PagedResult } from "@/lib/api"

export interface EventListItem {
  id: string
  name: string
  status: string
  createdAt: string
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
}
