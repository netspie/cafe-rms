"use client"

// Products — edit page, single file, six inline sections.
//
// 1. Basics: rename, edit description / barcode / tax rate.
// 2. Tags: attach/detach via badge list + select.
// 3. Allergens: attach/detach.
// 4. Modifier groups: attach/detach.
// 5. Images: list + add URL + remove.
// 6. Prices per price group: per-row Net upsert.
//
// Each section is self-contained: declares its own useState, calls api.* with
// the relevant URL, and asks the parent to refetch on change. No abstractions
// shared across sections — copy-paste is fine for thesis defense purposes.

import { use, useEffect, useState } from "react"
import { useRouter } from "next/navigation"
import { Trash2, X } from "lucide-react"
import { toast } from "sonner"

import { Field } from "@/components/field"
import { ApiError, api, type PagedResult } from "@/lib/api"

interface ProductImage { id: string; url: string }
interface ProductPrice { priceGroupId: string; net: number }

interface ProductDetail {
  id: string
  name: string
  description: string | null
  barcode: string | null
  taxRateId: string
  tagIds: string[]
  allergenIds: string[]
  modifierGroupIds: string[]
  images: ProductImage[]
  prices: ProductPrice[]
}

interface NamedRow { id: string; name: string }

export default function EditProductPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params)
  const router = useRouter()

  const [product, setProduct] = useState<ProductDetail | null>(null)
  const [taxRates, setTaxRates] = useState<NamedRow[]>([])
  const [tags, setTags] = useState<NamedRow[]>([])
  const [allergens, setAllergens] = useState<NamedRow[]>([])
  const [modifierGroups, setModifierGroups] = useState<NamedRow[]>([])
  const [priceGroups, setPriceGroups] = useState<NamedRow[]>([])
  const [loading, setLoading] = useState(true)
  const [refreshKey, setRefreshKey] = useState(0)

  useEffect(() => {
    async function load() {
      try {
        const [p, taxes, tg, al, mg, pg] = await Promise.all([
          api.get<ProductDetail>(`/api/products/${id}`),
          api.get<PagedResult<NamedRow>>(`/api/tax-rates?page=1&pageSize=100&sort=name`),
          api.get<PagedResult<NamedRow>>(`/api/tags?page=1&pageSize=200&sort=name`),
          api.get<PagedResult<NamedRow>>(`/api/allergens?page=1&pageSize=200&sort=name`),
          api.get<PagedResult<NamedRow>>(`/api/modifier-groups?page=1&pageSize=200&sort=name`),
          api.get<PagedResult<NamedRow>>(`/api/price-groups?page=1&pageSize=100&sort=name`),
        ])
        setProduct(p)
        setTaxRates(taxes.items)
        setTags(tg.items)
        setAllergens(al.items)
        setModifierGroups(mg.items)
        setPriceGroups(pg.items)
      } catch (err) {
        if (err instanceof ApiError) toast.error(err.message)
      } finally { setLoading(false) }
    }
    load()
  }, [id, refreshKey])

  if (loading || !product) return <p className="text-sm text-muted-foreground">Loading…</p>

  const refresh = () => setRefreshKey((k) => k + 1)

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">{product.name}</h1>
        <p className="text-muted-foreground">Edit details, link tags / allergens / modifier groups / images / prices.</p>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <div className="lg:col-span-2">
          <BasicsSection product={product} taxRates={taxRates} onSaved={refresh} />
        </div>

        <AttachSection
          title="Tags"
          attached={tags.filter((t) => product.tagIds.includes(t.id))}
          available={tags.filter((t) => !product.tagIds.includes(t.id))}
          attach={(tagId) => api.post(`/api/products/${product.id}/tags/${tagId}`)}
          detach={(tagId) => api.delete(`/api/products/${product.id}/tags/${tagId}`)}
          onChanged={refresh}
        />

        <AttachSection
          title="Allergens"
          attached={allergens.filter((a) => product.allergenIds.includes(a.id))}
          available={allergens.filter((a) => !product.allergenIds.includes(a.id))}
          attach={(allergenId) => api.post(`/api/products/${product.id}/allergens/${allergenId}`)}
          detach={(allergenId) => api.delete(`/api/products/${product.id}/allergens/${allergenId}`)}
          onChanged={refresh}
        />

        <AttachSection
          title="Modifier groups"
          attached={modifierGroups.filter((g) => product.modifierGroupIds.includes(g.id))}
          available={modifierGroups.filter((g) => !product.modifierGroupIds.includes(g.id))}
          attach={(groupId) => api.post(`/api/products/${product.id}/modifier-groups/${groupId}`)}
          detach={(groupId) => api.delete(`/api/products/${product.id}/modifier-groups/${groupId}`)}
          onChanged={refresh}
        />

        <ImagesSection productId={product.id} images={product.images} onChanged={refresh} />

        <div className="lg:col-span-2">
          <PricesSection productId={product.id} prices={product.prices} priceGroups={priceGroups} onChanged={refresh} />
        </div>
      </div>

      <button type="button" onClick={() => router.push("/admin/products")} className="h-9 rounded-md px-3 text-sm hover:bg-accent">
        Back to list
      </button>
    </div>
  )
}

