import { redirect } from "next/navigation"
import { revalidatePath } from "next/cache"
import { Field } from "@/components/field"
import { api } from "@/lib/server-api"

interface TemplateDetail { id: string; name: string; templateFileUrl: string }

export default async function EditPrintoutTemplatePage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params
  const item = await api.get<TemplateDetail>(`/api/printout-templates/${id}`)

  async function updateTemplate(formData: FormData) {
    "use server"
    await api.put(`/api/printout-templates/${id}`, {
      name: formData.get("name") as string,
      templateFileUrl: formData.get("templateFileUrl") as string,
    })
    revalidatePath("/admin/printout-templates")
    redirect("/admin/printout-templates")
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Edit template</h1>
        <p className="text-muted-foreground">Update name or file URL.</p>
      </div>
      <form action={updateTemplate} className="max-w-xl space-y-4">
        <Field label="Name">
          <input name="name" defaultValue={item.name} required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <Field label="Template file URL">
          <input name="templateFileUrl" defaultValue={item.templateFileUrl} required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <button type="submit" className="h-9 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90">Save changes</button>
      </form>
    </div>
  )
}
