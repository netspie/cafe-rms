import { LogOut } from "lucide-react"
import { redirect } from "next/navigation"
import { clearToken } from "@/lib/auth-cookie"
import { api } from "@/lib/server-api"

interface Me {
  email: string
  firstName: string
  lastName: string
  accountType: string
  roles: string[]
}

async function logoutAction() {
  "use server"
  await clearToken()
  redirect("/login")
}

export async function AdminTopbar() {
  const me = await api.get<Me>("/api/me").catch(() => null)

  return (
    <header className="sticky top-0 z-40 flex h-14 items-center gap-3 border-b bg-background px-4">
      <div className="flex-1" />
      {me && (
        <div className="flex items-center gap-2 text-sm">
          <span className="font-medium">
            {me.firstName || me.lastName ? `${me.firstName} ${me.lastName}`.trim() : me.email}
          </span>
          <span className="text-xs uppercase tracking-wider text-muted-foreground">
            {me.roles.length > 0 ? me.roles.join(", ") : me.accountType}
          </span>
        </div>
      )}
      <form action={logoutAction}>
        <button type="submit" className="inline-flex items-center gap-2 rounded-md px-3 py-1.5 text-sm hover:bg-accent">
          <LogOut className="h-4 w-4" />
          Sign out
        </button>
      </form>
    </header>
  )
}
