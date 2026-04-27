import { AllergensTable } from "@/features/allergens/allergens-table"

export default function AllergensListPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Allergens</h1>
        <p className="text-muted-foreground">Allergen labels you can attach to products for safety info.</p>
      </div>
      <AllergensTable />
    </div>
  )
}
