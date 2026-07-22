"use client"

import { Suspense, useEffect } from "react"
import { usePathname, useRouter, useSearchParams } from "next/navigation"
import { toast } from "sonner"

function FlashToasterInner() {
  const params = useSearchParams()
  const router = useRouter()
  const pathname = usePathname()
  const error = params.get("error")

  useEffect(() => {
    if (!error) return
    toast.error(error)
    const next = new URLSearchParams(params)
    next.delete("error")
    router.replace(next.toString() ? `${pathname}?${next}` : pathname, { scroll: false })
  }, [error, params, pathname, router])

  return null
}

export function FlashToaster() {
  return (
    <Suspense fallback={null}>
      <FlashToasterInner />
    </Suspense>
  )
}
