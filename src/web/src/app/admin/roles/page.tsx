import Link from "next/link"
import { Pencil, Plus, Trash2 } from "lucide-react"
import { revalidatePath } from "next/cache"
import { Field } from "@/components/field"
import { ShibaMark } from "@/components/shiba-mark"
import { SortableTh } from "@/components/sortable-th"
import { api } from "@/lib/server-api"
import { ALL_PERMISSIONS, PERMISSION_LABELS } from "@/features/auth/permissions"

interface RoleItem { id: string; name: string; permissions: string[] }

async function createRole(formData: FormData) {
  "use server"
  const permissions = formData.getAll("permissions").map((v) => v.toString())
  await api.post("/api/roles", { name: formData.get("name") as string, permissions })
  revalidatePath("/admin/roles")
}

async function deleteRole(id: string) {
  "use server"
  await api.delete(`/api/roles/${id}`)
  revalidatePath("/admin/roles")
}

export default async function RolesListPage({ searchParams }: { searchParams: Promise<{ sort?: string }> }) {
  const sp = await searchParams
  const sort = sp.sort ?? "name"
  const roles = await api.get<RoleItem[]>("/api/roles")

  const sortedRoles = sortRoles(roles, sort)
  const preserve: Record<string, string> = {}

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Roles</h1>
      </div>

      <form action={createRole} className="space-y-4 rounded-md border bg-card p-3">
        <Field label="Name">
          <input name="name" required className="h-9 max-w-sm rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <div className="space-y-2">
          <p className="text-sm font-medium">Permissions</p>
          <div className="grid grid-cols-1 gap-1.5 rounded-md border p-3 sm:grid-cols-2">
            {ALL_PERMISSIONS.map((perm) => (
              <label key={perm} className="flex cursor-pointer items-center gap-2 rounded-md px-2 py-1 text-sm hover:bg-accent/50">
                <input type="checkbox" name="permissions" value={perm} className="h-4 w-4" />
                <span>{PERMISSION_LABELS[perm]}</span>
              </label>
            ))}
          </div>
        </div>
        <button type="submit" className="inline-flex h-9 items-center gap-2 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90">
          <Plus className="h-4 w-4" />Create role
        </button>
      </form>

      <div className="overflow-hidden rounded-md border">
        <table className="w-full text-sm">
          <thead className="bg-muted/50 text-left">
            <tr>
              <SortableTh label="Name" field="name" currentSort={sort} preserve={preserve} />
              <th className="px-3 py-2 font-medium">Permissions</th>
              <th className="w-20 px-3 py-2" />
            </tr>
          </thead>
          <tbody>
            {sortedRoles.length === 0 && (
              <tr><td colSpan={3} className="px-3 py-12">
                <div className="flex flex-col items-center gap-3 text-muted-foreground">
                  <ShibaMark className="h-10 w-10 opacity-60" />
                  <p>No roles yet — define one to start grouping permissions.</p>
                </div>
              </td></tr>
            )}
            {sortedRoles.map((r) => (
              <tr key={r.id} className="border-t">
                <td className="px-3 py-2 font-medium">{r.name}</td>
                <td className="px-3 py-2">
                  <div className="flex flex-wrap gap-1">
                    {r.permissions.length === 0 && <span className="text-xs text-muted-foreground">—</span>}
                    {r.permissions.map((p) => <span key={p} className="inline-flex rounded-full border px-2 py-0.5 font-mono text-xs">{p}</span>)}
                  </div>
                </td>
                <td className="px-3 py-2">
                  <div className="flex justify-end gap-1">
                    <Link href={`/admin/roles/${r.id}`} className="inline-flex h-8 w-8 items-center justify-center rounded-md hover:bg-accent" aria-label="Edit"><Pencil className="h-4 w-4" /></Link>
                    <form action={deleteRole.bind(null, r.id)}>
                      <button type="submit" className="inline-flex h-8 w-8 items-center justify-center rounded-md text-destructive hover:bg-destructive/10" aria-label="Delete"><Trash2 className="h-4 w-4" /></button>
                    </form>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}

function sortRoles(roles: RoleItem[], sortExpression: string): RoleItem[] {
  const isDescending = sortExpression.startsWith("-")
  const field = isDescending ? sortExpression.slice(1) : sortExpression
  const multiplier = isDescending ? -1 : 1
  if (field !== "name") return roles
  return [...roles].sort((a, b) => a.name.localeCompare(b.name) * multiplier)
}
