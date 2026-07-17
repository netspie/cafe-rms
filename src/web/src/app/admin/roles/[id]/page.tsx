import { redirect } from "next/navigation"
import { revalidatePath } from "next/cache"
import { Field } from "@/components/field"
import { api } from "@/lib/server-api"
import { ALL_PERMISSIONS, PERMISSION_LABELS } from "@/features/auth/permissions"
import { requirePermissionFor } from "@/features/auth/access"

interface RoleItem { id: string; name: string; permissions: string[] }

export default async function EditRolePage({ params }: { params: Promise<{ id: string }> }) {
  await requirePermissionFor("/admin/roles")
  const { id } = await params
  const roles = await api.get<RoleItem[]>("/api/roles")
  const role = roles.find((r) => r.id === id)
  if (!role) return <p className="text-sm text-muted-foreground">Role not found.</p>

  async function updateRole(formData: FormData) {
    "use server"
    const permissions = formData.getAll("permissions").map((v) => v.toString())
    await api.put(`/api/roles/${id}`, { name: formData.get("name") as string, permissions })
    revalidatePath("/admin/roles")
    redirect("/admin/roles")
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Edit role</h1>
        <p className="text-muted-foreground">Rename or change which permissions this role grants.</p>
      </div>
      <form action={updateRole} className="max-w-2xl space-y-6">
        <Field label="Name">
          <input name="name" defaultValue={role.name} required className="h-9 max-w-sm rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <div className="space-y-2">
          <p className="text-sm font-medium">Permissions</p>
          <div className="grid grid-cols-1 gap-1.5 rounded-md border p-3 sm:grid-cols-2">
            {ALL_PERMISSIONS.map((perm) => (
              <label key={perm} className="flex cursor-pointer items-center gap-2 rounded-md px-2 py-1 text-sm hover:bg-accent/50">
                <input type="checkbox" name="permissions" value={perm} defaultChecked={role.permissions.includes(perm)} className="h-4 w-4" />
                <span>{PERMISSION_LABELS[perm]}</span>
              </label>
            ))}
          </div>
        </div>
        <button type="submit" className="h-9 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90">Save changes</button>
      </form>
    </div>
  )
}
