import { redirect } from "next/navigation"
import { revalidatePath } from "next/cache"
import { Field } from "@/components/field"
import { api, type PagedResult } from "@/lib/server-api"
import { requirePermissionFor } from "@/features/auth/access"

interface ModifierDetail { id: string; name: string; modifierGroupId: string }
interface NamedRow { id: string; name: string }

export default async function EditModifierPage({ params }: { params: Promise<{ id: string }> }) {
  await requirePermissionFor("/admin/modifiers")
  const { id } = await params
  const [item, groups] = await Promise.all([
    api.get<ModifierDetail>(`/api/modifiers/${id}`),
    api.get<PagedResult<NamedRow>>(`/api/modifier-groups?page=1&pageSize=100&sort=name`),
  ])
  const groupName = groups.items.find((g) => g.id === item.modifierGroupId)?.name ?? "—"

  async function updateModifier(formData: FormData) {
    "use server"
    await api.put(`/api/modifiers/${id}`, {
      name: formData.get("name") as string,
    })
    revalidatePath("/admin/modifiers")
    redirect("/admin/modifiers")
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Edit modifier</h1>
        <p className="text-muted-foreground">Group is fixed after creation.</p>
      </div>
      <form action={updateModifier} className="max-w-xl space-y-4">
        <div>
          <p className="text-sm font-medium">Group</p>
          <p className="text-sm text-muted-foreground">{groupName}</p>
        </div>
        <Field label="Name">
          <input name="name" defaultValue={item.name} required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <button type="submit" className="h-9 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90">Save changes</button>
      </form>
    </div>
  )
}
