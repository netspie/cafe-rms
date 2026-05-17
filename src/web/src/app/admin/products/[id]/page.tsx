import { revalidatePath } from "next/cache"
import { Trash2, X } from "lucide-react"
import { Field } from "@/components/field"
import { api, type PagedResult } from "@/lib/server-api"

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

export default async function EditProductPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params
  const [product, taxRates, tags, allergens, modifierGroups, priceGroups] = await Promise.all([
    api.get<ProductDetail>(`/api/products/${id}`),
    api.get<PagedResult<NamedRow>>(`/api/tax-rates?page=1&pageSize=100&sort=name`),
    api.get<PagedResult<NamedRow>>(`/api/tags?page=1&pageSize=200&sort=name`),
    api.get<PagedResult<NamedRow>>(`/api/allergens?page=1&pageSize=200&sort=name`),
    api.get<PagedResult<NamedRow>>(`/api/modifier-groups?page=1&pageSize=200&sort=name`),
    api.get<PagedResult<NamedRow>>(`/api/price-groups?page=1&pageSize=100&sort=name`),
  ])

  const path = `/admin/products/${id}`

  async function updateBasics(formData: FormData) {
    "use server"
    await api.put(`/api/products/${id}`, {
      name: formData.get("name") as string,
      description: (formData.get("description") as string) || null,
      barcode: (formData.get("barcode") as string) || null,
      taxRateId: formData.get("taxRateId") as string,
    })
    revalidatePath(path)
  }

  async function attachTag(formData: FormData) {
    "use server"
    const tagId = formData.get("tagId") as string
    if (!tagId) return
    await api.post(`/api/products/${id}/tags/${tagId}`)
    revalidatePath(path)
  }
  async function detachTag(tagId: string) {
    "use server"
    await api.delete(`/api/products/${id}/tags/${tagId}`)
    revalidatePath(path)
  }

  async function attachAllergen(formData: FormData) {
    "use server"
    const allergenId = formData.get("allergenId") as string
    if (!allergenId) return
    await api.post(`/api/products/${id}/allergens/${allergenId}`)
    revalidatePath(path)
  }
  async function detachAllergen(allergenId: string) {
    "use server"
    await api.delete(`/api/products/${id}/allergens/${allergenId}`)
    revalidatePath(path)
  }

  async function attachModifierGroup(formData: FormData) {
    "use server"
    const groupId = formData.get("groupId") as string
    if (!groupId) return
    await api.post(`/api/products/${id}/modifier-groups/${groupId}`)
    revalidatePath(path)
  }
  async function detachModifierGroup(groupId: string) {
    "use server"
    await api.delete(`/api/products/${id}/modifier-groups/${groupId}`)
    revalidatePath(path)
  }

  async function addImage(formData: FormData) {
    "use server"
    const url = formData.get("url") as string
    if (!url) return
    await api.post(`/api/products/${id}/images`, { url })
    revalidatePath(path)
  }
  async function removeImage(imageId: string) {
    "use server"
    await api.delete(`/api/products/${id}/images/${imageId}`)
    revalidatePath(path)
  }

  async function setPrice(formData: FormData) {
    "use server"
    const priceGroupId = formData.get("priceGroupId") as string
    const net = Number(formData.get("net"))
    await api.put(`/api/products/${id}/prices/${priceGroupId}`, { net })
    revalidatePath(path)
  }

  const tagsAttached = tags.items.filter((t) => product.tagIds.includes(t.id))
  const tagsAvailable = tags.items.filter((t) => !product.tagIds.includes(t.id))
  const allergensAttached = allergens.items.filter((a) => product.allergenIds.includes(a.id))
  const allergensAvailable = allergens.items.filter((a) => !product.allergenIds.includes(a.id))
  const groupsAttached = modifierGroups.items.filter((g) => product.modifierGroupIds.includes(g.id))
  const groupsAvailable = modifierGroups.items.filter((g) => !product.modifierGroupIds.includes(g.id))
  const priceFor = (groupId: string) => product.prices.find((p) => p.priceGroupId === groupId)?.net ?? ""

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">{product.name}</h1>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <section className="rounded-lg border bg-card p-4 lg:col-span-2">
          <h2 className="mb-4 text-base font-semibold">Basics</h2>
          <form action={updateBasics} className="max-w-xl space-y-4">
            <Field label="Name">
              <input name="name" defaultValue={product.name} required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
            </Field>
            <Field label="Description">
              <input name="description" defaultValue={product.description ?? ""} className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
            </Field>
            <Field label="Barcode">
              <input name="barcode" defaultValue={product.barcode ?? ""} className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
            </Field>
            <Field label="Tax rate">
              <select name="taxRateId" defaultValue={product.taxRateId} required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30">
                {taxRates.items.map((t) => <option key={t.id} value={t.id}>{t.name}</option>)}
              </select>
            </Field>
            <button type="submit" className="h-9 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90">Save changes</button>
          </form>
        </section>

        <section className="rounded-lg border bg-card p-4">
          <h2 className="mb-4 text-base font-semibold">Tags</h2>
          <div className="space-y-3">
            <div className="flex flex-wrap gap-2">
              {tagsAttached.length === 0 && <p className="text-sm text-muted-foreground">None attached.</p>}
              {tagsAttached.map((t) => (
                <form key={t.id} action={detachTag.bind(null, t.id)} className="inline-flex">
                  <button type="submit" className="inline-flex items-center gap-1.5 rounded-full bg-secondary px-2.5 py-0.5 text-xs hover:text-destructive">{t.name}<X className="h-3 w-3" /></button>
                </form>
              ))}
            </div>
            <form action={attachTag} className="flex gap-2">
              <select name="tagId" className="h-9 max-w-xs flex-1 rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30">
                <option value="">Add tag…</option>
                {tagsAvailable.map((t) => <option key={t.id} value={t.id}>{t.name}</option>)}
              </select>
              <button type="submit" className="h-9 rounded-md border px-3 text-sm hover:bg-accent">Add</button>
            </form>
          </div>
        </section>

        <section className="rounded-lg border bg-card p-4">
          <h2 className="mb-4 text-base font-semibold">Allergens</h2>
          <div className="space-y-3">
            <div className="flex flex-wrap gap-2">
              {allergensAttached.length === 0 && <p className="text-sm text-muted-foreground">None attached.</p>}
              {allergensAttached.map((a) => (
                <form key={a.id} action={detachAllergen.bind(null, a.id)} className="inline-flex">
                  <button type="submit" className="inline-flex items-center gap-1.5 rounded-full bg-secondary px-2.5 py-0.5 text-xs hover:text-destructive">{a.name}<X className="h-3 w-3" /></button>
                </form>
              ))}
            </div>
            <form action={attachAllergen} className="flex gap-2">
              <select name="allergenId" className="h-9 max-w-xs flex-1 rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30">
                <option value="">Add allergen…</option>
                {allergensAvailable.map((a) => <option key={a.id} value={a.id}>{a.name}</option>)}
              </select>
              <button type="submit" className="h-9 rounded-md border px-3 text-sm hover:bg-accent">Add</button>
            </form>
          </div>
        </section>

        <section className="rounded-lg border bg-card p-4">
          <h2 className="mb-4 text-base font-semibold">Modifier groups</h2>
          <div className="space-y-3">
            <div className="flex flex-wrap gap-2">
              {groupsAttached.length === 0 && <p className="text-sm text-muted-foreground">None attached.</p>}
              {groupsAttached.map((g) => (
                <form key={g.id} action={detachModifierGroup.bind(null, g.id)} className="inline-flex">
                  <button type="submit" className="inline-flex items-center gap-1.5 rounded-full bg-secondary px-2.5 py-0.5 text-xs hover:text-destructive">{g.name}<X className="h-3 w-3" /></button>
                </form>
              ))}
            </div>
            <form action={attachModifierGroup} className="flex gap-2">
              <select name="groupId" className="h-9 max-w-xs flex-1 rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30">
                <option value="">Add group…</option>
                {groupsAvailable.map((g) => <option key={g.id} value={g.id}>{g.name}</option>)}
              </select>
              <button type="submit" className="h-9 rounded-md border px-3 text-sm hover:bg-accent">Add</button>
            </form>
          </div>
        </section>

        <section className="rounded-lg border bg-card p-4">
          <h2 className="mb-4 text-base font-semibold">Images</h2>
          <div className="space-y-3">
            <div className="space-y-2">
              {product.images.length === 0 && <p className="text-sm text-muted-foreground">No images yet.</p>}
              {product.images.map((img) => (
                <div key={img.id} className="flex items-center gap-3 rounded-md border px-3 py-2">
                  <span className="flex-1 truncate font-mono text-xs text-muted-foreground">{img.url}</span>
                  <form action={removeImage.bind(null, img.id)}>
                    <button type="submit" className="inline-flex h-8 w-8 items-center justify-center rounded-md text-destructive hover:bg-destructive/10" aria-label="Remove image">
                      <Trash2 className="h-4 w-4" />
                    </button>
                  </form>
                </div>
              ))}
            </div>
            <form action={addImage} className="flex gap-2">
              <input name="url" className="h-9 max-w-md flex-1 rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
              <button type="submit" className="h-9 rounded-md border px-3 text-sm hover:bg-accent">Add image</button>
            </form>
          </div>
        </section>

        <section className="rounded-lg border bg-card p-4 lg:col-span-2">
          <h2 className="mb-4 text-base font-semibold">Prices per price group</h2>
          <div className="space-y-3">
            {priceGroups.items.length === 0 && <p className="text-sm text-muted-foreground">No price groups defined yet.</p>}
            {priceGroups.items.map((g) => (
              <form key={g.id} action={setPrice} className="flex items-center gap-3">
                <span className="w-40 text-sm font-medium">{g.name}</span>
                <input type="hidden" name="priceGroupId" value={g.id} />
                <input
                  name="net"
                  type="number"
                  step="0.01"
                  min="0"
                  defaultValue={priceFor(g.id)}
                  className="h-9 w-32 rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30"
                />
                <button type="submit" className="h-9 rounded-md border px-3 text-sm hover:bg-accent">Save</button>
              </form>
            ))}
          </div>
        </section>
      </div>
    </div>
  )
}
