"use client"

import { useEffect, useRef } from "react"
import { useRouter } from "next/navigation"
import { toast } from "sonner"

interface PlacedOrder {
  id: string
  tableName: string | null
  createdAt: string
}

interface PlacedOrdersPage {
  items: PlacedOrder[]
}

const POLL_MS = 7000
const LAST_SEEN_KEY = "caferms-live-orders-last-seen"

function playRing() {
  try {
    const AudioCtx =
      window.AudioContext ??
      (window as unknown as { webkitAudioContext: typeof AudioContext })
        .webkitAudioContext
    const ctx = new AudioCtx()
    const start = ctx.currentTime
    ;[880, 1320].forEach((frequency, index) => {
      const oscillator = ctx.createOscillator()
      const gain = ctx.createGain()
      oscillator.type = "sine"
      oscillator.frequency.value = frequency
      const at = start + index * 0.18
      gain.gain.setValueAtTime(0, at)
      gain.gain.linearRampToValueAtTime(0.3, at + 0.02)
      gain.gain.exponentialRampToValueAtTime(0.0001, at + 0.16)
      oscillator.connect(gain).connect(ctx.destination)
      oscillator.start(at)
      oscillator.stop(at + 0.18)
    })
    setTimeout(() => ctx.close(), 700)
  } catch {}
}

export function LiveOrdersWatcher() {
  const router = useRouter()
  const lastSeenRef = useRef<string | null>(null)

  useEffect(() => {
    lastSeenRef.current = localStorage.getItem(LAST_SEEN_KEY)

    const remember = (createdAt: string) => {
      lastSeenRef.current = createdAt
      localStorage.setItem(LAST_SEEN_KEY, createdAt)
    }

    const poll = async () => {
      try {
        const res = await fetch("/admin/live-orders/poll", { cache: "no-store" })
        if (!res.ok) return
        const data = (await res.json()) as PlacedOrdersPage
        const items = data.items ?? []
        if (items.length === 0) return

        const newest = items[0].createdAt

        if (lastSeenRef.current === null) {
          remember(newest)
          return
        }

        const seenAt = Date.parse(lastSeenRef.current)
        const fresh = items.filter((o) => Date.parse(o.createdAt) > seenAt)
        if (fresh.length === 0) return

        fresh.forEach((o) =>
          toast.success(
            `New order — ${o.tableName ? `Table ${o.tableName}` : "Walk-in"}`,
          ),
        )
        playRing()
        remember(newest)
        router.refresh()
      } catch {}
    }

    poll()
    const interval = setInterval(poll, POLL_MS)
    return () => clearInterval(interval)
  }, [router])

  return null
}
