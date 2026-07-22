import { redirect } from "next/navigation"
import { AdminSidebar } from "@/components/admin-sidebar"
import { AdminTopbar } from "@/components/admin-topbar"
import { FlashToaster } from "@/components/flash-toaster"
import { LiveOrdersWatcher } from "@/components/live-orders-watcher"
import { getMyPermissions } from "@/features/auth/access"
import { getToken } from "@/lib/auth-cookie"

export default async function AdminLayout({ children }: { children: React.ReactNode }) {
  const token = await getToken()
  if (!token) redirect("/login")

  const permissions = await getMyPermissions()

  return (
    <div className="flex min-h-dvh">
      <FlashToaster />
      <LiveOrdersWatcher />
      <AdminSidebar permissions={permissions} />
      <div className="flex flex-1 flex-col">
        <AdminTopbar />
        <main className="flex-1 p-6">{children}</main>
      </div>
    </div>
  )
}
