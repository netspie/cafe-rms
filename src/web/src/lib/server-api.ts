// Server-only fetch wrapper. Reads the JWT from an httpOnly cookie via
// Next.js `cookies()` and forwards it as a Bearer token to the C# API.
// Never imported by client code (the import-server-only marker errors at
// build time if a client bundle pulls it in).

import "server-only"
import { cookies } from "next/headers"

const API_BASE = process.env.API_BASE_URL ?? "http://localhost:5179"
const TOKEN_COOKIE = "caferms-token"
const COMPANY_COOKIE = "caferms-company"

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
}

async function request<T>(path: string, init: RequestInit = {}): Promise<T> {
  const store = await cookies()
  const token = store.get(TOKEN_COOKIE)?.value
  const companyId = store.get(COMPANY_COOKIE)?.value
  const headers: Record<string, string> = {
    "Content-Type": "application/json",
    ...(init.headers as Record<string, string> | undefined),
  }
  if (token) headers.Authorization = `Bearer ${token}`
  // SuperAdmin uses this header to context-switch to a tenant; the API
  // ignores it for Staff / Guest accounts and falls back to the JWT claim.
  if (companyId) headers["X-Company-Id"] = companyId

  const response = await fetch(`${API_BASE}${path}`, { ...init, headers, cache: "no-store" })

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
