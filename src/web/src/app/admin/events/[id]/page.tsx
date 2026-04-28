"use client"

// Event edit page. Single file with three inline sections:
// 1. Basics: name / description / image / product list / price group.
// 2. Days: list + add (date picker) + remove. Disabled once Closed/Cancelled.
// 3. Lifecycle actions: Publish (Draft → Published, requires ≥1 day),
//    Close (Published only), Cancel (Draft / Published with optional reason).

import { use, useEffect, useState } from "react"
import { useRouter } from "next/navigation"
import { Trash2 } from "lucide-react"
import { toast } from "sonner"

import { Field } from "@/components/field"
import { ApiError, api, type PagedResult } from "@/lib/api"

type EventStatus = "Draft" | "Published" | "Closed" | "Cancelled"

interface EventDay { id: string; date: string }

interface EventDetail {
  id: string
  name: string
  description: string | null
  imageUrl: string | null
  productListId: string | null
  priceGroupId: string | null
  status: EventStatus
  publishedAt: string | null
  closedAt: string | null
  cancelledAt: string | null
  cancellationReason: string | null
  days: EventDay[]
  createdAt: string
}

interface NamedRow { id: string; name: string }

function statusClass(status: EventStatus): string {
  if (status === "Published") return "bg-primary/10 text-primary"
  if (status === "Cancelled") return "bg-destructive/10 text-destructive"
  if (status === "Closed") return "bg-secondary text-secondary-foreground"
  return "border bg-transparent"
}

export default function EditEventPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params)
  const router = useRouter()

  const [event, setEvent] = useState<EventDetail | null>(null)
  const [productLists, setProductLists] = useState<NamedRow[]>([])
  const [priceGroups, setPriceGroups] = useState<NamedRow[]>([])
  const [loading, setLoading] = useState(true)
  const [refreshKey, setRefreshKey] = useState(0)

  useEffect(() => {
    async function load() {
      try {
        const [ev, pls, pgs] = await Promise.all([
          api.get<EventDetail>(`/api/events/${id}`),
          api.get<PagedResult<NamedRow>>(`/api/product-lists?page=1&pageSize=100&sort=name`),
          api.get<PagedResult<NamedRow>>(`/api/price-groups?page=1&pageSize=100&sort=name`),
        ])
        setEvent(ev)
        setProductLists(pls.items)
        setPriceGroups(pgs.items)
      } catch (err) {
        if (err instanceof ApiError) toast.error(err.message)
      } finally { setLoading(false) }
    }
    load()
  }, [id, refreshKey])

  if (loading || !event) return <p className="text-sm text-muted-foreground">Loading…</p>

  const refresh = () => setRefreshKey((k) => k + 1)
  const isClosedOrCancelled = event.status === "Closed" || event.status === "Cancelled"

  return (
    <div className="space-y-6">
      <div className="flex items-start justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">{event.name}</h1>
          <div className="mt-2 flex items-center gap-3">
            <span className={"inline-flex rounded-full px-2.5 py-0.5 text-xs " + statusClass(event.status)}>{event.status}</span>
            {event.publishedAt && (
              <span className="text-sm text-muted-foreground">Published {new Date(event.publishedAt).toLocaleString()}</span>
            )}
          </div>
        </div>
        <ActionsBar event={event} onChanged={refresh} />
      </div>

      <BasicsSection event={event} productLists={productLists} priceGroups={priceGroups} disabled={isClosedOrCancelled} onSaved={refresh} />

      <DaysSection eventId={event.id} days={event.days} disabled={isClosedOrCancelled} onChanged={refresh} />

      {event.cancellationReason && (
        <section className="rounded-lg border bg-card p-4">
          <h2 className="mb-2 text-base font-semibold">Cancellation reason</h2>
          <p className="text-sm text-muted-foreground">{event.cancellationReason}</p>
        </section>
      )}

      <button type="button" onClick={() => router.push("/admin/events")} className="h-9 rounded-md px-3 text-sm hover:bg-accent">
        Back to list
      </button>
    </div>
  )
}

