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
  Banknote,
  ListOrdered,
  UserCircle2,
  BarChart3,
  Package,
  type LucideIcon,
} from "lucide-react"
import type { Permission } from "./permissions"

export interface NavItem {
  href: string
  label: string
  icon: LucideIcon
  permission?: Permission
}

export interface NavGroup {
  title: string
  items: NavItem[]
}

export const NAV_GROUPS: NavGroup[] = [
  {
    title: "Catalog",
    items: [
      { href: "/admin/products", label: "Products", icon: Coffee, permission: "ProductsManage" },
      { href: "/admin/tags", label: "Tags", icon: Tag, permission: "ProductsManage" },
      { href: "/admin/allergens", label: "Allergens", icon: Heart, permission: "ProductsManage" },
      { href: "/admin/modifier-groups", label: "Modifier groups", icon: Layers, permission: "ModifiersManage" },
      { href: "/admin/modifiers", label: "Modifiers", icon: Sliders, permission: "ModifiersManage" },
      { href: "/admin/product-lists", label: "Product lists", icon: Boxes, permission: "MenusManage" },
    ],
  },
  {
    title: "Pricing & sales",
    items: [
      { href: "/admin/tax-rates", label: "Tax rates", icon: Receipt, permission: "TaxRatesManage" },
      { href: "/admin/price-groups", label: "Price groups", icon: Banknote, permission: "PricingManage" },
      { href: "/admin/sales-channels", label: "Sales channels", icon: ListOrdered, permission: "SalesChannelsManage" },
      { href: "/admin/promotion-codes", label: "Promotion codes", icon: Tags, permission: "PromotionsManage" },
    ],
  },
  {
    title: "Operations",
    items: [
      { href: "/admin/orders", label: "Orders", icon: ShoppingCart, permission: "OrdersView" },
      { href: "/admin/events", label: "Events", icon: Calendar, permission: "EventsManage" },
      { href: "/admin/tables", label: "Tables", icon: Layers, permission: "TablesManage" },
      { href: "/admin/loyalty", label: "Loyalty", icon: Star, permission: "LoyaltyManage" },
    ],
  },
  {
    title: "Reports",
    items: [
      { href: "/admin/reports/sales", label: "Sales", icon: BarChart3, permission: "ReportsView" },
      { href: "/admin/reports/products", label: "Sales per product", icon: Package, permission: "ReportsView" },
      { href: "/admin/reports/events", label: "Event attendance", icon: Calendar, permission: "ReportsView" },
    ],
  },
  {
    title: "Settings",
    items: [
      { href: "/admin/users", label: "Users", icon: Users, permission: "UsersManage" },
      { href: "/admin/roles", label: "Roles", icon: Shield, permission: "RolesManage" },
      { href: "/admin/settings", label: "Company & outlet", icon: Settings, permission: "OutletManage" },
      { href: "/admin/account", label: "My account", icon: UserCircle2 },
    ],
  },
]

export const PERMISSION_BY_HREF: Record<string, Permission> = Object.fromEntries(
  NAV_GROUPS.flatMap((group) => group.items)
    .filter((item) => item.permission !== undefined)
    .map((item) => [item.href, item.permission!])
)
