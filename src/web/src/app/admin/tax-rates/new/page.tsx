import { TaxRateForm } from "@/features/tax-rates/tax-rate-form"

export default function NewTaxRatePage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">New tax rate</h1>
        <p className="text-muted-foreground">Add a tax rate to assign to products.</p>
      </div>
      <TaxRateForm />
    </div>
  )
}
