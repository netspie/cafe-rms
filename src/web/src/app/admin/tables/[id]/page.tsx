import { redirect } from "next/navigation"
import { revalidatePath } from "next/cache"
import { Field } from "@/components/field"
import { api } from "@/lib/server-api"

interface TableDetail { id: string; name: string }

export default async function EditTablePage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params
  const item = await api.get<TableDetail>(`/api/tables/${id}`)

  async function updateTable(formData: FormData) {
    "use server"
    await api.put(`/api/tables/${id}`, { name: formData.get("name") as string })
    revalidatePath("/admin/tables")
    redirect("/admin/tables")
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Edit table</h1>
        <p className="text-muted-foreground">Rename this table.</p>
      </div>
      <form action={updateTable} className="max-w-xl space-y-4">
        <Field label="Name">
          <input name="name" defaultValue={item.name} required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <button type="submit" className="h-9 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90">Save changes</button>
      </form>
    </div>
  )
}
