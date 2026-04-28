// Raw fetch wrapper. Reads the JWT from localStorage and attaches it as
// "Authorization: Bearer ...". On 401 it clears the session and bounces to
// /login. Errors come back as ApiError with the API's ProblemDetails parsed,
// including the per-field `errors` map FluentValidation emits on 422.

import { getToken, clearSession, redirectToLogin } from "./auth"

export interface ProblemDetails {
  type?: string
  title?: string
  status?: number
  detail?: string
  errors?: Record<string, string[]>
}

export class ApiError extends Error {
  constructor(public status: number, public problem: ProblemDetails) {
    super(problem.detail ?? problem.title ?? `HTTP ${status}`)
  }
  fieldError(name: string): string | undefined {
    return this.problem.errors?.[name]?.[0]
  }
}

const API_BASE = process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5179"

async function request<T>(path: string, init: RequestInit = {}): Promise<T> {
  const token = getToken()
  const headers: Record<string, string> = {
    "Content-Type": "application/json",
    ...(init.headers as Record<string, string> | undefined),
  }
  if (token) headers.Authorization = `Bearer ${token}`

  const response = await fetch(`${API_BASE}${path}`, { ...init, headers })

  if (response.status === 401) {
    clearSession()
    redirectToLogin()
    throw new ApiError(401, { status: 401, title: "Session expired" })
  }

  if (response.status === 204) return undefined as T

  const contentType = response.headers.get("content-type") ?? ""
  const isJson = contentType.includes("application/json") || contentType.includes("application/problem+json")
  const body = isJson ? await response.json() : null

  if (!response.ok) {
    throw new ApiError(response.status, body ?? { status: response.status, title: response.statusText })
  }

  return body as T
}

export const api = {
  get: <T>(path: string) => request<T>(path, { method: "GET" }),
  post: <T>(path: string, body?: unknown) =>
    request<T>(path, { method: "POST", body: body !== undefined ? JSON.stringify(body) : undefined }),
  put: <T>(path: string, body?: unknown) =>
    request<T>(path, { method: "PUT", body: body !== undefined ? JSON.stringify(body) : undefined }),
  delete: <T>(path: string) => request<T>(path, { method: "DELETE" }),
}

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  total: number
}
