import { redirect } from "next/navigation"
import { revalidatePath } from "next/cache"
import { Field } from "@/components/field"
import { api } from "@/lib/server-api"
import { requirePermissionFor } from "@/features/auth/access"

interface AllergenDetail { id: string; name: string }

export default async function EditAllergenPage({ params }: { params: Promise<{ id: string }> }) {
  await requirePermissionFor("/admin/allergens")
  const { id } = await params
  const allergen = await api.get<AllergenDetail>(`/api/allergens/${id}`)

  async function updateAllergen(formData: FormData) {
    "use server"
    await api.put(`/api/allergens/${id}`, { name: formData.get("name") as string })
    revalidatePath("/admin/allergens")
    redirect("/admin/allergens")
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Edit allergen</h1>
      </div>
      <form action={updateAllergen} className="max-w-xl space-y-4">
        <Field label="Name">
          <input name="name" defaultValue={allergen.name} required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <button type="submit" className="h-9 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90">Save changes</button>
      </form>
    </div>
  )
}
