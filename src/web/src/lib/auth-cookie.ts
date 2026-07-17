import "server-only"
import { cookies } from "next/headers"

const TOKEN_COOKIE = "caferms-token"

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
}
