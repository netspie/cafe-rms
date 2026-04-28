"use client"

// Sales channel — edit page. Two sections inlined: basics (name + isTakeout)
// and price-groups linker (attach/detach price groups for this channel).

import { use, useEffect, useState } from "react"
import { useRouter } from "next/navigation"
import { X } from "lucide-react"
import { toast } from "sonner"

import { Field } from "@/components/field"
import { ApiError, api, type PagedResult } from "@/lib/api"

interface SalesChannelDetail { id: string; name: string; isTakeout: boolean; priceGroupIds: string[] }
interface PriceGroupRow { id: string; name: string }

export default function EditSalesChannelPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params)
  const router = useRouter()

  const [channel, setChannel] = useState<SalesChannelDetail | null>(null)
  const [priceGroups, setPriceGroups] = useState<PriceGroupRow[]>([])
  const [loading, setLoading] = useState(true)
  const [refreshKey, setRefreshKey] = useState(0)

  useEffect(() => {
    async function load() {
      try {
        const [c, pgs] = await Promise.all([
          api.get<SalesChannelDetail>(`/api/sales-channels/${id}`),
          api.get<PagedResult<PriceGroupRow>>(`/api/price-groups?page=1&pageSize=100&sort=name`),
        ])
        setChannel(c)
        setPriceGroups(pgs.items)
      } catch (err) {
        if (err instanceof ApiError) toast.error(err.message)
      } finally { setLoading(false) }
    }
    load()
  }, [id, refreshKey])

  if (loading || !channel) return <p className="text-sm text-muted-foreground">Loading…</p>

  const attached = priceGroups.filter((g) => channel.priceGroupIds.includes(g.id))
  const available = priceGroups.filter((g) => !channel.priceGroupIds.includes(g.id))

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">{channel.name}</h1>
        <p className="text-muted-foreground">Rename, toggle takeout flag, link price groups.</p>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <BasicsCard channel={channel} onSaved={() => setRefreshKey((k) => k + 1)} />
        <PriceGroupsCard channelId={id} attached={attached} available={available} onChanged={() => setRefreshKey((k) => k + 1)} />
      </div>

      <button type="button" onClick={() => router.push("/admin/sales-channels")} className="h-9 rounded-md px-3 text-sm hover:bg-accent">
        Back to list
      </button>
    </div>
  )
}

function BasicsCard({ channel, onSaved }: { channel: SalesChannelDetail; onSaved: () => void }) {
  const [name, setName] = useState(channel.name)
  const [isTakeout, setIsTakeout] = useState(channel.isTakeout)
  const [submitting, setSubmitting] = useState(false)
  const [errors, setErrors] = useState<Record<string, string>>({})

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setSubmitting(true)
    setErrors({})
    try {
      await api.put(`/api/sales-channels/${channel.id}`, { name, isTakeout })
      toast.success("Sales channel updated")
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
        <label className="flex cursor-pointer items-center gap-2 text-sm">
          <input type="checkbox" checked={isTakeout} onChange={(e) => setIsTakeout(e.target.checked)} className="h-4 w-4" />
          <span>This channel is takeout (off-premises)</span>
        </label>
        <button type="submit" disabled={submitting} className="h-9 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90 disabled:opacity-50">
          {submitting ? "Saving…" : "Save changes"}
        </button>
      </form>
    </section>
  )
}

function PriceGroupsCard({ channelId, attached, available, onChanged }: { channelId: string; attached: PriceGroupRow[]; available: PriceGroupRow[]; onChanged: () => void }) {
  const [selectedId, setSelectedId] = useState("")

  async function link() {
    if (!selectedId) return
    try {
      await api.post(`/api/sales-channels/${channelId}/price-groups/${selectedId}`)
      toast.success("Price group linked")
      setSelectedId("")
      onChanged()
    } catch (err) {
      if (err instanceof ApiError) toast.error(err.message)
    }
  }

  async function unlink(priceGroupId: string) {
    try {
      await api.delete(`/api/sales-channels/${channelId}/price-groups/${priceGroupId}`)
      toast.success("Price group unlinked")
      onChanged()
    } catch (err) {
      if (err instanceof ApiError) toast.error(err.message)
    }
  }

  return (
    <section className="rounded-lg border bg-card p-4">
      <h2 className="mb-4 text-base font-semibold">Price groups</h2>
      <div className="space-y-3">
        <div className="flex flex-wrap gap-2">
          {attached.length === 0 && <p className="text-sm text-muted-foreground">No price groups linked.</p>}
          {attached.map((g) => (
            <span key={g.id} className="inline-flex items-center gap-1.5 rounded-full bg-secondary px-2.5 py-0.5 text-xs">
              {g.name}
              <button type="button" onClick={() => unlink(g.id)} className="hover:text-destructive" aria-label={`Unlink ${g.name}`}>
                <X className="h-3 w-3" />
              </button>
            </span>
          ))}
        </div>
        <div className="flex gap-2">
          <select value={selectedId} onChange={(e) => setSelectedId(e.target.value)} className="h-9 max-w-xs flex-1 rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30">
            <option value="">Link price group…</option>
            {available.map((g) => <option key={g.id} value={g.id}>{g.name}</option>)}
          </select>
          <button type="button" onClick={link} disabled={!selectedId} className="h-9 rounded-md border px-3 text-sm hover:bg-accent disabled:opacity-50">Link</button>
        </div>
      </div>
    </section>
  )
}
