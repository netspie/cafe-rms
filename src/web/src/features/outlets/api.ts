import { api } from "@/lib/api"

export type Currency = "PLN" | "EUR" | "USD" | "GBP" | "CZK"

export const ALL_CURRENCIES: Currency[] = ["PLN", "EUR", "USD", "GBP", "CZK"]

export interface OutletDetail {
  id: string
  displayName: string
  streetAddress: string
  phone: string
  timeZone: string
  currency: string
  logoUrl: string | null
  createdAt: string
  updatedAt: string | null
}

export interface OutletInput {
  displayName: string
  streetAddress: string
  phone: string
  timeZone: string
  currency: Currency
  logoUrl: string | null
}

export const outletsApi = {
  get: (id: string) => api.get<OutletDetail>(`/api/outlets/${id}`),
  update: (id: string, body: OutletInput) => api.put<void>(`/api/outlets/${id}`, body),
}
