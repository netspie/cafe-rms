// Compatibility shim — kept only so untranslated pages on this branch
// (Products, etc.) still resolve `@/features/tags/api`. The new Tags pages
// hit `/api/tags` directly via `lib/api`. Delete this file in S6.

import { api, type PagedResult } from "@/lib/api"

export interface TagListItem {
  id: string
  name: string
  imageUrl: string | null
  createdAt: string
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
}
