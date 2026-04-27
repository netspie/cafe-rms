import { api } from "@/lib/api"

export type AccountType = "SuperAdmin" | "Staff" | "Guest"

export interface MeResult {
  userId: string
  email: string
  firstName: string
  lastName: string
  accountType: AccountType
  companyId: string | null
  outletId: string | null
  roles: string[]
  permissions: string[]
}

export const meApi = {
  get: () => api.get<MeResult>("/api/me"),
}
