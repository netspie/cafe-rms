import { cookies } from "next/headers"
import { NextResponse } from "next/server"

const API_BASE = process.env.API_BASE_URL ?? "http://localhost:5179"
const TOKEN_COOKIE = "caferms-token"

export async function GET() {
  const token = (await cookies()).get(TOKEN_COOKIE)?.value
  if (!token) return new NextResponse("Unauthorized", { status: 401 })

  const upstream = await fetch(
    `${API_BASE}/api/orders?status=Placed&sort=-createdAt&pageSize=20`,
    { headers: { Authorization: `Bearer ${token}` }, cache: "no-store" },
  )
  if (!upstream.ok) return new NextResponse("Upstream error", { status: upstream.status })

  return NextResponse.json(await upstream.json())
}
