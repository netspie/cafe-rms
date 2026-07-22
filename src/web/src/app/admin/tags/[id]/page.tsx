import { redirect } from "next/navigation"
import { revalidatePath } from "next/cache"
import { Field } from "@/components/field"
import { api } from "@/lib/server-api"
import { requirePermissionFor } from "@/features/auth/access"

interface TagDetail {
  id: string
  name: string
}

export default async function EditTagPage({ params }: { params: Promise<{ id: string }> }) {
  await requirePermissionFor("/admin/tags")
  const { id } = await params
  const tag = await api.get<TagDetail>(`/api/tags/${id}`)

  async function updateTag(formData: FormData) {
    "use server"
    const name = formData.get("name") as string
    await api.put(`/api/tags/${id}`, { name })
    revalidatePath("/admin/tags")
    redirect("/admin/tags")
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Edit tag</h1>
        <p className="text-muted-foreground">Update the tag name.</p>
      </div>
      <form action={updateTag} className="max-w-xl space-y-4">
        <Field label="Name">
          <input name="name" defaultValue={tag.name} required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <button type="submit" className="h-9 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90">
          Save changes
        </button>
      </form>
    </div>
  )
}
