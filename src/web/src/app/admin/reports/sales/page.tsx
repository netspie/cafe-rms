import { api } from "@/lib/server-api"
import { requirePermissionFor } from "@/features/auth/access"
import { SalesChart } from "./sales-chart"

interface Bucket {
  period: string
  revenue: number
  ordersCount: number
  itemsSold: number
}

interface SalesReport {
  from: string
  to: string
  granularity: string
  buckets: Bucket[]
  totalRevenue: number
  totalOrders: number
  totalItems: number
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

export default async function SalesReportPage({
  searchParams,
}: {
  searchParams: Promise<{ from?: string; to?: string; granularity?: string }>
}) {
  await requirePermissionFor("/admin/reports/sales")
  const sp = await searchParams
  const from = sp.from ?? daysAgo(30)
  const to = sp.to ?? formatToday()
  const granularity = sp.granularity ?? "day"

  const query = new URLSearchParams({ from, to, granularity })
  const data = await api.get<SalesReport>(`/api/reports/sales?${query}`)

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Sales Report</h1>
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
          <label className="space-y-1">
            <span className="block text-xs font-medium text-muted-foreground">Granularity</span>
            <select
              name="granularity"
              defaultValue={granularity}
              className="h-9 rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30"
            >
              <option value="day">Day</option>
              <option value="month">Month</option>
              <option value="quarter">Quarter</option>
            </select>
          </label>
          <button
            type="submit"
            className="h-9 rounded-md bg-primary px-4 text-sm font-medium text-primary-foreground hover:opacity-90"
          >
            Apply
          </button>
          <div className="ml-auto flex items-center gap-2">
            <a
              href={`/admin/reports/sales/export-pdf?${query}`}
              className="inline-flex h-9 items-center rounded-md border px-3 text-sm hover:bg-accent"
            >
              Export PDF
            </a>
            <a
              href={`/admin/reports/sales/export-xlsx?${query}`}
              className="inline-flex h-9 items-center rounded-md border px-3 text-sm hover:bg-accent"
            >
              Export Excel
            </a>
          </div>
        </form>
      </section>

      <section className="grid gap-4 md:grid-cols-3">
        <Stat label="Total Revenue" value={`${data.totalRevenue.toFixed(2)} PLN`} />
        <Stat label="Orders" value={`${data.totalOrders}`} />
        <Stat label="Items Sold" value={`${data.totalItems}`} />
      </section>

      <section className="rounded-lg border bg-card p-4">
        <h2 className="mb-4 text-base font-semibold">Revenue per {granularity}</h2>
        <SalesChart buckets={data.buckets} />
      </section>

      <section className="rounded-lg border bg-card p-4">
        <h2 className="mb-4 text-base font-semibold">Breakdown</h2>
        <div className="overflow-hidden rounded-md border">
          <table className="w-full text-sm">
            <thead className="bg-muted/50 text-left">
              <tr>
                <th className="px-3 py-2 font-medium">Period</th>
                <th className="px-3 py-2 text-right font-medium">Revenue (PLN)</th>
                <th className="px-3 py-2 text-right font-medium">Orders</th>
                <th className="px-3 py-2 text-right font-medium">Items</th>
              </tr>
            </thead>
            <tbody>
              {data.buckets.length === 0 && (
                <tr>
                  <td colSpan={4} className="px-3 py-12 text-center text-muted-foreground">
                    No data in the selected range.
                  </td>
                </tr>
              )}
              {data.buckets.map((b) => (
                <tr key={b.period} className="border-t">
                  <td className="px-3 py-2">{b.period}</td>
                  <td className="px-3 py-2 text-right font-mono">{b.revenue.toFixed(2)}</td>
                  <td className="px-3 py-2 text-right">{b.ordersCount}</td>
                  <td className="px-3 py-2 text-right">{b.itemsSold}</td>
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
