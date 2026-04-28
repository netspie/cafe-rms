import { ModifierGroupsTable } from "@/features/modifier-groups/modifier-groups-table"

export default function ModifierGroupsListPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Modifier groups</h1>
        <p className="text-muted-foreground">Buckets of modifiers (e.g. milk choice, sugar level).</p>
      </div>
      <ModifierGroupsTable />
    </div>
  )
}
