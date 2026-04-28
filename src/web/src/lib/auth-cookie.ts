// Server-only auth cookie helpers. The token lives in an httpOnly cookie so
// the browser never sees it; server components and server actions read it
// via `cookies()`. Same pattern for the SuperAdmin company-context cookie:
// the API expects `X-Company-Id` on every request when a SuperAdmin is
// pretending to be a specific tenant; the cookie persists that selection
// across page loads.

import "server-only"
import { cookies } from "next/headers"

const TOKEN_COOKIE = "caferms-token"
const COMPANY_COOKIE = "caferms-company"

export async function getToken(): Promise<string | null> {
  const store = await cookies()
  return store.get(TOKEN_COOKIE)?.value ?? null
}

export async function setToken(token: string, expiresAt: Date) {
  const store = await cookies()
  store.set(TOKEN_COOKIE, token, {
    httpOnly: true,
    secure: process.env.NODE_ENV === "production",
    sameSite: "lax",
    expires: expiresAt,
    path: "/",
  })
}

export async function clearToken() {
  const store = await cookies()
  store.delete(TOKEN_COOKIE)
  store.delete(COMPANY_COOKIE)
}

// SuperAdmin company context — only meaningful for the SuperAdmin account
// type; the JWT already carries companyId for Staff / Guest users and the
// API ignores X-Company-Id for them.

export async function getCompanyId(): Promise<string | null> {
  const store = await cookies()
  return store.get(COMPANY_COOKIE)?.value ?? null
}

export async function setCompanyId(id: string) {
  const store = await cookies()
  store.set(COMPANY_COOKIE, id, {
    httpOnly: true,
    secure: process.env.NODE_ENV === "production",
    sameSite: "lax",
    path: "/",
  })
}

export async function clearCompanyId() {
  const store = await cookies()
  store.delete(COMPANY_COOKIE)
}
