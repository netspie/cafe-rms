import { AdminSidebar } from "@/components/admin-sidebar"
import { AdminTopbar } from "@/components/admin-topbar"
import { AuthGuard } from "@/components/auth-guard"

export default function AdminLayout({ children }: { children: React.ReactNode }) {
  return (
    <AuthGuard>
      <div className="flex min-h-dvh">
        <AdminSidebar />
        <div className="flex flex-1 flex-col">
          <AdminTopbar />
          <main className="flex-1 p-6">{children}</main>
        </div>
      </div>
    </AuthGuard>
  )
}
