import { redirect } from "next/navigation"
import { revalidatePath } from "next/cache"
import { X } from "lucide-react"
import { Field } from "@/components/field"
import { api, type PagedResult } from "@/lib/server-api"
import { requirePermissionFor } from "@/features/auth/access"

interface ProductListDetail { id: string; name: string; productIds: string[] }
interface ProductRow { id: string; name: string }

export default async function EditProductListPage({ params }: { params: Promise<{ id: string }> }) {
  await requirePermissionFor("/admin/product-lists")
  const { id } = await params
  const [list, products] = await Promise.all([
    api.get<ProductListDetail>(`/api/product-lists/${id}`),
    api.get<PagedResult<ProductRow>>(`/api/products?page=1&pageSize=200&sort=name`),
  ])
  const attached = products.items.filter((p) => list.productIds.includes(p.id))
  const available = products.items.filter((p) => !list.productIds.includes(p.id))

  async function rename(formData: FormData) {
    "use server"
    await api.put(`/api/product-lists/${id}`, { name: formData.get("name") as string })
    revalidatePath(`/admin/product-lists/${id}`)
  }

  async function addProduct(formData: FormData) {
    "use server"
    const productId = formData.get("productId") as string
    if (!productId) return
    await api.post(`/api/product-lists/${id}/items`, { productId })
    revalidatePath(`/admin/product-lists/${id}`)
  }

  async function removeProduct(productId: string) {
    "use server"
    await api.delete(`/api/product-lists/${id}/items/${productId}`)
    revalidatePath(`/admin/product-lists/${id}`)
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">{list.name}</h1>
        <p className="text-muted-foreground">Rename or pick which products belong to this list.</p>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <section className="rounded-lg border bg-card p-4">
          <h2 className="mb-4 text-base font-semibold">Basics</h2>
          <form action={rename} className="space-y-4">
            <Field label="Name">
              <input name="name" defaultValue={list.name} required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
            </Field>
            <button type="submit" className="h-9 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90">Save changes</button>
          </form>
        </section>

        <section className="rounded-lg border bg-card p-4">
          <h2 className="mb-4 text-base font-semibold">Products in this list</h2>
          <div className="space-y-3">
            <div className="flex flex-wrap gap-2">
              {attached.length === 0 && <p className="text-sm text-muted-foreground">No products yet.</p>}
              {attached.map((p) => (
                <form key={p.id} action={removeProduct.bind(null, p.id)} className="inline-flex">
                  <button type="submit" className="inline-flex items-center gap-1.5 rounded-full bg-secondary px-2.5 py-0.5 text-xs hover:text-destructive" aria-label={`Remove ${p.name}`}>
                    {p.name}<X className="h-3 w-3" />
                  </button>
                </form>
              ))}
            </div>
            <form action={addProduct} className="flex gap-2">
              <select name="productId" className="h-9 max-w-xs flex-1 rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30">
                <option value="">Add product…</option>
                {available.map((p) => <option key={p.id} value={p.id}>{p.name}</option>)}
              </select>
              <button type="submit" className="h-9 rounded-md border px-3 text-sm hover:bg-accent">Add</button>
            </form>
          </div>
        </section>
      </div>
    </div>
  )
}
