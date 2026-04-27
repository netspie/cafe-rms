import { TaxRatesTable } from "@/features/tax-rates/tax-rates-table"

export default function TaxRatesListPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Tax rates</h1>
        <p className="text-muted-foreground">VAT rates applied to products.</p>
      </div>
      <TaxRatesTable />
    </div>
  )
}
