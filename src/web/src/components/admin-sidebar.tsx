"use client"

import Link from "next/link"
import { usePathname } from "next/navigation"
import {
  Tags,
  Coffee,
  Receipt,
  Layers,
  Boxes,
  Sliders,
  Settings,
  Tag,
  Calendar,
  ShoppingCart,
  Users,
  Shield,
  Heart,
  Star,
  PrinterCheck,
  Banknote,
  ListOrdered,
  UserCircle2,
  type LucideIcon,
} from "lucide-react"
import { ShibaMark } from "@/components/shiba-mark"

interface NavItem {
  href: string
  label: string
  icon: LucideIcon
}

const groups: { title: string; items: NavItem[] }[] = [
  {
    title: "Catalog",
    items: [
      { href: "/admin/products", label: "Products", icon: Coffee },
      { href: "/admin/tags", label: "Tags", icon: Tag },
      { href: "/admin/allergens", label: "Allergens", icon: Heart },
      { href: "/admin/modifier-groups", label: "Modifier groups", icon: Layers },
      { href: "/admin/modifiers", label: "Modifiers", icon: Sliders },
      { href: "/admin/product-lists", label: "Product lists", icon: Boxes },
    ],
  },
  {
    title: "Pricing & sales",
    items: [
      { href: "/admin/tax-rates", label: "Tax rates", icon: Receipt },
      { href: "/admin/price-groups", label: "Price groups", icon: Banknote },
      { href: "/admin/sales-channels", label: "Sales channels", icon: ListOrdered },
      { href: "/admin/promotion-codes", label: "Promotion codes", icon: Tags },
    ],
  },
  {
    title: "Operations",
    items: [
      { href: "/admin/orders", label: "Orders", icon: ShoppingCart },
      { href: "/admin/events", label: "Events", icon: Calendar },
      { href: "/admin/tables", label: "Tables", icon: Layers },
      { href: "/admin/loyalty", label: "Loyalty", icon: Star },
    ],
  },
  {
    title: "Settings",
    items: [
      { href: "/admin/users", label: "Users", icon: Users },
      { href: "/admin/roles", label: "Roles", icon: Shield },
      { href: "/admin/printout-templates", label: "Printout templates", icon: PrinterCheck },
      { href: "/admin/settings", label: "Company & outlet", icon: Settings },
      { href: "/admin/account", label: "My account", icon: UserCircle2 },
    ],
  },
]

export function AdminSidebar() {
  const pathname = usePathname()
  return (
    <aside className="flex w-64 shrink-0 flex-col border-r bg-card">
      <Link href="/admin" className="flex items-center gap-2 border-b px-3 py-3">
        <ShibaMark className="h-7 w-7" />
        <div className="flex flex-col leading-tight">
          <span className="font-semibold tracking-tight">CafeRMS</span>
          <span className="text-[10px] uppercase tracking-[0.18em] text-muted-foreground">いらっしゃい</span>
        </div>
      </Link>
      <nav className="flex-1 overflow-y-auto px-2 py-3">
        {groups.map((group) => (
          <div key={group.title} className="mb-4">
            <p className="px-2 pb-1 text-[10px] font-medium uppercase tracking-wider text-muted-foreground">
              {group.title}
            </p>
            <ul className="space-y-0.5">
              {group.items.map((item) => {
                const Icon = item.icon
                const active = pathname === item.href || pathname.startsWith(`${item.href}/`)
                return (
                  <li key={item.href}>
                    <Link
                      href={item.href}
                      className={
                        "flex items-center gap-2 rounded-md px-2 py-1.5 text-sm transition-colors " +
                        (active ? "bg-accent text-accent-foreground font-medium" : "text-muted-foreground hover:bg-accent/50 hover:text-foreground")
                      }
                    >
                      <Icon className="h-4 w-4" />
                      <span>{item.label}</span>
                    </Link>
                  </li>
                )
              })}
            </ul>
          </div>
        ))}
      </nav>
    </aside>
  )
}
