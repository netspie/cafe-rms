import { revalidatePath } from "next/cache"
import { Trash2 } from "lucide-react"
import { Field } from "@/components/field"
import { api, type PagedResult } from "@/lib/server-api"

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
}
interface NamedRow { id: string; name: string }

function statusClass(status: EventStatus): string {
  if (status === "Published") return "bg-primary/10 text-primary"
  if (status === "Cancelled") return "bg-destructive/10 text-destructive"
  if (status === "Closed") return "bg-secondary text-secondary-foreground"
  return "border bg-transparent"
}

export default async function EditEventPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params
  const [event, productLists, priceGroups] = await Promise.all([
    api.get<EventDetail>(`/api/events/${id}`),
    api.get<PagedResult<NamedRow>>(`/api/product-lists?page=1&pageSize=100&sort=name`),
    api.get<PagedResult<NamedRow>>(`/api/price-groups?page=1&pageSize=100&sort=name`),
  ])
  const path = `/admin/events/${id}`
  const isClosedOrCancelled = event.status === "Closed" || event.status === "Cancelled"
  const isDraft = event.status === "Draft"
  const isPublished = event.status === "Published"
  const isPublishable = isDraft && event.days.length > 0

  async function updateBasics(formData: FormData) {
    "use server"
    await api.put(`/api/events/${id}`, {
      name: formData.get("name") as string,
      description: (formData.get("description") as string) || null,
      imageUrl: (formData.get("imageUrl") as string) || null,
      productListId: (formData.get("productListId") as string) || null,
      priceGroupId: (formData.get("priceGroupId") as string) || null,
    })
    revalidatePath(path)
  }

  async function addDay(formData: FormData) {
    "use server"
    const date = formData.get("date") as string
    if (!date) return
    await api.post(`/api/events/${id}/days`, { date })
    revalidatePath(path)
  }

  async function removeDay(dayId: string) {
    "use server"
    await api.delete(`/api/events/${id}/days/${dayId}`)
    revalidatePath(path)
  }

  async function publish() {
    "use server"
    await api.post(`/api/events/${id}/publish`)
    revalidatePath(path)
  }

  async function close() {
    "use server"
    await api.post(`/api/events/${id}/close`)
    revalidatePath(path)
  }

  async function cancel(formData: FormData) {
    "use server"
    const reason = (formData.get("reason") as string) || null
    await api.post(`/api/events/${id}/cancel`, { reason })
    revalidatePath(path)
  }

  return (
    <div className="space-y-6">
      <div className="flex items-start justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">{event.name}</h1>
          <div className="mt-2 flex items-center gap-3">
            <span className={"inline-flex rounded-full px-2.5 py-0.5 text-xs " + statusClass(event.status)}>{event.status}</span>
            {event.publishedAt && <span className="text-sm text-muted-foreground">Published {new Date(event.publishedAt).toLocaleString()}</span>}
          </div>
        </div>
        <div className="flex flex-wrap items-center gap-2">
          {isDraft && (
            <form action={publish}>
              <button type="submit" disabled={!isPublishable} className="h-9 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90 disabled:opacity-50">Publish</button>
            </form>
          )}
          {isPublished && (
            <form action={close}>
              <button type="submit" className="h-9 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90">Close event</button>
            </form>
          )}
          {isDraft && !isPublishable && <p className="text-sm text-muted-foreground">Add at least one day before publishing.</p>}
        </div>
      </div>

      <section className="rounded-lg border bg-card p-4">
        <h2 className="mb-4 text-base font-semibold">Basics</h2>
        {isClosedOrCancelled && <p className="mb-3 text-xs text-muted-foreground">Closed or cancelled — basics are read-only.</p>}
        <form action={updateBasics} className="max-w-xl space-y-4">
          <Field label="Name">
            <input name="name" defaultValue={event.name} required disabled={isClosedOrCancelled} className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30 disabled:opacity-60" />
          </Field>
          <Field label="Description">
            <input name="description" defaultValue={event.description ?? ""} disabled={isClosedOrCancelled} className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30 disabled:opacity-60" />
          </Field>
          <Field label="Image URL">
            <input name="imageUrl" defaultValue={event.imageUrl ?? ""} placeholder="https://… (optional)" disabled={isClosedOrCancelled} className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30 disabled:opacity-60" />
          </Field>
          <Field label="Product list">
            <select name="productListId" defaultValue={event.productListId ?? ""} disabled={isClosedOrCancelled} className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30 disabled:opacity-60">
              <option value="">— No special list —</option>
              {productLists.items.map((pl) => <option key={pl.id} value={pl.id}>{pl.name}</option>)}
            </select>
          </Field>
          <Field label="Price group">
            <select name="priceGroupId" defaultValue={event.priceGroupId ?? ""} disabled={isClosedOrCancelled} className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30 disabled:opacity-60">
              <option value="">— Default pricing —</option>
              {priceGroups.items.map((pg) => <option key={pg.id} value={pg.id}>{pg.name}</option>)}
            </select>
          </Field>
          <button type="submit" disabled={isClosedOrCancelled} className="h-9 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90 disabled:opacity-50">Save changes</button>
        </form>
      </section>

      <section className="rounded-lg border bg-card p-4">
        <h2 className="mb-4 text-base font-semibold">Days</h2>
        <div className="space-y-3">
          <div className="space-y-2">
            {event.days.length === 0 && <p className="text-sm text-muted-foreground">No days scheduled yet.</p>}
            {event.days.map((d) => (
              <div key={d.id} className="flex items-center gap-3 rounded-md border px-3 py-2">
                <span className="flex-1 text-sm font-medium">
                  {new Date(d.date).toLocaleDateString(undefined, { weekday: "long", year: "numeric", month: "long", day: "numeric" })}
                </span>
                {!isClosedOrCancelled && (
                  <form action={removeDay.bind(null, d.id)}>
                    <button type="submit" className="inline-flex h-8 w-8 items-center justify-center rounded-md text-destructive hover:bg-destructive/10" aria-label="Remove day"><Trash2 className="h-4 w-4" /></button>
                  </form>
                )}
              </div>
            ))}
          </div>
          {!isClosedOrCancelled && (
            <form action={addDay} className="flex gap-2">
              <input name="date" type="date" required className="h-9 max-w-xs rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
              <button type="submit" className="h-9 rounded-md border px-3 text-sm hover:bg-accent">Add day</button>
            </form>
          )}
        </div>
      </section>

      {(isDraft || isPublished) && (
        <section className="rounded-lg border bg-card p-4">
          <h2 className="mb-2 text-base font-semibold">Cancel this event</h2>
          <p className="mb-4 text-sm text-muted-foreground">Optional reason — appears on the event record afterwards.</p>
          <form action={cancel} className="flex gap-2">
            <input name="reason" placeholder="Reason (optional)" className="h-9 max-w-md flex-1 rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
            <button type="submit" className="h-9 rounded-md bg-destructive px-3 text-sm font-medium text-destructive-foreground hover:opacity-90">Cancel event</button>
          </form>
        </section>
      )}

      {event.cancellationReason && (
        <section className="rounded-lg border bg-card p-4">
          <h2 className="mb-2 text-base font-semibold">Cancellation reason</h2>
          <p className="text-sm text-muted-foreground">{event.cancellationReason}</p>
        </section>
      )}
    </div>
  )
}
