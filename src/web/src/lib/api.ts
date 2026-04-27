// Hand-written fetch wrapper. Reads the JWT from the httpOnly cookie via the
// Next.js Route Handler proxy (`/api/proxy/[...path]`) so client code never has
// to touch the token. Throws ApiError with the API's ProblemDetails body parsed.

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

const PROXY_BASE = "/api/proxy"

async function request<T>(path: string, init: RequestInit = {}): Promise<T> {
  const response = await fetch(`${PROXY_BASE}${path}`, {
    ...init,
    headers: {
      "Content-Type": "application/json",
      ...(init.headers ?? {}),
    },
  })

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
