import { ModifierForm } from "@/features/modifiers/modifier-form"

export default function NewModifierPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">New modifier</h1>
        <p className="text-muted-foreground">Pick the group, name it, set the price delta.</p>
      </div>
      <ModifierForm />
    </div>
  )
}
