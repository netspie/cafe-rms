import { AllergenForm } from "@/features/allergens/allergen-form"

export default function NewAllergenPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">New allergen</h1>
        <p className="text-muted-foreground">Add an allergen for your product catalog.</p>
      </div>
      <AllergenForm />
    </div>
  )
}