function BasicsSection({ product, taxRates, onSaved }: { product: ProductDetail; taxRates: NamedRow[]; onSaved: () => void }) {
  const [name, setName] = useState(product.name)
  const [description, setDescription] = useState(product.description ?? "")
  const [barcode, setBarcode] = useState(product.barcode ?? "")
  const [taxRateId, setTaxRateId] = useState(product.taxRateId)
  const [submitting, setSubmitting] = useState(false)
  const [errors, setErrors] = useState<Record<string, string>>({})

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setSubmitting(true)
    setErrors({})
    try {
      await api.put(`/api/products/${product.id}`, {
        name,
        description: description || null,
        barcode: barcode || null,
        taxRateId,
      })
      toast.success("Product updated")
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
      <form onSubmit={handleSubmit} className="max-w-xl space-y-4">
        <Field label="Name" error={errors.name}>
          <input value={name} onChange={(e) => setName(e.target.value)} required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <Field label="Description" error={errors.description}>
          <input value={description} onChange={(e) => setDescription(e.target.value)} className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <Field label="Barcode" hint="Optional, must be unique." error={errors.barcode}>
          <input value={barcode} onChange={(e) => setBarcode(e.target.value)} className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <Field label="Tax rate" error={errors.taxrateid}>
          <select value={taxRateId} onChange={(e) => setTaxRateId(e.target.value)} required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30">
            {taxRates.map((t) => <option key={t.id} value={t.id}>{t.name}</option>)}
          </select>
        </Field>
        <button type="submit" disabled={submitting} className="h-9 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90 disabled:opacity-50">
          {submitting ? "Saving…" : "Save changes"}
        </button>
      </form>
    </section>
  )
}

function AttachSection({ title, attached, available, attach, detach, onChanged }: { title: string; attached: NamedRow[]; available: NamedRow[]; attach: (id: string) => Promise<unknown>; detach: (id: string) => Promise<unknown>; onChanged: () => void }) {
  const [selectedId, setSelectedId] = useState("")

  async function add() {
    if (!selectedId) return
    try {
      await attach(selectedId)
      toast.success(`${title.replace(/s$/, "")} added`)
      setSelectedId("")
      onChanged()
    } catch (err) {
      if (err instanceof ApiError) toast.error(err.message)
    }
  }

  async function remove(itemId: string) {
    try {
      await detach(itemId)
      toast.success(`${title.replace(/s$/, "")} removed`)
      onChanged()
    } catch (err) {
      if (err instanceof ApiError) toast.error(err.message)
    }
  }

  return (
    <section className="rounded-lg border bg-card p-4">
      <h2 className="mb-4 text-base font-semibold">{title}</h2>
      <div className="space-y-3">
        <div className="flex flex-wrap gap-2">
          {attached.length === 0 && <p className="text-sm text-muted-foreground">None attached.</p>}
          {attached.map((it) => (
            <span key={it.id} className="inline-flex items-center gap-1.5 rounded-full bg-secondary px-2.5 py-0.5 text-xs">
              {it.name}
              <button type="button" onClick={() => remove(it.id)} className="hover:text-destructive" aria-label={`Remove ${it.name}`}>
                <X className="h-3 w-3" />
              </button>
            </span>
          ))}
        </div>
        <div className="flex gap-2">
          <select value={selectedId} onChange={(e) => setSelectedId(e.target.value)} className="h-9 max-w-xs flex-1 rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30">
            <option value="">Add…</option>
            {available.map((it) => <option key={it.id} value={it.id}>{it.name}</option>)}
          </select>
          <button type="button" onClick={add} disabled={!selectedId} className="h-9 rounded-md border px-3 text-sm hover:bg-accent disabled:opacity-50">Add</button>
        </div>
      </div>
    </section>
  )
}

