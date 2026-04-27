import { TablesTable } from "@/features/tables/tables-table"

export default function TablesListPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Tables</h1>
        <p className="text-muted-foreground">Physical tables in your outlet for walk-in orders.</p>
      </div>
      <TablesTable />
    </div>
  )
}
