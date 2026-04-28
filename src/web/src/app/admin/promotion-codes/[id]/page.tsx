"use client"

import { use, useEffect, useState } from "react"
import { useRouter } from "next/navigation"
import { toast } from "sonner"

import { Field } from "@/components/field"
import { ApiError, api } from "@/lib/api"

interface PromotionCodeDetail {
  id: string
  code: string
  discountPercentage: number
  validFrom: string | null
  validUntil: string | null
  maxUses: number | null
  usesCount: number
}

const toDateInput = (iso: string | null) => iso ? iso.slice(0, 10) : ""

export default function EditPromotionCodePage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params)
  const router = useRouter()
  const [code, setCode] = useState("")
  const [discountPercentage, setDiscountPercentage] = useState("0")
  const [validFrom, setValidFrom] = useState("")
  const [validUntil, setValidUntil] = useState("")
  const [maxUses, setMaxUses] = useState("")
  const [usesCount, setUsesCount] = useState(0)
  const [loading, setLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)
  const [errors, setErrors] = useState<Record<string, string>>({})

  useEffect(() => {
    api.get<PromotionCodeDetail>(`/api/promotion-codes/${id}`)
      .then((r) => {
        setCode(r.code)
        setDiscountPercentage(String(r.discountPercentage))
        setValidFrom(toDateInput(r.validFrom))
        setValidUntil(toDateInput(r.validUntil))
        setMaxUses(r.maxUses != null ? String(r.maxUses) : "")
        setUsesCount(r.usesCount)
      })
      .catch((err) => { if (err instanceof ApiError) toast.error(err.message) })
      .finally(() => setLoading(false))
  }, [id])

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setSubmitting(true)
    setErrors({})
    try {
      await api.put(`/api/promotion-codes/${id}`, {
        code,
        discountPercentage: Number(discountPercentage),
        validFrom: validFrom ? new Date(validFrom).toISOString() : null,
        validUntil: validUntil ? new Date(validUntil).toISOString() : null,
        maxUses: maxUses ? Number(maxUses) : null,
      })
      toast.success("Promotion code updated")
      router.push("/admin/promotion-codes")
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

  if (loading) return <p className="text-sm text-muted-foreground">Loading…</p>

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Edit promotion code</h1>
        <p className="text-muted-foreground">Used {usesCount} time{usesCount === 1 ? "" : "s"} so far.</p>
      </div>
      <form onSubmit={handleSubmit} className="max-w-xl space-y-4">
        <Field label="Code" error={errors.code}>
          <input value={code} onChange={(e) => setCode(e.target.value)} required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <Field label="Discount (%)" error={errors.discountpercentage}>
          <input type="number" step="0.01" min="0" max="100" value={discountPercentage} onChange={(e) => setDiscountPercentage(e.target.value)} required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <div className="grid grid-cols-2 gap-3">
          <Field label="Valid from" hint="Optional." error={errors.validfrom}>
            <input type="date" value={validFrom} onChange={(e) => setValidFrom(e.target.value)} className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
          </Field>
          <Field label="Valid until" hint="Optional." error={errors.validuntil}>
            <input type="date" value={validUntil} onChange={(e) => setValidUntil(e.target.value)} className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
          </Field>
        </div>
        <Field label="Max uses" hint="Empty = unlimited." error={errors.maxuses}>
          <input type="number" min="1" step="1" value={maxUses} onChange={(e) => setMaxUses(e.target.value)} className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <div className="flex gap-2">
          <button type="submit" disabled={submitting} className="h-9 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90 disabled:opacity-50">{submitting ? "Saving…" : "Save changes"}</button>
          <button type="button" onClick={() => router.back()} className="h-9 rounded-md px-3 text-sm hover:bg-accent">Cancel</button>
        </div>
      </form>
    </div>
  )
}
