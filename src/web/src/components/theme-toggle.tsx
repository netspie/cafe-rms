"use client"

import { Moon, Sun } from "lucide-react"
import { useTheme } from "next-themes"
import { useEffect, useState } from "react"

export function ThemeToggle() {
  const { resolvedTheme, setTheme } = useTheme()
  const [mounted, setMounted] = useState(false)
  useEffect(() => setMounted(true), [])

  if (!mounted) {
    // Avoid hydration mismatch — render a placeholder on the server.
    return (
      <button aria-label="Toggle theme" className="h-8 w-8 rounded-md hover:bg-accent">
        <Sun className="mx-auto h-4 w-4" />
      </button>
    )
  }

  const isDark = resolvedTheme === "dark"
  return (
    <button
      aria-label="Toggle theme"
      onClick={() => setTheme(isDark ? "light" : "dark")}
      className="h-8 w-8 rounded-md hover:bg-accent"
    >
      {isDark ? <Sun className="mx-auto h-4 w-4" /> : <Moon className="mx-auto h-4 w-4" />}
    </button>
  )
}
