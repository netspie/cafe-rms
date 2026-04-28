import { ModifiersTable } from "@/features/modifiers/modifiers-table"

export default function ModifiersListPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Modifiers</h1>
        <p className="text-muted-foreground">Customizations attached to products via groups.</p>
      </div>
      <ModifiersTable />
    </div>
  )
}