function BasicsSection({ event, productLists, priceGroups, disabled, onSaved }: { event: EventDetail; productLists: NamedRow[]; priceGroups: NamedRow[]; disabled: boolean; onSaved: () => void }) {
  const [name, setName] = useState(event.name)
  const [description, setDescription] = useState(event.description ?? "")
  const [imageUrl, setImageUrl] = useState(event.imageUrl ?? "")
  const [productListId, setProductListId] = useState(event.productListId ?? "")
  const [priceGroupId, setPriceGroupId] = useState(event.priceGroupId ?? "")
  const [submitting, setSubmitting] = useState(false)
  const [errors, setErrors] = useState<Record<string, string>>({})

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setSubmitting(true)
    setErrors({})
    try {
      await api.put(`/api/events/${event.id}`, {
        name,
        description: description || null,
        imageUrl: imageUrl || null,
        productListId: productListId || null,
        priceGroupId: priceGroupId || null,
      })
      toast.success("Event updated")
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
      {disabled && <p className="mb-3 text-xs text-muted-foreground">Closed or cancelled — basics are read-only.</p>}
      <form onSubmit={handleSubmit} className="max-w-xl space-y-4">
        <Field label="Name" error={errors.name}>
          <input value={name} onChange={(e) => setName(e.target.value)} required disabled={disabled} className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30 disabled:opacity-60" />
        </Field>
        <Field label="Description" error={errors.description}>
          <input value={description} onChange={(e) => setDescription(e.target.value)} disabled={disabled} className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30 disabled:opacity-60" />
        </Field>
        <Field label="Image URL" error={errors.imageurl}>
          <input value={imageUrl} onChange={(e) => setImageUrl(e.target.value)} disabled={disabled} placeholder="https://… (optional)" className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30 disabled:opacity-60" />
        </Field>
        <Field label="Product list" hint="Optional." error={errors.productlistid}>
          <select value={productListId} onChange={(e) => setProductListId(e.target.value)} disabled={disabled} className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30 disabled:opacity-60">
            <option value="">— No special list —</option>
            {productLists.map((pl) => <option key={pl.id} value={pl.id}>{pl.name}</option>)}
          </select>
        </Field>
        <Field label="Price group" hint="Optional." error={errors.pricegroupid}>
          <select value={priceGroupId} onChange={(e) => setPriceGroupId(e.target.value)} disabled={disabled} className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30 disabled:opacity-60">
            <option value="">— Default pricing —</option>
            {priceGroups.map((pg) => <option key={pg.id} value={pg.id}>{pg.name}</option>)}
          </select>
        </Field>
        <button type="submit" disabled={submitting || disabled} className="h-9 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90 disabled:opacity-50">
          {submitting ? "Saving…" : "Save changes"}
        </button>
      </form>
    </section>
  )
}

function DaysSection({ eventId, days, disabled, onChanged }: { eventId: string; days: EventDay[]; disabled: boolean; onChanged: () => void }) {
  const [date, setDate] = useState("")
  const [submitting, setSubmitting] = useState(false)

  async function add() {
    if (!date) return
    setSubmitting(true)
    try {
      await api.post(`/api/events/${eventId}/days`, { date })
      toast.success("Day added")
      setDate("")
      onChanged()
    } catch (err) {
      if (err instanceof ApiError) toast.error(err.message)
    } finally { setSubmitting(false) }
  }

  async function remove(dayId: string) {
    try {
      await api.delete(`/api/events/${eventId}/days/${dayId}`)
      toast.success("Day removed")
      onChanged()
    } catch (err) {
      if (err instanceof ApiError) toast.error(err.message)
    }
  }

  return (
    <section className="rounded-lg border bg-card p-4">
      <h2 className="mb-4 text-base font-semibold">Days</h2>
      <div className="space-y-3">
        <div className="space-y-2">
          {days.length === 0 && <p className="text-sm text-muted-foreground">No days scheduled yet.</p>}
          {days.map((d) => (
            <div key={d.id} className="flex items-center gap-3 rounded-md border px-3 py-2">
              <span className="flex-1 text-sm font-medium">
                {new Date(d.date).toLocaleDateString(undefined, { weekday: "long", year: "numeric", month: "long", day: "numeric" })}
              </span>
              {!disabled && (
                <button type="button" onClick={() => remove(d.id)} className="inline-flex h-8 w-8 items-center justify-center rounded-md text-destructive hover:bg-destructive/10" aria-label="Remove day">
                  <Trash2 className="h-4 w-4" />
                </button>
              )}
            </div>
          ))}
        </div>
        {!disabled && (
          <div className="flex gap-2">
            <input type="date" value={date} onChange={(e) => setDate(e.target.value)} className="h-9 max-w-xs rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
            <button type="button" onClick={add} disabled={!date || submitting} className="h-9 rounded-md border px-3 text-sm hover:bg-accent disabled:opacity-50">Add day</button>
          </div>
        )}
      </div>
    </section>
  )
}

