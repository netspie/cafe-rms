import Link from "next/link"
import { redirect } from "next/navigation"
import { Download, Eye, Plus, Trash2 } from "lucide-react"
import { revalidatePath } from "next/cache"
import { Field } from "@/components/field"
import { ShibaMark } from "@/components/shiba-mark"
import { SortableTh } from "@/components/sortable-th"
import { api, postFormData, type PagedResult } from "@/lib/server-api"
import { getMyPermissions, requirePermissionFor } from "@/features/auth/access"
import { PlaceholderReference } from "@/features/printout/placeholders"

type EventStatus = "Draft" | "Published" | "Closed" | "Cancelled"

interface EventItem { id: string; name: string; status: EventStatus; createdAt: string }
interface NamedRow { id: string; name: string }
interface TemplateRow { id: string; name: string; fileName: string }

const PAGE_SIZE = 20

function statusClass(status: EventStatus): string {
  if (status === "Published") return "bg-primary/10 text-primary"
  if (status === "Cancelled") return "bg-destructive/10 text-destructive"
  if (status === "Closed") return "bg-secondary text-secondary-foreground"
  return "border bg-transparent"
}

async function createEvent(formData: FormData) {
  "use server"
  const created = await api.post<{ id: string }>("/api/events", {
    name: formData.get("name") as string,
    description: (formData.get("description") as string) || null,
    imageUrl: (formData.get("imageUrl") as string) || null,
    productListId: (formData.get("productListId") as string) || null,
    priceGroupId: (formData.get("priceGroupId") as string) || null,
  })
  revalidatePath("/admin/events")
  redirect(`/admin/events/${created.id}`)
}

const CONFIRMATION_TEMPLATE_NAME = "Potwierdzenie wydarzenia"

async function replaceTemplate(formData: FormData) {
  "use server"
  const templateId = formData.get("templateId") as string
  const templateName = formData.get("templateName") as string
  const file = formData.get("file") as File | null
  if (!templateId || !file || file.size === 0) return
  const upload = new FormData()
  upload.append("name", templateName)
  upload.append("file", file)
  await postFormData(`/api/printout-templates/${templateId}`, upload, "PUT")
  revalidatePath("/admin/events")
}

async function addTemplate(formData: FormData) {
  "use server"
  const file = formData.get("file") as File | null
  if (!file || file.size === 0) return
  const upload = new FormData()
  upload.append("name", CONFIRMATION_TEMPLATE_NAME)
  upload.append("file", file)
  await postFormData("/api/printout-templates", upload)
  revalidatePath("/admin/events")
}

async function deleteTemplate(formData: FormData) {
  "use server"
  const templateId = formData.get("templateId") as string
  if (!templateId) return
  await api.delete(`/api/printout-templates/${templateId}`)
  revalidatePath("/admin/events")
}

