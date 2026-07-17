import "server-only"
import { cache } from "react"
import { redirect } from "next/navigation"
import { api } from "@/lib/server-api"
import type { Permission } from "./permissions"
import { PERMISSION_BY_HREF } from "./nav"

export interface Me {
  userId: string
  email: string
  firstName: string
  lastName: string
  accountType: string
  outletId: string | null
  defaultPriceGroupId: string | null
  defaultProductListId: string | null
  roles: string[]
  permissions: Permission[]
}

export const getMe = cache(() => api.get<Me>("/api/me"))

export async function getMyPermissions(): Promise<Permission[]> {
  const me = await getMe()
  return me.permissions
}

export async function requirePermissionFor(href: string) {
  const required = PERMISSION_BY_HREF[href]
  if (required === undefined)
    return

  const permissions = await getMyPermissions()
  if (!permissions.includes(required))
    redirect("/admin")
}
