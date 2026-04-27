import { NextResponse, type NextRequest } from "next/server"

export function proxy(request: NextRequest) {
  const token = request.cookies.get("auth-token")?.value
  const isAdmin = request.nextUrl.pathname.startsWith("/admin")
  const isLogin = request.nextUrl.pathname === "/login"

  if (isAdmin && !token) {
    const loginUrl = new URL("/login", request.url)
    loginUrl.searchParams.set("redirect", request.nextUrl.pathname)
    return NextResponse.redirect(loginUrl)
  }
  if (isLogin && token) {
    return NextResponse.redirect(new URL("/admin", request.url))
  }
  return NextResponse.next()
}

export const config = {
  matcher: ["/admin/:path*", "/login"],
}
