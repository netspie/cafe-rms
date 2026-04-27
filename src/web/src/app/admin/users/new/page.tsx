import { RegisterStaffForm } from "@/features/users/register-staff-form"

export default function NewUserPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">New staff user</h1>
        <p className="text-muted-foreground">Roles can be assigned after the user is created.</p>
      </div>
      <RegisterStaffForm />
    </div>
  )
}
