"use client"

import Link from "next/link"
import { usePathname } from "next/navigation"
import { ShibaMark } from "@/components/shiba-mark"
import { NAV_GROUPS } from "@/features/auth/nav"
import type { Permission } from "@/features/auth/permissions"

export function AdminSidebar({ permissions }: { permissions: Permission[] }) {
  const pathname = usePathname()
  const groups = NAV_GROUPS.map((group) => ({
    ...group,
    items: group.items.filter((item) => item.permission === undefined || permissions.includes(item.permission)),
  })).filter((group) => group.items.length > 0)

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
