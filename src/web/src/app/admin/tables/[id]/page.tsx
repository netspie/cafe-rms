import Link from "next/link"
import { redirect } from "next/navigation"
import { revalidatePath } from "next/cache"
import { Field } from "@/components/field"
import { api, type PagedResult } from "@/lib/server-api"

interface TableDetail { id: string; name: string }
interface OpenOrder { id: string; createdAt: string; total: number }

export default async function EditTablePage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params
  const [item, openOrders] = await Promise.all([
    api.get<TableDetail>(`/api/tables/${id}`),
    api.get<PagedResult<OpenOrder>>(`/api/orders?status=Placed&tableId=${id}&pageSize=100&sort=-createdAt`),
  ])
  const openTotal = openOrders.items.reduce((sum, o) => sum + o.total, 0)

  async function updateTable(formData: FormData) {
    "use server"
    await api.put(`/api/tables/${id}`, { name: formData.get("name") as string })
    revalidatePath("/admin/tables")
    redirect("/admin/tables")
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">{item.name}</h1>
        <p className="text-muted-foreground">Open orders on this table, and table settings.</p>
      </div>

      <section className="rounded-lg border bg-card p-4">
        <div className="mb-3 flex items-baseline justify-between">
          <h2 className="text-base font-semibold">Open orders</h2>
          <span className="text-sm text-muted-foreground">{openOrders.items.length} open, {openTotal.toFixed(2)} total</span>
        </div>
        {openOrders.items.length === 0 ? (
          <p className="text-sm text-muted-foreground">No open orders on this table.</p>
        ) : (
          <ul className="divide-y">
            {openOrders.items.map((o) => (
              <li key={o.id}>
                <Link href={`/admin/orders/${o.id}`} className="flex items-center justify-between gap-4 py-2 hover:text-primary">
                  <span className="font-mono text-xs text-muted-foreground">{o.id.slice(0, 8)}…</span>
                  <span className="text-sm text-muted-foreground">{new Date(o.createdAt).toLocaleString()}</span>
                  <span className="font-mono text-sm">{o.total.toFixed(2)}</span>
                </Link>
              </li>
            ))}
          </ul>
        )}
      </section>

      <form action={updateTable} className="max-w-xl space-y-4">
        <h2 className="text-base font-semibold">Rename table</h2>
        <Field label="Name">
          <input name="name" defaultValue={item.name} required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <button type="submit" className="h-9 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90">Save changes</button>
      </form>
    </div>
  )
}
