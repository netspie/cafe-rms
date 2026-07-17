import { redirect } from "next/navigation"
import { revalidatePath } from "next/cache"
import { Field } from "@/components/field"
import { api } from "@/lib/server-api"
import { requirePermissionFor } from "@/features/auth/access"

interface PriceGroupDetail { id: string; name: string }

export default async function EditPriceGroupPage({ params }: { params: Promise<{ id: string }> }) {
  await requirePermissionFor("/admin/price-groups")
  const { id } = await params
  const item = await api.get<PriceGroupDetail>(`/api/price-groups/${id}`)

  async function updatePriceGroup(formData: FormData) {
    "use server"
    await api.put(`/api/price-groups/${id}`, { name: formData.get("name") as string })
    revalidatePath("/admin/price-groups")
    redirect("/admin/price-groups")
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Edit price group</h1>
        <p className="text-muted-foreground">Rename this pricing tier.</p>
      </div>
      <form action={updatePriceGroup} className="max-w-xl space-y-4">
        <Field label="Name">
          <input name="name" defaultValue={item.name} required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <button type="submit" className="h-9 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90">Save changes</button>
      </form>
    </div>
  )
}
