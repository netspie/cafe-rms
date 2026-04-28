"use client"

import Link from "next/link"
import { useEffect, useState } from "react"
import { useRouter } from "next/navigation"
import { Eye, Plus } from "lucide-react"
import { toast } from "sonner"

import { Field } from "@/components/field"
import { ShibaMark } from "@/components/shiba-mark"
import { ApiError, api, type PagedResult } from "@/lib/api"

type EventStatus = "Draft" | "Published" | "Closed" | "Cancelled"

interface EventItem { id: string; name: string; status: EventStatus; createdAt: string }
interface NamedRow { id: string; name: string }

const PAGE_SIZE = 20

function statusClass(status: EventStatus): string {
  if (status === "Published") return "bg-primary/10 text-primary"
  if (status === "Cancelled") return "bg-destructive/10 text-destructive"
  if (status === "Closed") return "bg-secondary text-secondary-foreground"
  return "border bg-transparent"
}

export default function EventsListPage() {
  const [page, setPage] = useState(1)
  const [filter, setFilter] = useState("")
  const [data, setData] = useState<PagedResult<EventItem> | null>(null)
  const [loading, setLoading] = useState(true)
  const [refreshKey, setRefreshKey] = useState(0)
  const [showAdd, setShowAdd] = useState(false)
  const [productLists, setProductLists] = useState<NamedRow[]>([])
  const [priceGroups, setPriceGroups] = useState<NamedRow[]>([])

  useEffect(() => {
    Promise.all([
      api.get<PagedResult<NamedRow>>(`/api/product-lists?page=1&pageSize=100&sort=name`).then((r) => setProductLists(r.items)),
      api.get<PagedResult<NamedRow>>(`/api/price-groups?page=1&pageSize=100&sort=name`).then((r) => setPriceGroups(r.items)),
    ]).catch(() => {})
  }, [])

  useEffect(() => {
    setLoading(true)
    const params = new URLSearchParams({ page: String(page), pageSize: String(PAGE_SIZE), sort: "-createdAt" })
    if (filter) params.set("name", filter)
    api.get<PagedResult<EventItem>>(`/api/events?${params}`)
      .then(setData)
      .catch((err) => { if (err instanceof ApiError) toast.error(err.message) })
      .finally(() => setLoading(false))
  }, [page, filter, refreshKey])

  const totalPages = data ? Math.max(1, Math.ceil(data.total / PAGE_SIZE)) : 1

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Events</h1>
        <p className="text-muted-foreground">Themed evenings, workshops, anything ticketed.</p>
      </div>

      <div className="flex items-center gap-3">
        <input placeholder="Filter by name…" value={filter} onChange={(e) => { setPage(1); setFilter(e.target.value) }} className="h-9 max-w-xs rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        <div className="flex-1" />
        <button onClick={() => setShowAdd(true)} className="inline-flex h-9 items-center gap-2 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90">
          <Plus className="h-4 w-4" />New event
        </button>
      </div>

      <div className="overflow-hidden rounded-md border">
        <table className="w-full text-sm">
          <thead className="bg-muted/50 text-left">
            <tr>
              <th className="px-3 py-2 font-medium">Status</th>
              <th className="px-3 py-2 font-medium">Name</th>
              <th className="px-3 py-2 font-medium">Created</th>
              <th className="w-20 px-3 py-2" />
            </tr>
          </thead>
          <tbody>
            {loading && <tr><td colSpan={4} className="px-3 py-8 text-center text-muted-foreground">Loading…</td></tr>}
            {!loading && data?.items.length === 0 && (
              <tr><td colSpan={4} className="px-3 py-12">
                <div className="flex flex-col items-center gap-3 text-muted-foreground">
                  <ShibaMark className="h-10 w-10 opacity-60" />
                  <p>No events yet — let&apos;s plan one.</p>
                </div>
              </td></tr>
            )}
            {!loading && data?.items.map((e) => (
              <tr key={e.id} className="border-t">
                <td className="px-3 py-2">
                  <span className={"inline-flex rounded-full px-2.5 py-0.5 text-xs " + statusClass(e.status)}>{e.status}</span>
                </td>
                <td className="px-3 py-2 font-medium">{e.name}</td>
                <td className="px-3 py-2 text-muted-foreground">{new Date(e.createdAt).toLocaleDateString()}</td>
                <td className="px-3 py-2">
                  <Link href={`/admin/events/${e.id}`} className="inline-flex h-8 w-8 items-center justify-center rounded-md hover:bg-accent" aria-label="Open event">
                    <Eye className="h-4 w-4" />
                  </Link>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <div className="flex items-center justify-between">
        <p className="text-sm text-muted-foreground">{data ? `${data.total} total` : ""}</p>
        <div className="flex items-center gap-2">
          <button onClick={() => setPage((p) => Math.max(1, p - 1))} disabled={page === 1 || loading} className="h-8 rounded-md border px-3 text-sm hover:bg-accent disabled:opacity-50">Previous</button>
          <span className="text-sm text-muted-foreground">Page {page} of {totalPages}</span>
          <button onClick={() => setPage((p) => Math.min(totalPages, p + 1))} disabled={page >= totalPages || loading} className="h-8 rounded-md border px-3 text-sm hover:bg-accent disabled:opacity-50">Next</button>
        </div>
      </div>

      {showAdd && <AddDialog productLists={productLists} priceGroups={priceGroups} onClose={() => setShowAdd(false)} />}
    </div>
  )
}

function AddDialog({ productLists, priceGroups, onClose }: { productLists: NamedRow[]; priceGroups: NamedRow[]; onClose: () => void }) {
  const router = useRouter()
  const [name, setName] = useState("")
  const [description, setDescription] = useState("")
  const [imageUrl, setImageUrl] = useState("")
  const [productListId, setProductListId] = useState("")
  const [priceGroupId, setPriceGroupId] = useState("")
  const [submitting, setSubmitting] = useState(false)
  const [errors, setErrors] = useState<Record<string, string>>({})

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setSubmitting(true)
    setErrors({})
    try {
      const created = await api.post<{ id: string }>("/api/events", {
        name,
        description: description || null,
        imageUrl: imageUrl || null,
        productListId: productListId || null,
        priceGroupId: priceGroupId || null,
      })
      toast.success("Event created")
      router.push(`/admin/events/${created.id}`)
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
    <Modal title="New event" onClose={onClose}>
      <form onSubmit={handleSubmit} className="space-y-4">
        <Field label="Name" error={errors.name}>
          <input value={name} onChange={(e) => setName(e.target.value)} required placeholder="Spring jazz night" className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <Field label="Description" error={errors.description}>
          <input value={description} onChange={(e) => setDescription(e.target.value)} className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <Field label="Image URL" error={errors.imageurl}>
          <input value={imageUrl} onChange={(e) => setImageUrl(e.target.value)} placeholder="https://… (optional)" className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <Field label="Product list" hint="Optional — restricts the menu shown for this event." error={errors.productlistid}>
          <select value={productListId} onChange={(e) => setProductListId(e.target.value)} className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30">
            <option value="">— No special list —</option>
            {productLists.map((pl) => <option key={pl.id} value={pl.id}>{pl.name}</option>)}
          </select>
        </Field>
        <Field label="Price group" hint="Optional — overrides default product pricing." error={errors.pricegroupid}>
          <select value={priceGroupId} onChange={(e) => setPriceGroupId(e.target.value)} className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30">
            <option value="">— Default pricing —</option>
            {priceGroups.map((pg) => <option key={pg.id} value={pg.id}>{pg.name}</option>)}
          </select>
        </Field>
        <div className="flex justify-end gap-2">
          <button type="button" onClick={onClose} className="h-9 rounded-md px-3 text-sm hover:bg-accent">Cancel</button>
          <button type="submit" disabled={submitting} className="h-9 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90 disabled:opacity-50">{submitting ? "Saving…" : "Create event"}</button>
        </div>
      </form>
    </Modal>
  )
}

function Modal({ title, onClose, children }: { title: string; onClose: () => void; children: React.ReactNode }) {
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4" onClick={onClose}>
      <div onClick={(e) => e.stopPropagation()} className="w-full max-w-md rounded-lg border bg-card p-6 shadow-lg">
        <h2 className="text-lg font-semibold">{title}</h2>
        <div className="mt-4">{children}</div>
      </div>
    </div>
  )
}
