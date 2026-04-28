// Topbar — server component. Sign-out is a tiny `<form>` whose action is
// a server function: clears the cookie and redirects to /login.

import { LogOut } from "lucide-react"
import { redirect } from "next/navigation"
import { ThemeToggle } from "@/components/theme-toggle"
import { clearToken } from "@/lib/auth-cookie"

async function logoutAction() {
  "use server"
  await clearToken()
  redirect("/login")
}

export function AdminTopbar() {
  return (
    <header className="sticky top-0 z-40 flex h-14 items-center gap-3 border-b bg-background px-4">
      <div className="flex-1" />
      <ThemeToggle />
      <form action={logoutAction}>
        <button type="submit" className="inline-flex items-center gap-2 rounded-md px-3 py-1.5 text-sm hover:bg-accent">
          <LogOut className="h-4 w-4" />
          Sign out
        </button>
      </form>
    </header>
  )
}
