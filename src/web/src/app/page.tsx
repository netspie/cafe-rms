"use client"

// Public homepage — the café directory. Lists every company that opted in
// via Settings → Listed publicly. Anonymous endpoint, no auth needed.

import Link from "next/link"
import { useEffect, useState } from "react"

import { ShibaMark } from "@/components/shiba-mark"
import { api } from "@/lib/api"

interface PublicCompany {
  id: string
  legalName: string
  displayName: string
  streetAddress: string
  phone: string
  timeZone: string
  currency: string
  logoUrl: string | null
}

export default function PublicHomePage() {
  const [companies, setCompanies] = useState<PublicCompany[] | null>(null)

  useEffect(() => {
    api.get<PublicCompany[]>("/api/companies/public")
      .then(setCompanies)
      .catch(() => setCompanies([]))
  }, [])

  return (
    <div className="min-h-dvh bg-background">
      <header className="sticky top-0 z-10 flex items-center gap-3 border-b bg-background px-6 py-3">
        <ShibaMark className="h-7 w-7" />
        <div>
          <p className="font-semibold tracking-tight">CafeRMS</p>
          <p className="text-[10px] uppercase tracking-[0.18em] text-muted-foreground">いらっしゃい</p>
        </div>
        <div className="flex-1" />
        <Link href="/login" className="inline-flex h-9 items-center rounded-md border px-3 text-sm hover:bg-accent">
          Staff sign-in
        </Link>
      </header>

      <main className="mx-auto max-w-5xl space-y-8 px-6 py-12">
        <section className="space-y-3 text-center">
          <h1 className="text-4xl font-semibold tracking-tight">Find a café near you</h1>
          <p className="mx-auto max-w-xl text-muted-foreground">
            Cafés using CafeRMS to handle their menu, events, and loyalty.
          </p>
        </section>

        {companies === null && (
          <p className="text-center text-sm text-muted-foreground">Loading…</p>
        )}

        {companies?.length === 0 && (
          <div className="flex flex-col items-center gap-3 py-16 text-muted-foreground">
            <ShibaMark className="h-14 w-14 opacity-60" />
            <p className="text-sm">No public cafés listed yet.</p>
          </div>
        )}

        {companies && companies.length > 0 && (
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {companies.map((c) => (
              <article key={c.id} className="rounded-lg border bg-card p-4">
                <h3 className="text-base font-semibold">{c.displayName}</h3>
                <p className="text-xs text-muted-foreground">{c.legalName}</p>
                <div className="mt-3 space-y-1 text-sm text-muted-foreground">
                  <p>{c.streetAddress}</p>
                  <p>{c.phone}</p>
                  <p className="text-xs">{c.timeZone} · {c.currency}</p>
                </div>
              </article>
            ))}
          </div>
        )}
      </main>
    </div>
  )
}
