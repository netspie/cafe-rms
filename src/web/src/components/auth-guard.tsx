"use client"

import { useEffect, useState } from "react"
import { useRouter, usePathname } from "next/navigation"
import { getToken } from "@/lib/auth"

export function AuthGuard({ children }: { children: React.ReactNode }) {
  const router = useRouter()
  const pathname = usePathname()
  const [ready, setReady] = useState(false)

  useEffect(() => {
    if (!getToken()) {
      router.replace(`/login?redirect=${encodeURIComponent(pathname)}`)
      return
    }
    setReady(true)
  }, [router, pathname])

  if (!ready) return null
  return <>{children}</>
}
