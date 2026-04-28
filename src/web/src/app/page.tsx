"use client"

import Link from "next/link"
import { useQuery } from "@tanstack/react-query"

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Skeleton } from "@/components/ui/skeleton"
import { ShibaMark } from "@/components/shiba-mark"
import { publicCompaniesApi } from "@/features/public-companies/api"

export default function PublicHomePage() {
  const query = useQuery({
    queryKey: ["public-companies"],
    queryFn: () => publicCompaniesApi.list(),
  })

  return (
    <div className="min-h-dvh bg-background">
      <header className="sticky top-0 z-10 flex items-center gap-3 border-b bg-background px-6 py-3">
        <ShibaMark className="h-7 w-7" />
        <div>
          <p className="font-semibold tracking-tight">CafeRMS</p>
          <p className="text-[10px] uppercase tracking-[0.18em] text-muted-foreground">いらっしゃい</p>
        </div>
        <div className="flex-1" />
        <Button render={<Link href="/login" />} nativeButton={false} variant="outline" size="sm">
          Staff sign-in
        </Button>
      </header>

      <main className="mx-auto max-w-5xl space-y-8 px-6 py-12">
        <section className="space-y-3 text-center">
          <h1 className="text-4xl font-semibold tracking-tight">Find a café near you</h1>
          <p className="mx-auto max-w-xl text-muted-foreground">
            Cafés using CafeRMS to handle their menu, events, and loyalty.
          </p>
        </section>

        {query.isLoading && (
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {[0, 1, 2, 3, 4, 5].map((i) => (
              <Skeleton key={i} className="h-44 w-full" />
            ))}
          </div>
        )}

        {query.data?.length === 0 && (
          <div className="flex flex-col items-center gap-3 py-16 text-muted-foreground">
            <ShibaMark className="h-14 w-14 opacity-60" />
            <p className="text-sm">No public cafés listed yet.</p>
          </div>
        )}

        {query.data && query.data.length > 0 && (
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {query.data.map((c) => (
              <Card key={c.id}>
                <CardHeader>
                  <CardTitle className="text-base">{c.displayName}</CardTitle>
                  <CardDescription>{c.legalName}</CardDescription>
                </CardHeader>
                <CardContent className="space-y-1 text-sm text-muted-foreground">
                  <p>{c.streetAddress}</p>
                  <p>{c.phone}</p>
                  <p className="text-xs">{c.timeZone} · {c.currency}</p>
                </CardContent>
              </Card>
            ))}
          </div>
        )}
      </main>
    </div>
  )
}
