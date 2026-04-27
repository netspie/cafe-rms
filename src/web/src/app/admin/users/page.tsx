import { UsersTable } from "@/features/users/users-table"

export default function UsersListPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Users</h1>
        <p className="text-muted-foreground">Staff members with admin panel access.</p>
      </div>
      <UsersTable />
    </div>
  )
}
