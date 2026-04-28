"use client"

import Link from "next/link"
import { useQuery } from "@tanstack/react-query"
import { Calendar, Coffee, ShoppingCart, Star } from "lucide-react"

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Skeleton } from "@/components/ui/skeleton"
import { ShibaMark } from "@/components/shiba-mark"
import { ordersApi } from "@/features/orders/api"
import { eventsApi } from "@/features/events/api"
import { productsApi } from "@/features/products/api"
import { loyaltyApi } from "@/features/loyalty/api"
import { meApi } from "@/features/me/api"

interface StatCardProps {
  href: string
  label: string
  value: string | number
  isLoading: boolean
  icon: React.ReactNode
  hint?: string
}

function StatCard({ href, label, value, isLoading, icon, hint }: StatCardProps) {
  return (
    <Link href={href} className="block">
      <Card className="transition-colors hover:border-primary/50">
        <CardHeader className="flex flex-row items-start justify-between gap-2 pb-2">
          <CardTitle className="text-sm font-medium text-muted-foreground">{label}</CardTitle>
          <div className="text-muted-foreground">{icon}</div>
        </CardHeader>
        <CardContent>
          {isLoading ? <Skeleton className="h-8 w-16" /> : <p className="text-2xl font-semibold">{value}</p>}
          {hint && <p className="mt-1 text-xs text-muted-foreground">{hint}</p>}
        </CardContent>
      </Card>
    </Link>
  )
}

export default function AdminDashboard() {
  const meQuery = useQuery({ queryKey: ["me"], queryFn: () => meApi.get() })

  const placedOrders = useQuery({
    queryKey: ["orders", "dashboard-placed"],
    queryFn: () => ordersApi.list({ page: 1, pageSize: 1, status: "Placed" }),
  })

  const publishedEvents = useQuery({
    queryKey: ["events", "dashboard-published"],
    queryFn: () => eventsApi.list({ page: 1, pageSize: 1 }),
  })

  const productCount = useQuery({
    queryKey: ["products", "dashboard-total"],
    queryFn: () => productsApi.list({ page: 1, pageSize: 1 }),
  })

  const loyaltyActivity = useQuery({
    queryKey: ["loyalty-entries", "dashboard"],
    queryFn: () => loyaltyApi.list({ page: 1, pageSize: 1 }),
  })

  return (
    <div className="space-y-6">
      <div className="flex items-start gap-3">
        <ShibaMark className="h-8 w-8 mt-1" />
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">
            Welcome back{meQuery.data ? `, ${meQuery.data.firstName}` : ""}
          </h1>
          <p className="text-muted-foreground">
            A quick view of your café — click any card to drill in.
          </p>
        </div>
      </div>

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <StatCard
          href="/admin/orders"
          label="Open orders"
          value={placedOrders.data?.total ?? 0}
          isLoading={placedOrders.isLoading}
          icon={<ShoppingCart className="h-4 w-4" />}
          hint="Status = Placed"
        />
        <StatCard
          href="/admin/events"
          label="Events"
          value={publishedEvents.data?.total ?? 0}
          isLoading={publishedEvents.isLoading}
          icon={<Calendar className="h-4 w-4" />}
          hint="All statuses"
        />
        <StatCard
          href="/admin/products"
          label="Products"
          value={productCount.data?.total ?? 0}
          isLoading={productCount.isLoading}
          icon={<Coffee className="h-4 w-4" />}
        />
        <StatCard
          href="/admin/loyalty"
          label="Loyalty entries"
          value={loyaltyActivity.data?.total ?? 0}
          isLoading={loyaltyActivity.isLoading}
          icon={<Star className="h-4 w-4" />}
          hint="Earned + redeemed + adjustments"
        />
      </div>

      <Card>
        <CardHeader>
          <CardTitle>What&apos;s next?</CardTitle>
          <CardDescription>Common starting points.</CardDescription>
        </CardHeader>
        <CardContent className="grid gap-3 text-sm sm:grid-cols-2">
          <Link href="/admin/products/new" className="rounded-md border px-3 py-2 hover:border-primary/50">
            <p className="font-medium">Add a product</p>
            <p className="text-muted-foreground">New menu item with tags, allergens, and prices.</p>
          </Link>
          <Link href="/admin/events/new" className="rounded-md border px-3 py-2 hover:border-primary/50">
            <p className="font-medium">Plan an event</p>
            <p className="text-muted-foreground">Set days, then publish to make it visible.</p>
          </Link>
          <Link href="/admin/users/new" className="rounded-md border px-3 py-2 hover:border-primary/50">
            <p className="font-medium">Invite a teammate</p>
            <p className="text-muted-foreground">Create a staff account and assign roles.</p>
          </Link>
          <Link href="/admin/settings" className="rounded-md border px-3 py-2 hover:border-primary/50">
            <p className="font-medium">Update café details</p>
            <p className="text-muted-foreground">Address, currency, logo — shown to customers.</p>
          </Link>
        </CardContent>
      </Card>
    </div>
  )
}
