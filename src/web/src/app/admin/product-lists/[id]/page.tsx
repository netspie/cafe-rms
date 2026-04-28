"use client"

// Product list — edit page. Two sections inlined: basics (rename) and the
// items picker (attach/detach products from this list).

import { use, useEffect, useState } from "react"
import { useRouter } from "next/navigation"
import { X } from "lucide-react"
import { toast } from "sonner"

import { Field } from "@/components/field"
import { ApiError, api, type PagedResult } from "@/lib/api"

interface ProductListDetail { id: string; name: string; productIds: string[] }
interface ProductRow { id: string; name: string }

export default function EditProductListPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params)
  const router = useRouter()

  const [name, setName] = useState("")
  const [productIds, setProductIds] = useState<string[]>([])
  const [products, setProducts] = useState<ProductRow[]>([])
  const [loading, setLoading] = useState(true)
  const [refreshKey, setRefreshKey] = useState(0)

  // Load the list + the products lookup in one effect.
  useEffect(() => {
    async function load() {
      try {
        const [list, prods] = await Promise.all([
          api.get<ProductListDetail>(`/api/product-lists/${id}`),
          api.get<PagedResult<ProductRow>>(`/api/products?page=1&pageSize=200&sort=name`),
        ])
        setName(list.name)
        setProductIds(list.productIds)
        setProducts(prods.items)
      } catch (err) {
        if (err instanceof ApiError) toast.error(err.message)
      } finally { setLoading(false) }
    }
    load()
  }, [id, refreshKey])

  if (loading) return <p className="text-sm text-muted-foreground">Loading…</p>

  const attached = products.filter((p) => productIds.includes(p.id))
  const available = products.filter((p) => !productIds.includes(p.id))

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">{name}</h1>
        <p className="text-muted-foreground">Rename or pick which products belong to this list.</p>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <BasicsCard listId={id} initialName={name} onSaved={() => setRefreshKey((k) => k + 1)} />
        <ItemsCard listId={id} attached={attached} available={available} onChanged={() => setRefreshKey((k) => k + 1)} />
      </div>

      <button type="button" onClick={() => router.push("/admin/product-lists")} className="h-9 rounded-md px-3 text-sm hover:bg-accent">
        Back to list
      </button>
    </div>
  )
}

function BasicsCard({ listId, initialName, onSaved }: { listId: string; initialName: string; onSaved: () => void }) {
  const [name, setName] = useState(initialName)
  const [submitting, setSubmitting] = useState(false)
  const [errors, setErrors] = useState<Record<string, string>>({})

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setSubmitting(true)
    setErrors({})
    try {
      await api.put(`/api/product-lists/${listId}`, { name })
      toast.success("Product list updated")
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
      <h2 className="mb-4 text-base font-semibold">Basics</h2>
      <form onSubmit={handleSubmit} className="space-y-4">
        <Field label="Name" error={errors.name}>
          <input value={name} onChange={(e) => setName(e.target.value)} required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <button type="submit" disabled={submitting} className="h-9 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90 disabled:opacity-50">
          {submitting ? "Saving…" : "Save changes"}
        </button>
      </form>
    </section>
  )
}

function ItemsCard({ listId, attached, available, onChanged }: { listId: string; attached: ProductRow[]; available: ProductRow[]; onChanged: () => void }) {
  const [selectedId, setSelectedId] = useState("")

  async function add() {
    if (!selectedId) return
    try {
      await api.post(`/api/product-lists/${listId}/items`, { productId: selectedId })
      toast.success("Product added")
      setSelectedId("")
      onChanged()
    } catch (err) {
      if (err instanceof ApiError) toast.error(err.message)
    }
  }

  async function remove(productId: string) {
    try {
      await api.delete(`/api/product-lists/${listId}/items/${productId}`)
      toast.success("Product removed")
      onChanged()
    } catch (err) {
      if (err instanceof ApiError) toast.error(err.message)
    }
  }

  return (
    <section className="rounded-lg border bg-card p-4">
      <h2 className="mb-4 text-base font-semibold">Products in this list</h2>
      <div className="space-y-3">
        <div className="flex flex-wrap gap-2">
          {attached.length === 0 && <p className="text-sm text-muted-foreground">No products yet.</p>}
          {attached.map((p) => (
            <span key={p.id} className="inline-flex items-center gap-1.5 rounded-full bg-secondary px-2.5 py-0.5 text-xs">
              {p.name}
              <button type="button" onClick={() => remove(p.id)} className="hover:text-destructive" aria-label={`Remove ${p.name}`}>
                <X className="h-3 w-3" />
              </button>
            </span>
          ))}
        </div>
        <div className="flex gap-2">
          <select value={selectedId} onChange={(e) => setSelectedId(e.target.value)} className="h-9 max-w-xs flex-1 rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30">
            <option value="">Add product…</option>
            {available.map((p) => <option key={p.id} value={p.id}>{p.name}</option>)}
          </select>
          <button type="button" onClick={add} disabled={!selectedId} className="h-9 rounded-md border px-3 text-sm hover:bg-accent disabled:opacity-50">Add</button>
        </div>
      </div>
    </section>
  )
}
