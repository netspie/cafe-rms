"use client"

// Settings — single file. Resolves the current user's company + outlet via
// /api/me, then renders two inline sections (Company and Outlet) for editing.

import { useEffect, useState } from "react"
import { toast } from "sonner"

import { Field } from "@/components/field"
import { ApiError, api } from "@/lib/api"

interface MeResult { companyId: string | null; outletId: string | null }

interface CompanyDetail {
  id: string
  legalName: string
  taxId: string
  invoicingAddress: string
  billingEmail: string
  billingPhone: string
  isPublic: boolean
}

interface OutletDetail {
  id: string
  displayName: string
  streetAddress: string
  phone: string
  timeZone: string
  currency: string
  logoUrl: string | null
}

const CURRENCIES = ["PLN", "EUR", "USD", "GBP", "CZK"]

export default function SettingsPage() {
  const [me, setMe] = useState<MeResult | null>(null)
  const [company, setCompany] = useState<CompanyDetail | null>(null)
  const [outlet, setOutlet] = useState<OutletDetail | null>(null)
  const [loading, setLoading] = useState(true)
  const [refreshKey, setRefreshKey] = useState(0)

  useEffect(() => {
    async function load() {
      try {
        const m = await api.get<MeResult>("/api/me")
        setMe(m)
        if (m.companyId) {
          const [c, o] = await Promise.all([
            api.get<CompanyDetail>(`/api/companies/${m.companyId}`),
            m.outletId ? api.get<OutletDetail>(`/api/outlets/${m.outletId}`) : Promise.resolve(null),
          ])
          setCompany(c)
          setOutlet(o)
        }
      } catch (err) {
        if (err instanceof ApiError) toast.error(err.message)
      } finally { setLoading(false) }
    }
    load()
  }, [refreshKey])

  if (loading) return <p className="text-sm text-muted-foreground">Loading…</p>
  if (!me?.companyId) return <p className="text-sm text-muted-foreground">No company is associated with your account.</p>

  const refresh = () => setRefreshKey((k) => k + 1)

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Settings</h1>
        <p className="text-muted-foreground">Company and outlet details — visible on receipts and listings.</p>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        {company && <CompanySection company={company} onSaved={refresh} />}
        {outlet && <OutletSection outlet={outlet} onSaved={refresh} />}
      </div>
    </div>
  )
}

function CompanySection({ company, onSaved }: { company: CompanyDetail; onSaved: () => void }) {
  const [legalName, setLegalName] = useState(company.legalName)
  const [taxId, setTaxId] = useState(company.taxId)
  const [invoicingAddress, setInvoicingAddress] = useState(company.invoicingAddress)
  const [billingEmail, setBillingEmail] = useState(company.billingEmail)
  const [billingPhone, setBillingPhone] = useState(company.billingPhone)
  const [isPublic, setIsPublic] = useState(company.isPublic)
  const [submitting, setSubmitting] = useState(false)
  const [errors, setErrors] = useState<Record<string, string>>({})

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setSubmitting(true)
    setErrors({})
    try {
      await api.put(`/api/companies/${company.id}`, {
        legalName, taxId, invoicingAddress, billingEmail, billingPhone, isPublic,
      })
      toast.success("Company updated")
      onSaved()
    } catch (err) {
      if (err instanceof ApiError) {
        if (err.problem.errors) {
          const flat: Record<string, string> = {}
          for (const [k, v] of Object.entries(err.problem.errors)) flat[k.toLowerCase()] = v[0]
          setErrors(flat)
        } else toast.error(err.message)
      }
    } finally { setSubmitting(false) }
  }

  return (
    <section className="rounded-lg border bg-card p-4">
      <h2 className="text-base font-semibold">Company</h2>
      <p className="mb-4 text-xs text-muted-foreground">Legal and billing information.</p>
      <form onSubmit={handleSubmit} className="space-y-4">
        <Field label="Legal name" error={errors.legalname}>
          <input value={legalName} onChange={(e) => setLegalName(e.target.value)} required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <div className="grid grid-cols-2 gap-3">
          <Field label="Tax ID" error={errors.taxid}>
            <input value={taxId} onChange={(e) => setTaxId(e.target.value)} required placeholder="NIP" className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
          </Field>
          <Field label="Billing phone" error={errors.billingphone}>
            <input value={billingPhone} onChange={(e) => setBillingPhone(e.target.value)} required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
          </Field>
        </div>
        <Field label="Billing email" error={errors.billingemail}>
          <input type="email" value={billingEmail} onChange={(e) => setBillingEmail(e.target.value)} required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <Field label="Invoicing address" error={errors.invoicingaddress}>
          <input value={invoicingAddress} onChange={(e) => setInvoicingAddress(e.target.value)} required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <label className="flex cursor-pointer items-center gap-2 text-sm">
          <input type="checkbox" checked={isPublic} onChange={(e) => setIsPublic(e.target.checked)} className="h-4 w-4" />
          <span>Listed publicly in the CafeRMS public directory</span>
        </label>
        <button type="submit" disabled={submitting} className="h-9 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90 disabled:opacity-50">
          {submitting ? "Saving…" : "Save company"}
        </button>
      </form>
    </section>
  )
}

