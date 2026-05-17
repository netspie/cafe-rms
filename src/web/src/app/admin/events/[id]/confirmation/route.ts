import { cookies } from "next/headers"
import { NextResponse } from "next/server"

const API_BASE = process.env.API_BASE_URL ?? "http://localhost:5179"
const TOKEN_COOKIE = "caferms-token"

export async function GET(_req: Request, { params }: { params: Promise<{ id: string }> }) {
  const { id } = await params
  const token = (await cookies()).get(TOKEN_COOKIE)?.value
  if (!token) return new NextResponse("Unauthorized", { status: 401 })

  const upstream = await fetch(`${API_BASE}/api/events/${id}/confirmation`, {
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
      "Content-Type":
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
      "Content-Disposition":
        upstream.headers.get("content-disposition") ?? `attachment; filename="potwierdzenie-${id}.docx"`,
    },
  })
}
