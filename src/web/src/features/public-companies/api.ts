import { api } from "@/lib/api"

export interface PublicCompanyItem {
  id: string
  legalName: string
  displayName: string
  streetAddress: string
  phone: string
  timeZone: string
  currency: string
  logoUrl: string | null
}

export const publicCompaniesApi = {
  list: () => api.get<PublicCompanyItem[]>("/api/companies/public"),
}
