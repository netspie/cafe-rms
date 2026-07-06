import { api } from "@/lib/server-api"
import { ProductsChart } from "./products-chart"

interface ProductRow {
  productId: string
  productName: string
  quantitySold: number
  net: number
  vat: number
  gross: number
}

interface ProductsReport {
  from: string
  to: string
  eventId: string | null
  products: ProductRow[]
  totalItems: number
  totalNet: number
  totalVat: number
  totalGross: number
}

interface EventItem {
  id: string
  name: string
}

interface Paged<T> {
  items: T[]
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

export default async function ProductsReportPage({
  searchParams,
}: {
  searchParams: Promise<{ from?: string; to?: string; eventId?: string }>
}) {
  const sp = await searchParams
  const from = sp.from ?? daysAgo(30)
  const to = sp.to ?? formatToday()
  const eventId = sp.eventId ?? ""

  const query = new URLSearchParams({ from, to })
  if (eventId) query.set("eventId", eventId)

  const [data, events] = await Promise.all([
    api.get<ProductsReport>(`/api/reports/products?${query}`),
    api.get<Paged<EventItem>>(`/api/events?pageSize=100`),
  ])

  const selectedEvent = events.items.find((e) => e.id === eventId)

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Sales per Product</h1>
        {selectedEvent && (
          <p className="text-muted-foreground">Products sold at “{selectedEvent.name}”.</p>
        )}
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
            <span className="block text-xs font-medium text-muted-foreground">Event</span>
            <select
              name="eventId"
              defaultValue={eventId}
              className="h-9 rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30"
            >
              <option value="">All events</option>
              {events.items.map((e) => (
                <option key={e.id} value={e.id}>
                  {e.name}
                </option>
              ))}
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
              href={`/admin/reports/products/export-pdf?${query}`}
              className="inline-flex h-9 items-center rounded-md border px-3 text-sm hover:bg-accent"
            >
              Export PDF
            </a>
            <a
              href={`/admin/reports/products/export-xlsx?${query}`}
              className="inline-flex h-9 items-center rounded-md border px-3 text-sm hover:bg-accent"
            >
              Export Excel
            </a>
          </div>
        </form>
      </section>

      <section className="grid gap-4 md:grid-cols-3">
        <Stat label="Gross Revenue" value={`${data.totalGross.toFixed(2)} PLN`} />
        <Stat label="Items Sold" value={`${data.totalItems}`} />
        <Stat label="Products" value={`${data.products.length}`} />
      </section>

      <section className="rounded-lg border bg-card p-4">
        <h2 className="mb-4 text-base font-semibold">Gross revenue per product</h2>
        <ProductsChart products={data.products} />
      </section>

      <section className="rounded-lg border bg-card p-4">
        <h2 className="mb-4 text-base font-semibold">Breakdown</h2>
        <div className="overflow-hidden rounded-md border">
          <table className="w-full text-sm">
            <thead className="bg-muted/50 text-left">
              <tr>
                <th className="px-3 py-2 font-medium">Product</th>
                <th className="px-3 py-2 text-right font-medium">Qty</th>
                <th className="px-3 py-2 text-right font-medium">Net (PLN)</th>
                <th className="px-3 py-2 text-right font-medium">VAT (PLN)</th>
                <th className="px-3 py-2 text-right font-medium">Gross (PLN)</th>
              </tr>
            </thead>
            <tbody>
              {data.products.length === 0 && (
                <tr>
                  <td colSpan={5} className="px-3 py-12 text-center text-muted-foreground">
                    No sales in the selected range.
                  </td>
                </tr>
              )}
              {data.products.map((p) => (
                <tr key={p.productId} className="border-t">
                  <td className="px-3 py-2">{p.productName}</td>
                  <td className="px-3 py-2 text-right">{p.quantitySold}</td>
                  <td className="px-3 py-2 text-right font-mono">{p.net.toFixed(2)}</td>
                  <td className="px-3 py-2 text-right font-mono">{p.vat.toFixed(2)}</td>
                  <td className="px-3 py-2 text-right font-mono">{p.gross.toFixed(2)}</td>
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