function ActionsBar({ event, onChanged }: { event: EventDetail; onChanged: () => void }) {
  const [showCancel, setShowCancel] = useState(false)
  const [reason, setReason] = useState("")
  const [busy, setBusy] = useState<null | "publish" | "close" | "cancel">(null)

  const isDraft = event.status === "Draft"
  const isPublished = event.status === "Published"
  const isPublishable = isDraft && event.days.length > 0

  async function publish() {
    setBusy("publish")
    try {
      await api.post(`/api/events/${event.id}/publish`)
      toast.success("Event published")
      onChanged()
    } catch (err) {
      if (err instanceof ApiError) toast.error(err.message)
    } finally { setBusy(null) }
  }

  async function close() {
    setBusy("close")
    try {
      await api.post(`/api/events/${event.id}/close`)
      toast.success("Event closed")
      onChanged()
    } catch (err) {
      if (err instanceof ApiError) toast.error(err.message)
    } finally { setBusy(null) }
  }

  async function cancel() {
    setBusy("cancel")
    try {
      await api.post(`/api/events/${event.id}/cancel`, { reason: reason || null })
      toast.success("Event cancelled")
      setShowCancel(false)
      setReason("")
      onChanged()
    } catch (err) {
      if (err instanceof ApiError) toast.error(err.message)
    } finally { setBusy(null) }
  }

  return (
    <div className="flex flex-wrap items-center gap-2">
      {isDraft && (
        <button onClick={publish} disabled={!isPublishable || busy === "publish"} className="h-9 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90 disabled:opacity-50">
          {busy === "publish" ? "Publishing…" : "Publish"}
        </button>
      )}
      {isPublished && (
        <button onClick={close} disabled={busy === "close"} className="h-9 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90 disabled:opacity-50">
          {busy === "close" ? "Closing…" : "Close event"}
        </button>
      )}
      {(isDraft || isPublished) && (
        <button onClick={() => setShowCancel(true)} className="h-9 rounded-md bg-destructive px-3 text-sm font-medium text-destructive-foreground hover:opacity-90">
          Cancel event
        </button>
      )}
      {isDraft && !isPublishable && (
        <p className="text-sm text-muted-foreground">Add at least one day before publishing.</p>
      )}

      {showCancel && (
        <Modal title="Cancel this event?" onClose={() => setShowCancel(false)}>
          <p className="text-sm text-muted-foreground">Optional reason — appears on the event record afterwards.</p>
          <input
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            placeholder="Reason (optional)"
            className="mt-4 h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30"
          />
          <div className="mt-6 flex justify-end gap-2">
            <button onClick={() => setShowCancel(false)} className="h-9 rounded-md px-3 text-sm hover:bg-accent">Keep event</button>
            <button onClick={cancel} disabled={busy === "cancel"} className="h-9 rounded-md bg-destructive px-3 text-sm font-medium text-destructive-foreground hover:opacity-90 disabled:opacity-50">
              {busy === "cancel" ? "Cancelling…" : "Cancel event"}
            </button>
          </div>
        </Modal>
      )}
    </div>
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