function ImagesSection({ productId, images, onChanged }: { productId: string; images: ProductImage[]; onChanged: () => void }) {
  const [url, setUrl] = useState("")
  const [submitting, setSubmitting] = useState(false)

  async function add() {
    if (!url) return
    setSubmitting(true)
    try {
      await api.post(`/api/products/${productId}/images`, { url })
      toast.success("Image added")
      setUrl("")
      onChanged()
    } catch (err) {
      if (err instanceof ApiError) toast.error(err.message)
    } finally { setSubmitting(false) }
  }

  async function remove(imageId: string) {
    try {
      await api.delete(`/api/products/${productId}/images/${imageId}`)
      toast.success("Image removed")
      onChanged()
    } catch (err) {
      if (err instanceof ApiError) toast.error(err.message)
    }
  }

  return (
    <section className="rounded-lg border bg-card p-4">
      <h2 className="mb-4 text-base font-semibold">Images</h2>
      <div className="space-y-3">
        <div className="space-y-2">
          {images.length === 0 && <p className="text-sm text-muted-foreground">No images yet.</p>}
          {images.map((img) => (
            <div key={img.id} className="flex items-center gap-3 rounded-md border px-3 py-2">
              <span className="flex-1 truncate font-mono text-xs text-muted-foreground">{img.url}</span>
              <button type="button" onClick={() => remove(img.id)} className="inline-flex h-8 w-8 items-center justify-center rounded-md text-destructive hover:bg-destructive/10" aria-label="Remove image">
                <Trash2 className="h-4 w-4" />
              </button>
            </div>
          ))}
        </div>
        <div className="flex gap-2">
          <input value={url} onChange={(e) => setUrl(e.target.value)} placeholder="https://…" className="h-9 max-w-md flex-1 rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
          <button type="button" onClick={add} disabled={!url || submitting} className="h-9 rounded-md border px-3 text-sm hover:bg-accent disabled:opacity-50">Add image</button>
        </div>
      </div>
    </section>
  )
}

function PricesSection({ productId, prices, priceGroups, onChanged }: { productId: string; prices: ProductPrice[]; priceGroups: NamedRow[]; onChanged: () => void }) {
  const [drafts, setDrafts] = useState<Record<string, string>>({})
  const [savingId, setSavingId] = useState<string | null>(null)

  const priceFor = (groupId: string) => prices.find((p) => p.priceGroupId === groupId)?.net.toString() ?? ""
  const valueFor = (groupId: string) => drafts[groupId] !== undefined ? drafts[groupId] : priceFor(groupId)

  async function save(priceGroupId: string) {
    const raw = valueFor(priceGroupId)
    const net = Number(raw)
    if (!Number.isFinite(net) || net < 0) {
      toast.error("Enter a non-negative number")
      return
    }
    setSavingId(priceGroupId)
    try {
      await api.put(`/api/products/${productId}/prices/${priceGroupId}`, { net })
      toast.success("Price saved")
      onChanged()
    } catch (err) {
      if (err instanceof ApiError) toast.error(err.message)
    } finally { setSavingId(null) }
  }

  return (
    <section className="rounded-lg border bg-card p-4">
      <h2 className="mb-4 text-base font-semibold">Prices per price group</h2>
      <div className="space-y-3">
        {priceGroups.length === 0 && <p className="text-sm text-muted-foreground">No price groups defined yet.</p>}
        {priceGroups.map((g) => (
          <div key={g.id} className="flex items-center gap-3">
            <span className="w-40 text-sm font-medium">{g.name}</span>
            <input
              type="number"
              step="0.01"
              min="0"
              placeholder="0.00"
              value={valueFor(g.id)}
              onChange={(e) => setDrafts((d) => ({ ...d, [g.id]: e.target.value }))}
              className="h-9 w-32 rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30"
            />
            <button type="button" onClick={() => save(g.id)} disabled={savingId === g.id} className="h-9 rounded-md border px-3 text-sm hover:bg-accent disabled:opacity-50">
              {savingId === g.id ? "Saving…" : "Save"}
            </button>
          </div>
        ))}
      </div>
    </section>
  )
}
