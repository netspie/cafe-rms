import { PriceGroupForm } from "@/features/price-groups/price-group-form"

export default function NewPriceGroupPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">New price group</h1>
        <p className="text-muted-foreground">Add a pricing tier customers can be assigned to.</p>
      </div>
      <PriceGroupForm />
    </div>
  )
}
