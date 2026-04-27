import { api, type PagedResult } from "@/lib/api"

export interface TableListItem {
  id: string
  name: string
  outletId: string
  createdAt: string
}

export interface TableDetail {
  id: string
  name: string
  outletId: string
  createdAt: string
  updatedAt: string | null
}

export interface TableInput {
  name: string
}

export interface ListTablesQuery {
  page: number
  pageSize: number
  sort?: string
  name?: string
}

export const tablesApi = {
  list: (q: ListTablesQuery) => {
    const search = new URLSearchParams()
    search.set("page", String(q.page))
    search.set("pageSize", String(q.pageSize))
    if (q.sort) search.set("sort", q.sort)
    if (q.name) search.set("name", q.name)
    return api.get<PagedResult<TableListItem>>(`/api/tables?${search.toString()}`)
  },
  get: (id: string) => api.get<TableDetail>(`/api/tables/${id}`),
  create: (body: TableInput) => api.post<{ id: string }>("/api/tables", body),
  update: (id: string, body: TableInput) => api.put<void>(`/api/tables/${id}`, body),
  remove: (id: string) => api.delete<void>(`/api/tables/${id}`),
}
