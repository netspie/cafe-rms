"use client"

import { useRouter } from "next/navigation"
import { LogOut } from "lucide-react"
import { ThemeToggle } from "@/components/theme-toggle"
import { clearSession } from "@/lib/auth"

export function AdminTopbar() {
  const router = useRouter()

  function handleLogout() {
    clearSession()
    router.push("/login")
  }

  return (
    <header className="sticky top-0 z-40 flex h-14 items-center gap-3 border-b bg-background px-4">
      <div className="flex-1" />
      <ThemeToggle />
      <button
        onClick={handleLogout}
        className="inline-flex items-center gap-2 rounded-md px-3 py-1.5 text-sm hover:bg-accent"
      >
        <LogOut className="h-4 w-4" />
        Sign out
      </button>
    </header>
  )
}
