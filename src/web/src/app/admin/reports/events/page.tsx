import { api } from "@/lib/server-api"
import { requirePermissionFor } from "@/features/auth/access"
import { EventsChart } from "./events-chart"

interface EventRow {
  eventId: string
  eventName: string
  firstDay: string | null
  lastDay: string | null
  dayCount: number
  attendeesCount: number
  ordersCount: number
  itemsSold: number
  revenue: number
}

interface EventReport {
  from: string
  to: string
  events: EventRow[]
  totalEvents: number
  totalAttendees: number
  totalOrders: number
  totalItems: number
  totalRevenue: number
}

function formatToday(): string {
  const d = new Date()
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}-${String(d.getDate()).padStart(2, "0")}`
}

function daysAgo(n: number): string {
  const d = new Date()
  d.setDate(d.getDate() - n)
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}-${String(d.getDate()).padStart(2, "0")}`
}

function formatDateRange(first: string | null, last: string | null): string {
  if (first === null) return "—"
  if (first === last) return first
  return `${first} → ${last}`
}

export default async function EventAttendanceReportPage({
  searchParams,
}: {
  searchParams: Promise<{ from?: string; to?: string }>
}) {
  await requirePermissionFor("/admin/reports/events")
  const sp = await searchParams
  const from = sp.from ?? daysAgo(90)
  const to = sp.to ?? formatToday()

  const query = new URLSearchParams({ from, to })
  const data = await api.get<EventReport>(`/api/reports/events?${query}`)

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Event Attendance Report</h1>
      </div>

      <section className="rounded-lg border bg-card p-4">
        <form className="flex flex-wrap items-end gap-3">
          <label className="space-y-1">
            <span className="block text-xs font-medium text-muted-foreground">From</span>
            <input
              type="date"
              name="from"
              defaultValue={from}
              className="h-9 rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30"
            />
          </label>
          <label className="space-y-1">
            <span className="block text-xs font-medium text-muted-foreground">To</span>
            <input
              type="date"
              name="to"
              defaultValue={to}
              className="h-9 rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30"
            />
          </label>
          <button
            type="submit"
            className="h-9 rounded-md bg-primary px-4 text-sm font-medium text-primary-foreground hover:opacity-90"
          >
            Apply
          </button>
          <div className="ml-auto flex items-center gap-2">
            <a
              href={`/admin/reports/events/export-pdf?${query}`}
              className="inline-flex h-9 items-center rounded-md border px-3 text-sm hover:bg-accent"
            >
              Export PDF
            </a>
            <a
              href={`/admin/reports/events/export-xlsx?${query}`}
              className="inline-flex h-9 items-center rounded-md border px-3 text-sm hover:bg-accent"
            >
              Export Excel
            </a>
          </div>
        </form>
      </section>

      <section className="grid gap-4 md:grid-cols-4">
        <Stat label="Events" value={`${data.totalEvents}`} />
        <Stat label="Attendees" value={`${data.totalAttendees}`} />
        <Stat label="Orders" value={`${data.totalOrders}`} />
        <Stat label="Revenue" value={`${data.totalRevenue.toFixed(2)} PLN`} />
      </section>

      <section className="rounded-lg border bg-card p-4">
        <h2 className="mb-4 text-base font-semibold">Revenue per event</h2>
        <EventsChart events={data.events} />
      </section>

      <section className="rounded-lg border bg-card p-4">
        <h2 className="mb-4 text-base font-semibold">Breakdown</h2>
        <div className="overflow-hidden rounded-md border">
          <table className="w-full text-sm">
            <thead className="bg-muted/50 text-left">
              <tr>
                <th className="px-3 py-2 font-medium">Event</th>
                <th className="px-3 py-2 font-medium">Dates</th>
                <th className="px-3 py-2 text-right font-medium">Days</th>
                <th className="px-3 py-2 text-right font-medium">Attendees</th>
                <th className="px-3 py-2 text-right font-medium">Orders</th>
                <th className="px-3 py-2 text-right font-medium">Items</th>
                <th className="px-3 py-2 text-right font-medium">Revenue (PLN)</th>
              </tr>
            </thead>
            <tbody>
              {data.events.length === 0 && (
                <tr>
                  <td colSpan={7} className="px-3 py-12 text-center text-muted-foreground">
                    No events in the selected range.
                  </td>
                </tr>
              )}
              {data.events.map((e) => (
                <tr key={e.eventId} className="border-t">
                  <td className="px-3 py-2">{e.eventName}</td>
                  <td className="px-3 py-2 text-muted-foreground">{formatDateRange(e.firstDay, e.lastDay)}</td>
                  <td className="px-3 py-2 text-right">{e.dayCount}</td>
                  <td className="px-3 py-2 text-right">{e.attendeesCount}</td>
                  <td className="px-3 py-2 text-right">{e.ordersCount}</td>
                  <td className="px-3 py-2 text-right">{e.itemsSold}</td>
                  <td className="px-3 py-2 text-right font-mono">{e.revenue.toFixed(2)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </section>
    </div>
  )
}

function Stat({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-lg border bg-card p-4">
      <p className="text-xs font-medium text-muted-foreground">{label}</p>
      <p className="mt-1 text-2xl font-semibold tabular-nums">{value}</p>
    </div>
  )
}
