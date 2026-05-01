import Link from "next/link"
import { Calendar, Coffee, ShoppingCart, Star } from "lucide-react"
import { ShibaMark } from "@/components/shiba-mark"
import { api, type PagedResult } from "@/lib/server-api"

interface Me { firstName: string }

async function totalOf(path: string): Promise<number | null> {
  try {
    const r = await api.get<PagedResult<unknown>>(path)
    return r.total
  } catch { return null }
}

export default async function AdminDashboard() {
  const [me, placedOrders, events, products, loyaltyEntries] = await Promise.all([
    api.get<Me>("/api/me").catch(() => null),
    totalOf("/api/orders?page=1&pageSize=1&status=Placed"),
    totalOf("/api/events?page=1&pageSize=1"),
    totalOf("/api/products?page=1&pageSize=1"),
    totalOf("/api/loyalty/entries?page=1&pageSize=1"),
  ])

  return (
    <div className="space-y-6">
      <div className="flex items-start gap-3">
        <ShibaMark className="mt-1 h-8 w-8" />
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">
            Welcome back{me ? `, ${me.firstName}` : ""}
          </h1>
        </div>
      </div>

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <StatCard href="/admin/orders" label="Open orders" value={placedOrders} icon={<ShoppingCart className="h-4 w-4" />} />
        <StatCard href="/admin/events" label="Events" value={events} icon={<Calendar className="h-4 w-4" />} />
        <StatCard href="/admin/products" label="Products" value={products} icon={<Coffee className="h-4 w-4" />} />
        <StatCard href="/admin/loyalty" label="Loyalty entries" value={loyaltyEntries} icon={<Star className="h-4 w-4" />} />
      </div>

      <section className="rounded-lg border bg-card p-4">
        <h2 className="mb-4 text-base font-semibold">What&apos;s next?</h2>
        <div className="grid gap-3 text-sm sm:grid-cols-2">
          <QuickLink href="/admin/products" title="Add a product" />
          <QuickLink href="/admin/events" title="Plan an event" />
          <QuickLink href="/admin/users" title="Invite a teammate" />
          <QuickLink href="/admin/settings" title="Update café details" />
        </div>
      </section>
    </div>
  )
}

function StatCard({ href, label, value, icon }: { href: string; label: string; value: number | null; icon: React.ReactNode }) {
  return (
    <Link href={href} className="block rounded-lg border bg-card p-4 transition-colors hover:border-primary/50">
      <div className="flex items-start justify-between gap-2">
        <p className="text-sm font-medium text-muted-foreground">{label}</p>
        <span className="text-muted-foreground">{icon}</span>
      </div>
      <p className="mt-2 text-2xl font-semibold">{value === null ? "—" : value}</p>
    </Link>
  )
}

function QuickLink({ href, title }: { href: string; title: string }) {
  return (
    <Link href={href} className="rounded-md border px-3 py-2 hover:border-primary/50">
      <p className="font-medium">{title}</p>
    </Link>
  )
}
