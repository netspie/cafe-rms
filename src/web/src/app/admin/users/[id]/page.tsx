import { revalidatePath } from "next/cache"
import { X } from "lucide-react"
import { Field } from "@/components/field"
import { api } from "@/lib/server-api"
import { requirePermissionFor } from "@/features/auth/access"

interface UserDetail { id: string; email: string; firstName: string; lastName: string; roles: string[] }
interface RoleRow { id: string; name: string; permissions: string[] }

export default async function EditUserPage({ params }: { params: Promise<{ id: string }> }) {
  await requirePermissionFor("/admin/users")
  const { id } = await params
  const [user, roles] = await Promise.all([
    api.get<UserDetail>(`/api/users/${id}`),
    api.get<RoleRow[]>(`/api/roles`),
  ])
  const path = `/admin/users/${id}`
  const attached = roles.filter((r) => user.roles.includes(r.name))
  const available = roles.filter((r) => !user.roles.includes(r.name))

  async function updateProfile(formData: FormData) {
    "use server"
    await api.put(`/api/users/${id}`, {
      firstName: formData.get("firstName") as string,
      lastName: formData.get("lastName") as string,
    })
    revalidatePath(path)
  }

  async function assignRole(formData: FormData) {
    "use server"
    const roleId = formData.get("roleId") as string
    if (!roleId) return
    await api.post(`/api/users/${id}/roles/${roleId}`)
    revalidatePath(path)
  }

  async function unassignRole(roleId: string) {
    "use server"
    await api.delete(`/api/users/${id}/roles/${roleId}`)
    revalidatePath(path)
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">{user.firstName} {user.lastName}</h1>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <section className="rounded-lg border bg-card p-4">
          <h2 className="mb-4 text-base font-semibold">Profile</h2>
          <form action={updateProfile} className="space-y-4">
            <div>
              <p className="text-sm font-medium text-muted-foreground">Email</p>
              <p className="text-sm font-mono">{user.email}</p>
            </div>
            <div className="grid grid-cols-2 gap-3">
              <Field label="First name">
                <input name="firstName" defaultValue={user.firstName} required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
              </Field>
              <Field label="Last name">
                <input name="lastName" defaultValue={user.lastName} required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
              </Field>
            </div>
            <button type="submit" className="h-9 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90">Save changes</button>
          </form>
        </section>

        <section className="rounded-lg border bg-card p-4">
          <h2 className="mb-4 text-base font-semibold">Roles</h2>
          <div className="space-y-3">
            <div className="flex flex-wrap gap-2">
              {attached.length === 0 && <p className="text-sm text-muted-foreground">No roles assigned.</p>}
              {attached.map((r) => (
                <form key={r.id} action={unassignRole.bind(null, r.id)} className="inline-flex">
                  <button type="submit" className="inline-flex items-center gap-1.5 rounded-full bg-secondary px-2.5 py-0.5 text-xs hover:text-destructive">{r.name}<X className="h-3 w-3" /></button>
                </form>
              ))}
            </div>
            <form action={assignRole} className="flex gap-2">
              <select name="roleId" className="h-9 max-w-xs flex-1 rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30">
                <option value="">Assign role…</option>
                {available.map((r) => <option key={r.id} value={r.id}>{r.name}</option>)}
              </select>
              <button type="submit" className="h-9 rounded-md border px-3 text-sm hover:bg-accent">Assign</button>
            </form>
          </div>
        </section>
      </div>
    </div>
  )
}
