import { redirect } from "next/navigation"
import { revalidatePath } from "next/cache"
import { Field } from "@/components/field"
import { api } from "@/lib/server-api"

interface TaxRateDetail { id: string; name: string; description: string; rate: number }

export default async function EditTaxRatePage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params
  const item = await api.get<TaxRateDetail>(`/api/tax-rates/${id}`)

  async function updateTaxRate(formData: FormData) {
    "use server"
    await api.put(`/api/tax-rates/${id}`, {
      name: formData.get("name") as string,
      description: (formData.get("description") as string) ?? "",
      rate: Number(formData.get("rate")),
    })
    revalidatePath("/admin/tax-rates")
    redirect("/admin/tax-rates")
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Edit tax rate</h1>
        <p className="text-muted-foreground">Update name, description, or rate.</p>
      </div>
      <form action={updateTaxRate} className="max-w-xl space-y-4">
        <Field label="Name">
          <input name="name" defaultValue={item.name} required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <Field label="Description">
          <input name="description" defaultValue={item.description} className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <Field label="Rate (%)">
          <input name="rate" type="number" step="0.01" min="0" max="100" defaultValue={item.rate} required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <button type="submit" className="h-9 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90">Save changes</button>
      </form>
    </div>
  )
}
