"use client"

import { useQuery } from "@tanstack/react-query"

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Skeleton } from "@/components/ui/skeleton"
import { CompanyForm } from "@/features/companies/company-form"
import { OutletForm } from "@/features/outlets/outlet-form"
import { companiesApi } from "@/features/companies/api"
import { outletsApi } from "@/features/outlets/api"
import { meApi } from "@/features/me/api"

export default function SettingsPage() {
  const meQuery = useQuery({
    queryKey: ["me"],
    queryFn: () => meApi.get(),
  })

  const companyQuery = useQuery({
    queryKey: ["company", meQuery.data?.companyId],
    queryFn: () => companiesApi.get(meQuery.data!.companyId!),
    enabled: !!meQuery.data?.companyId,
  })

  const outletQuery = useQuery({
    queryKey: ["outlet", meQuery.data?.outletId],
    queryFn: () => outletsApi.get(meQuery.data!.outletId!),
    enabled: !!meQuery.data?.outletId,
  })

  if (meQuery.isLoading) return <Skeleton className="h-64 w-full max-w-3xl" />
  if (!meQuery.data?.companyId)
    return <p className="text-sm text-muted-foreground">No company is associated with your account.</p>

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Settings</h1>
        <p className="text-muted-foreground">Company and outlet details — visible on receipts and listings.</p>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Company</CardTitle>
            <CardDescription>Legal and billing information.</CardDescription>
          </CardHeader>
          <CardContent>
            {companyQuery.isLoading && <Skeleton className="h-48 w-full" />}
            {companyQuery.data && <CompanyForm company={companyQuery.data} />}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="text-base">Outlet</CardTitle>
            <CardDescription>Customer-facing café details.</CardDescription>
          </CardHeader>
          <CardContent>
            {outletQuery.isLoading && <Skeleton className="h-48 w-full" />}
            {outletQuery.data && <OutletForm outlet={outletQuery.data} />}
          </CardContent>
        </Card>
      </div>
    </div>
  )
}
