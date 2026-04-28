import { ModifierGroupForm } from "@/features/modifier-groups/modifier-group-form"

export default function NewModifierGroupPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">New modifier group</h1>
        <p className="text-muted-foreground">Group modifiers shown together to a customer.</p>
      </div>
      <ModifierGroupForm />
    </div>
  )
}
