import { api, type PagedResult } from "@/lib/api"

export interface TagListItem {
  id: string
  name: string
  imageUrl: string | null
  createdAt: string
}

export interface TagDetail {
  id: string
  name: string
  imageUrl: string | null
  createdAt: string
  updatedAt: string | null
}

export interface TagInput {
  name: string
  imageUrl?: string | null
}

export interface ListTagsQuery {
  page: number
  pageSize: number
  sort?: string
  name?: string
}

export const tagsApi = {
  list: (q: ListTagsQuery) => {
    const search = new URLSearchParams()
    search.set("page", String(q.page))
    search.set("pageSize", String(q.pageSize))
    if (q.sort) search.set("sort", q.sort)
    if (q.name) search.set("name", q.name)
    return api.get<PagedResult<TagListItem>>(`/api/tags?${search.toString()}`)
  },
  get: (id: string) => api.get<TagDetail>(`/api/tags/${id}`),
  create: (body: TagInput) => api.post<{ id: string }>("/api/tags", body),
  update: (id: string, body: TagInput) => api.put<void>(`/api/tags/${id}`, body),
  remove: (id: string) => api.delete<void>(`/api/tags/${id}`),
}