export default async function EventsListPage({ searchParams }: { searchParams: Promise<{ page?: string; q?: string; sort?: string }> }) {
  await requirePermissionFor("/admin/events")
  const sp = await searchParams
  const page = Number(sp.page ?? 1)
  const filter = sp.q ?? ""
  const sort = sp.sort ?? "-createdAt"
  const search = new URLSearchParams({ page: String(page), pageSize: String(PAGE_SIZE), sort })
  if (filter) search.set("name", filter)
  const [data, productLists, priceGroups] = await Promise.all([
    api.get<PagedResult<EventItem>>(`/api/events?${search}`),
    api.get<PagedResult<NamedRow>>(`/api/product-lists?page=1&pageSize=100&sort=name`),
    api.get<PagedResult<NamedRow>>(`/api/price-groups?page=1&pageSize=100&sort=name`),
  ])
  const totalPages = Math.max(1, Math.ceil(data.total / PAGE_SIZE))
  const preserve: Record<string, string> = {}
  if (filter) preserve.q = filter

  const permissions = await getMyPermissions()
  const canManageTemplate = permissions.includes("PrintoutTemplatesManage")
  const templates = canManageTemplate
    ? await api.get<PagedResult<TemplateRow>>(`/api/printout-templates?page=1&pageSize=1&sort=name`)
    : null
  const template = templates?.items[0] ?? null

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Events</h1>
      </div>

      <form className="flex items-center gap-3">
        <input name="q" defaultValue={filter} className="h-9 max-w-xs rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        <button type="submit" className="h-9 rounded-md border px-3 text-sm hover:bg-accent">Search</button>
      </form>

      <form action={createEvent} className="grid gap-3 rounded-md border bg-card p-3 sm:grid-cols-2 lg:grid-cols-[2fr_2fr_2fr_1.5fr_1.5fr_auto]">
        <Field label="Name">
          <input name="name" required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <Field label="Description">
          <input name="description" className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <Field label="Image URL">
          <input name="imageUrl" className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <Field label="Product list">
          <select name="productListId" className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30">
            <option value="">— No special list —</option>
            {productLists.items.map((pl) => <option key={pl.id} value={pl.id}>{pl.name}</option>)}
          </select>
        </Field>
        <Field label="Price group">
          <select name="priceGroupId" className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30">
            <option value="">— Default pricing —</option>
            {priceGroups.items.map((pg) => <option key={pg.id} value={pg.id}>{pg.name}</option>)}
          </select>
        </Field>
        <button type="submit" className="inline-flex h-9 items-center gap-2 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90 lg:self-end">
          <Plus className="h-4 w-4" />Add
        </button>
      </form>

      <div className="overflow-hidden rounded-md border">
        <table className="w-full text-sm">
          <thead className="bg-muted/50 text-left">
            <tr>
              <th className="px-3 py-2 font-medium">Status</th>
              <SortableTh label="Name" field="name" currentSort={sort} preserve={preserve} />
              <SortableTh label="Created" field="createdAt" currentSort={sort} preserve={preserve} />
              <th className="w-20 px-3 py-2" />
            </tr>
          </thead>
          <tbody>
            {data.items.length === 0 && (
              <tr><td colSpan={4} className="px-3 py-12">
                <div className="flex flex-col items-center gap-3 text-muted-foreground">
                  <ShibaMark className="h-10 w-10 opacity-60" />
                  <p>No events match this filter.</p>
                </div>
              </td></tr>
            )}
            {data.items.map((e) => (
              <tr key={e.id} className="border-t">
                <td className="px-3 py-2"><span className={"inline-flex rounded-full px-2.5 py-0.5 text-xs " + statusClass(e.status)}>{e.status}</span></td>
                <td className="px-3 py-2 font-medium">{e.name}</td>
                <td className="px-3 py-2 text-muted-foreground">{new Date(e.createdAt).toLocaleDateString()}</td>
                <td className="px-3 py-2">
                  <Link href={`/admin/events/${e.id}`} className="inline-flex h-8 w-8 items-center justify-center rounded-md hover:bg-accent" aria-label="Open event"><Eye className="h-4 w-4" /></Link>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <div className="flex items-center justify-between">
        <p className="text-sm text-muted-foreground">{data.total} total</p>
        <div className="flex items-center gap-2">
          {page > 1 ? <Link href={{ query: { ...preserve, sort, page: page - 1 } }} className="inline-flex h-8 items-center rounded-md border px-3 text-sm hover:bg-accent">Previous</Link> : <span className="inline-flex h-8 items-center rounded-md border px-3 text-sm opacity-50">Previous</span>}
          <span className="text-sm text-muted-foreground">Page {page} of {totalPages}</span>
          {page < totalPages ? <Link href={{ query: { ...preserve, sort, page: page + 1 } }} className="inline-flex h-8 items-center rounded-md border px-3 text-sm hover:bg-accent">Next</Link> : <span className="inline-flex h-8 items-center rounded-md border px-3 text-sm opacity-50">Next</span>}
        </div>
      </div>

      {canManageTemplate && (
        <section id="confirmation-template" className="scroll-mt-6 rounded-lg border bg-card p-4">
          <h2 className="mb-1 text-base font-semibold">Confirmation template</h2>
          <p className="mb-4 text-sm text-muted-foreground">
            The Word (<span className="font-mono">.docx</span>) template merged with an event&apos;s data by the
            Download confirmation button. Shared by every event — replacing it here changes the confirmation for all of them.
          </p>
          <div className="grid gap-4 md:grid-cols-2">
            {template ? (
              <div className="space-y-3">
                <form action={replaceTemplate} className="space-y-3">
                  <input type="hidden" name="templateId" value={template.id} />
                  <input type="hidden" name="templateName" value={template.name} />
                  <Field label="Current file">
                    <div className="flex items-center gap-3">
                      <span className="font-mono text-xs text-muted-foreground">{template.fileName}</span>
                      <a href={`/admin/printout-templates/download/${template.id}`} className="inline-flex h-8 items-center gap-1.5 rounded-md border px-2.5 text-sm hover:bg-accent">
                        <Download className="h-4 w-4" />Download
                      </a>
                    </div>
                  </Field>
                  <Field label="Replace file (.docx)">
                    <input name="file" type="file" accept=".docx" required className="h-9 w-full rounded-md border bg-transparent px-3 py-1.5 text-sm outline-none file:mr-3 file:rounded file:border-0 file:bg-accent file:px-2 file:py-1 file:text-sm" />
                  </Field>
                  <button type="submit" className="h-9 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90">Upload new template</button>
                </form>
                <form action={deleteTemplate}>
                  <input type="hidden" name="templateId" value={template.id} />
                  <button type="submit" className="inline-flex h-9 items-center gap-2 rounded-md border border-destructive/40 px-3 text-sm font-medium text-destructive hover:bg-destructive/10">
                    <Trash2 className="h-4 w-4" />Delete template
                  </button>
                </form>
              </div>
            ) : (
              <form action={addTemplate} className="space-y-3">
                <p className="text-sm text-muted-foreground">No template — Download confirmation will fail until you add one.</p>
                <Field label="Template file (.docx)">
                  <input name="file" type="file" accept=".docx" required className="h-9 w-full rounded-md border bg-transparent px-3 py-1.5 text-sm outline-none file:mr-3 file:rounded file:border-0 file:bg-accent file:px-2 file:py-1 file:text-sm" />
                </Field>
                <button type="submit" className="h-9 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90">Add template</button>
              </form>
            )}
            <PlaceholderReference />
          </div>
        </section>
      )}
    </div>
  )
}
