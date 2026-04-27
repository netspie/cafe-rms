import { RoleForm } from "@/features/roles/role-form"

export default function NewRolePage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">New role</h1>
        <p className="text-muted-foreground">Pick which permissions this role grants.</p>
      </div>
      <RoleForm />
    </div>
  )
}
