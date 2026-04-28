import { PromotionCodeForm } from "@/features/promotion-codes/promotion-code-form"

export default function NewPromotionCodePage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">New promotion code</h1>
        <p className="text-muted-foreground">Set the percentage, optional date window, and use cap.</p>
      </div>
      <PromotionCodeForm />
    </div>
  )
}
