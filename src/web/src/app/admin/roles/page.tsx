import { RolesTable } from "@/features/roles/roles-table"

export default function RolesListPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Roles</h1>
        <p className="text-muted-foreground">Permission groups assigned to staff users.</p>
      </div>
      <RolesTable />
    </div>
  )
}