function OutletSection({ outlet, onSaved }: { outlet: OutletDetail; onSaved: () => void }) {
  const [displayName, setDisplayName] = useState(outlet.displayName)
  const [streetAddress, setStreetAddress] = useState(outlet.streetAddress)
  const [phone, setPhone] = useState(outlet.phone)
  const [timeZone, setTimeZone] = useState(outlet.timeZone)
  const [currency, setCurrency] = useState(outlet.currency)
  const [logoUrl, setLogoUrl] = useState(outlet.logoUrl ?? "")
  const [submitting, setSubmitting] = useState(false)
  const [errors, setErrors] = useState<Record<string, string>>({})

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setSubmitting(true)
    setErrors({})
    try {
      await api.put(`/api/outlets/${outlet.id}`, {
        displayName, streetAddress, phone, timeZone, currency, logoUrl: logoUrl || null,
      })
      toast.success("Outlet updated")
      onSaved()
    } catch (err) {
      if (err instanceof ApiError) {
        if (err.problem.errors) {
          const flat: Record<string, string> = {}
          for (const [k, v] of Object.entries(err.problem.errors)) flat[k.toLowerCase()] = v[0]
          setErrors(flat)
        } else toast.error(err.message)
      }
    } finally { setSubmitting(false) }
  }

  return (
    <section className="rounded-lg border bg-card p-4">
      <h2 className="text-base font-semibold">Outlet</h2>
      <p className="mb-4 text-xs text-muted-foreground">Customer-facing café details.</p>
      <form onSubmit={handleSubmit} className="space-y-4">
        <Field label="Display name" error={errors.displayname}>
          <input value={displayName} onChange={(e) => setDisplayName(e.target.value)} required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <Field label="Street address" error={errors.streetaddress}>
          <input value={streetAddress} onChange={(e) => setStreetAddress(e.target.value)} required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <div className="grid grid-cols-2 gap-3">
          <Field label="Phone" error={errors.phone}>
            <input value={phone} onChange={(e) => setPhone(e.target.value)} required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
          </Field>
          <Field label="Time zone" error={errors.timezone}>
            <input value={timeZone} onChange={(e) => setTimeZone(e.target.value)} required placeholder="Europe/Warsaw" className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
          </Field>
        </div>
        <Field label="Currency" error={errors.currency}>
          <select value={currency} onChange={(e) => setCurrency(e.target.value)} required className="h-9 max-w-xs rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30">
            {CURRENCIES.map((c) => <option key={c} value={c}>{c}</option>)}
          </select>
        </Field>
        <Field label="Logo URL" hint="Shown on receipts and the public listing." error={errors.logourl}>
          <input value={logoUrl} onChange={(e) => setLogoUrl(e.target.value)} placeholder="https://… (optional)" className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <button type="submit" disabled={submitting} className="h-9 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90 disabled:opacity-50">
          {submitting ? "Saving…" : "Save outlet"}
        </button>
      </form>
    </section>
  )
}
