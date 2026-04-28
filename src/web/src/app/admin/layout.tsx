// Admin layout — server component. If there's no auth cookie, redirect to
// /login. No client-side guard; the cookie check runs on every request.

import { redirect } from "next/navigation"
import { AdminSidebar } from "@/components/admin-sidebar"
import { AdminTopbar } from "@/components/admin-topbar"
import { getToken } from "@/lib/auth-cookie"

export default async function AdminLayout({ children }: { children: React.ReactNode }) {
  const token = await getToken()
  if (!token) redirect("/login")

  return (
    <div className="flex min-h-dvh">
      <AdminSidebar />
      <div className="flex flex-1 flex-col">
        <AdminTopbar />
        <main className="flex-1 p-6">{children}</main>
      </div>
    </div>
  )
}
