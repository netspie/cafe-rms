import { cookies } from "next/headers"
import { NextRequest, NextResponse } from "next/server"

const API_BASE = process.env.API_BASE_URL ?? "http://localhost:5179"
const TOKEN_COOKIE = "caferms-token"

export async function GET(req: NextRequest) {
  const token = (await cookies()).get(TOKEN_COOKIE)?.value
  if (!token) return new NextResponse("Unauthorized", { status: 401 })

  const search = req.nextUrl.searchParams.toString()
  const upstream = await fetch(`${API_BASE}/api/reports/events/export.pdf?${search}`, {
    headers: { Authorization: `Bearer ${token}` },
    cache: "no-store",
  })

  if (!upstream.ok) {
    const text = await upstream.text()
    return new NextResponse(text, { status: upstream.status })
  }

  return new NextResponse(upstream.body, {
    status: 200,
    headers: {
      "Content-Type": "application/pdf",
      "Content-Disposition":
        upstream.headers.get("content-disposition") ?? "attachment; filename=\"events.pdf\"",
    },
  })
}
