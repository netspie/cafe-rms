import { redirect } from "next/navigation"
import { revalidatePath } from "next/cache"
import { Field } from "@/components/field"
import { api } from "@/lib/server-api"

interface PromotionCodeDetail {
  id: string
  code: string
  discountPercentage: number
  validFrom: string | null
  validUntil: string | null
  maxUses: number | null
  usesCount: number
}

const toDateInput = (iso: string | null) => iso ? iso.slice(0, 10) : ""

export default async function EditPromotionCodePage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params
  const item = await api.get<PromotionCodeDetail>(`/api/promotion-codes/${id}`)

  async function updatePromotionCode(formData: FormData) {
    "use server"
    const validFrom = formData.get("validFrom") as string
    const validUntil = formData.get("validUntil") as string
    const maxUses = formData.get("maxUses") as string
    await api.put(`/api/promotion-codes/${id}`, {
      code: formData.get("code") as string,
      discountPercentage: Number(formData.get("discountPercentage")),
      validFrom: validFrom ? new Date(validFrom).toISOString() : null,
      validUntil: validUntil ? new Date(validUntil).toISOString() : null,
      maxUses: maxUses ? Number(maxUses) : null,
    })
    revalidatePath("/admin/promotion-codes")
    redirect("/admin/promotion-codes")
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Edit promotion code</h1>
        <p className="text-muted-foreground">Used {item.usesCount} time{item.usesCount === 1 ? "" : "s"} so far.</p>
      </div>
      <form action={updatePromotionCode} className="max-w-xl space-y-4">
        <Field label="Code">
          <input name="code" defaultValue={item.code} required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <Field label="Discount (%)">
          <input name="discountPercentage" type="number" step="0.01" min="0" max="100" defaultValue={item.discountPercentage} required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <div className="grid grid-cols-2 gap-3">
          <Field label="Valid from" hint="Optional.">
            <input name="validFrom" type="date" defaultValue={toDateInput(item.validFrom)} className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
          </Field>
          <Field label="Valid until" hint="Optional.">
            <input name="validUntil" type="date" defaultValue={toDateInput(item.validUntil)} className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
          </Field>
        </div>
        <Field label="Max uses" hint="Empty = unlimited.">
          <input name="maxUses" type="number" min="1" step="1" defaultValue={item.maxUses ?? ""} className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <button type="submit" className="h-9 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90">Save changes</button>
      </form>
    </div>
  )
}
