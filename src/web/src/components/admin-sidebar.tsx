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
  UserCircle2,
  type LucideIcon,
  PrinterCheck,
  Banknote,
  ListOrdered,
} from "lucide-react"

import {
  Sidebar,
  SidebarContent,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
} from "@/components/ui/sidebar"
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
    <Sidebar>
      <SidebarHeader>
        <Link href="/admin" className="flex items-center gap-2 px-2 py-3">
          <ShibaMark className="h-7 w-7" />
          <div className="flex flex-col leading-tight">
            <span className="font-semibold tracking-tight">CafeRMS</span>
            <span className="text-[10px] uppercase tracking-[0.18em] text-muted-foreground">
              いらっしゃい
            </span>
          </div>
        </Link>
      </SidebarHeader>
      <SidebarContent>
        {groups.map((group) => (
          <SidebarGroup key={group.title}>
            <SidebarGroupLabel>{group.title}</SidebarGroupLabel>
            <SidebarGroupContent>
              <SidebarMenu>
                {group.items.map((item) => {
                  const Icon = item.icon
                  const active = pathname === item.href || pathname.startsWith(`${item.href}/`)
                  return (
                    <SidebarMenuItem key={item.href}>
                      <SidebarMenuButton render={<Link href={item.href} />} isActive={active}>
                        <Icon className="h-4 w-4" />
                        <span>{item.label}</span>
                      </SidebarMenuButton>
                    </SidebarMenuItem>
                  )
                })}
              </SidebarMenu>
            </SidebarGroupContent>
          </SidebarGroup>
        ))}
      </SidebarContent>
    </Sidebar>
  )
}
