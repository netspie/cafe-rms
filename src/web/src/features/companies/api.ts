import { api } from "@/lib/api"

export interface CompanyOutletInfo {
  id: string
  displayName: string
  streetAddress: string
  phone: string
  timeZone: string
  currency: string
  logoUrl: string | null
}

export interface CompanyDetail {
  id: string
  legalName: string
  taxId: string
  invoicingAddress: string
  billingEmail: string
  billingPhone: string
  isPublic: boolean
  createdAt: string
  outlet: CompanyOutletInfo
}

export interface CompanyInput {
  legalName: string
  taxId: string
  invoicingAddress: string
  billingEmail: string
  billingPhone: string
  isPublic: boolean
}

export const companiesApi = {
  get: (id: string) => api.get<CompanyDetail>(`/api/companies/${id}`),
  update: (id: string, body: CompanyInput) => api.put<void>(`/api/companies/${id}`, body),
}
