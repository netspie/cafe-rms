import { api, type PagedResult } from "@/lib/api"

export interface PrintoutTemplateListItem {
  id: string
  name: string
  templateFileUrl: string
  createdAt: string
}

export interface PrintoutTemplateDetail {
  id: string
  name: string
  templateFileUrl: string
  createdAt: string
  updatedAt: string | null
}

export interface PrintoutTemplateInput {
  name: string
  templateFileUrl: string
}

export interface ListPrintoutTemplatesQuery {
  page: number
  pageSize: number
  sort?: string
  name?: string
}

export const printoutTemplatesApi = {
  list: (q: ListPrintoutTemplatesQuery) => {
    const search = new URLSearchParams()
    search.set("page", String(q.page))
    search.set("pageSize", String(q.pageSize))
    if (q.sort) search.set("sort", q.sort)
    if (q.name) search.set("name", q.name)
    return api.get<PagedResult<PrintoutTemplateListItem>>(`/api/printout-templates?${search.toString()}`)
  },
  get: (id: string) => api.get<PrintoutTemplateDetail>(`/api/printout-templates/${id}`),
  create: (body: PrintoutTemplateInput) => api.post<{ id: string }>("/api/printout-templates", body),
  update: (id: string, body: PrintoutTemplateInput) => api.put<void>(`/api/printout-templates/${id}`, body),
  remove: (id: string) => api.delete<void>(`/api/printout-templates/${id}`),
}
