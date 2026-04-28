"use client"

import { use, useEffect, useState } from "react"
import { useRouter } from "next/navigation"
import { toast } from "sonner"

import { Field } from "@/components/field"
import { ApiError, api, type PagedResult } from "@/lib/api"

interface ModifierDetail { id: string; name: string; priceDelta: number; modifierGroupId: string }
interface ModifierGroupRow { id: string; name: string }

export default function EditModifierPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params)
  const router = useRouter()
  const [name, setName] = useState("")
  const [priceDelta, setPriceDelta] = useState("0")
  const [groupName, setGroupName] = useState<string>("")
  const [loading, setLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)
  const [errors, setErrors] = useState<Record<string, string>>({})

  useEffect(() => {
    async function load() {
      try {
        const m = await api.get<ModifierDetail>(`/api/modifiers/${id}`)
        setName(m.name)
        setPriceDelta(String(m.priceDelta))
        const groups = await api.get<PagedResult<ModifierGroupRow>>(`/api/modifier-groups?page=1&pageSize=100`)
        setGroupName(groups.items.find((g) => g.id === m.modifierGroupId)?.name ?? "—")
      } catch (err) {
        if (err instanceof ApiError) toast.error(err.message)
      } finally { setLoading(false) }
    }
    load()
  }, [id])

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setSubmitting(true)
    setErrors({})
    try {
      await api.put(`/api/modifiers/${id}`, { name, priceDelta: Number(priceDelta) })
      toast.success("Modifier updated")
      router.push("/admin/modifiers")
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
        <h1 className="text-2xl font-semibold tracking-tight">Edit modifier</h1>
        <p className="text-muted-foreground">Group is fixed after creation.</p>
      </div>
      <form onSubmit={handleSubmit} className="max-w-xl space-y-4">
        <div className="space-y-1">
          <p className="text-sm font-medium">Modifier group</p>
          <p className="text-sm text-muted-foreground">{groupName}</p>
        </div>
        <Field label="Name" error={errors.name}>
          <input value={name} onChange={(e) => setName(e.target.value)} required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <Field label="Price delta" hint="Added to the product price (can be negative)." error={errors.pricedelta}>
          <input type="number" step="0.01" value={priceDelta} onChange={(e) => setPriceDelta(e.target.value)} required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <div className="flex gap-2">
          <button type="submit" disabled={submitting} className="h-9 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90 disabled:opacity-50">{submitting ? "Saving…" : "Save changes"}</button>
          <button type="button" onClick={() => router.back()} className="h-9 rounded-md px-3 text-sm hover:bg-accent">Cancel</button>
        </div>
      </form>
    </div>
  )
}
