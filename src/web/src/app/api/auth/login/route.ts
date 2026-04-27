import { cookies } from "next/headers"
import { NextRequest, NextResponse } from "next/server"

const API_BASE = process.env.API_BASE_URL ?? "http://localhost:5179"

interface LoginResponse {
  accessToken: string
  expiresAt: string
  accountType: string
}

export async function POST(request: NextRequest) {
  const body = await request.json()

  const upstream = await fetch(`${API_BASE}/api/auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  })

  if (!upstream.ok) {
    const problem = await upstream.json().catch(() => ({ status: upstream.status, title: upstream.statusText }))
    return NextResponse.json(problem, { status: upstream.status })
  }

  const result = (await upstream.json()) as LoginResponse
  const expiresAt = new Date(result.expiresAt)

  const cookieStore = await cookies()
  cookieStore.set({
    name: "auth-token",
    value: result.accessToken,
    httpOnly: true,
    secure: process.env.NODE_ENV === "production",
    sameSite: "lax",
    expires: expiresAt,
    path: "/",
  })
  cookieStore.set({
    name: "account-type",
    value: result.accountType,
    httpOnly: false, // readable by client to render role-aware UI
    secure: process.env.NODE_ENV === "production",
    sameSite: "lax",
    expires: expiresAt,
    path: "/",
  })

  return NextResponse.json({ accountType: result.accountType })
}
