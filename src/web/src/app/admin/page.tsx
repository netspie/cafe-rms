"use client"

// Admin dashboard. Stat cards click through to the relevant list. Counts
// come from each list endpoint with pageSize=1 — we only need the `total`.

import Link from "next/link"
import { useEffect, useState } from "react"
import { Calendar, Coffee, ShoppingCart, Star } from "lucide-react"

import { ShibaMark } from "@/components/shiba-mark"
import { api, type PagedResult } from "@/lib/api"

interface Counts {
  placedOrders: number | null
  events: number | null
  products: number | null
  loyaltyEntries: number | null
}

interface Me { firstName: string }

export default function AdminDashboard() {
  const [me, setMe] = useState<Me | null>(null)
  const [counts, setCounts] = useState<Counts>({ placedOrders: null, events: null, products: null, loyaltyEntries: null })

  useEffect(() => {
    api.get<Me>("/api/me").then(setMe).catch(() => {})

    async function pull(path: string): Promise<number | null> {
      try {
        const r = await api.get<PagedResult<unknown>>(path)
        return r.total
      } catch { return null }
    }

    Promise.all([
      pull("/api/orders?page=1&pageSize=1&status=Placed"),
      pull("/api/events?page=1&pageSize=1"),
      pull("/api/products?page=1&pageSize=1"),
      pull("/api/loyalty/entries?page=1&pageSize=1"),
    ]).then(([placedOrders, events, products, loyaltyEntries]) => {
      setCounts({ placedOrders, events, products, loyaltyEntries })
    })
  }, [])

  return (
    <div className="space-y-6">
      <div className="flex items-start gap-3">
        <ShibaMark className="mt-1 h-8 w-8" />
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">
            Welcome back{me ? `, ${me.firstName}` : ""}
          </h1>
          <p className="text-muted-foreground">A quick view of your café — click any card to drill in.</p>
        </div>
      </div>

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <StatCard href="/admin/orders" label="Open orders" value={counts.placedOrders} icon={<ShoppingCart className="h-4 w-4" />} hint="Status = Placed" />
        <StatCard href="/admin/events" label="Events" value={counts.events} icon={<Calendar className="h-4 w-4" />} hint="All statuses" />
        <StatCard href="/admin/products" label="Products" value={counts.products} icon={<Coffee className="h-4 w-4" />} />
        <StatCard href="/admin/loyalty" label="Loyalty entries" value={counts.loyaltyEntries} icon={<Star className="h-4 w-4" />} hint="Earned + redeemed + adjustments" />
      </div>

      <section className="rounded-lg border bg-card p-4">
        <h2 className="text-base font-semibold">What&apos;s next?</h2>
        <p className="mb-4 text-xs text-muted-foreground">Common starting points.</p>
        <div className="grid gap-3 text-sm sm:grid-cols-2">
          <QuickLink href="/admin/products" title="Add a product" hint="New menu item with tags, allergens, and prices." />
          <QuickLink href="/admin/events" title="Plan an event" hint="Set days, then publish to make it visible." />
          <QuickLink href="/admin/users" title="Invite a teammate" hint="Create a staff account and assign roles." />
          <QuickLink href="/admin/settings" title="Update café details" hint="Address, currency, logo — shown to customers." />
        </div>
      </section>
    </div>
  )
}

function StatCard({ href, label, value, icon, hint }: { href: string; label: string; value: number | null; icon: React.ReactNode; hint?: string }) {
  return (
    <Link href={href} className="block rounded-lg border bg-card p-4 transition-colors hover:border-primary/50">
      <div className="flex items-start justify-between gap-2">
        <p className="text-sm font-medium text-muted-foreground">{label}</p>
        <span className="text-muted-foreground">{icon}</span>
      </div>
      <p className="mt-2 text-2xl font-semibold">{value === null ? "…" : value}</p>
      {hint && <p className="mt-1 text-xs text-muted-foreground">{hint}</p>}
    </Link>
  )
}

function QuickLink({ href, title, hint }: { href: string; title: string; hint: string }) {
  return (
    <Link href={href} className="rounded-md border px-3 py-2 hover:border-primary/50">
      <p className="font-medium">{title}</p>
      <p className="text-muted-foreground">{hint}</p>
    </Link>
  )
}
