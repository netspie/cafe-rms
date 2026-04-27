import { cookies } from "next/headers"
import { NextRequest, NextResponse } from "next/server"

// Server-side proxy that forwards every /api/proxy/<path> call to the C# API,
// attaching the JWT from the httpOnly cookie. Keeps the token off the client.

const API_BASE = process.env.API_BASE_URL ?? "http://localhost:5179"

async function proxy(request: NextRequest, path: string[]) {
  const cookieStore = await cookies()
  const token = cookieStore.get("auth-token")?.value

  const url = new URL(`${API_BASE}/${path.join("/")}`)
  request.nextUrl.searchParams.forEach((v, k) => url.searchParams.append(k, v))

  const headers: Record<string, string> = {}
  request.headers.forEach((value, key) => {
    if (key === "host" || key === "connection") return
    headers[key] = value
  })
  if (token) headers["authorization"] = `Bearer ${token}`

  const init: RequestInit = {
    method: request.method,
    headers,
    body: ["GET", "HEAD"].includes(request.method) ? undefined : await request.text(),
    cache: "no-store",
  }

  const upstream = await fetch(url.toString(), init)

  const responseHeaders = new Headers()
  upstream.headers.forEach((value, key) => {
    if (key === "transfer-encoding" || key === "content-encoding") return
    responseHeaders.set(key, value)
  })

  return new NextResponse(upstream.body, {
    status: upstream.status,
    headers: responseHeaders,
  })
}

type Ctx = { params: Promise<{ path: string[] }> }
export async function GET(request: NextRequest, ctx: Ctx) {
  return proxy(request, (await ctx.params).path)
}
export async function POST(request: NextRequest, ctx: Ctx) {
  return proxy(request, (await ctx.params).path)
}
export async function PUT(request: NextRequest, ctx: Ctx) {
  return proxy(request, (await ctx.params).path)
}
export async function DELETE(request: NextRequest, ctx: Ctx) {
  return proxy(request, (await ctx.params).path)
}
