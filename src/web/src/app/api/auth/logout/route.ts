import { cookies } from "next/headers"
import { NextResponse } from "next/server"

export async function POST() {
  const cookieStore = await cookies()
  cookieStore.delete("auth-token")
  cookieStore.delete("account-type")
  return NextResponse.json({ ok: true })
}
