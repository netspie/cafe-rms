// Transitional proxy. Untranslated client pages on this branch still call
// `/api/proxy/...` from the browser; this route reads the httpOnly auth
// cookie, forwards the request to the C# API with a Bearer header, and
// returns the upstream response. Deleted in S6 once every page is a
// server component.

import { cookies } from "next/headers"
import { NextRequest, NextResponse } from "next/server"

const API_BASE = process.env.API_BASE_URL ?? "http://localhost:5179"
const TOKEN_COOKIE = "caferms-token"

async function forward(request: NextRequest, ctx: { params: Promise<{ path: string[] }> }) {
  const { path } = await ctx.params
  const url = `${API_BASE}/${path.join("/")}${request.nextUrl.search}`
  const store = await cookies()
  const token = store.get(TOKEN_COOKIE)?.value

  const headers: Record<string, string> = {
    "Content-Type": request.headers.get("content-type") ?? "application/json",
  }
  if (token) headers.Authorization = `Bearer ${token}`

  const hasBody = request.method !== "GET" && request.method !== "HEAD"
  const upstream = await fetch(url, {
    method: request.method,
    headers,
    body: hasBody ? await request.text() : undefined,
    cache: "no-store",
  })

  return new NextResponse(upstream.body, {
    status: upstream.status,
    headers: { "content-type": upstream.headers.get("content-type") ?? "application/json" },
  })
}

export const GET = forward
export const POST = forward
export const PUT = forward
export const DELETE = forward
export const PATCH = forward
