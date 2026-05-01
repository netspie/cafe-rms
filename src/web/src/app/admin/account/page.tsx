import { revalidatePath } from "next/cache"
import { Field } from "@/components/field"
import { api } from "@/lib/server-api"

interface MeResult {
  firstName: string
  lastName: string
  email: string
  accountType: string
  roles: string[]
}

async function changePassword(formData: FormData) {
  "use server"
  const newPassword = formData.get("newPassword") as string
  const confirm = formData.get("confirm") as string
  if (newPassword !== confirm) throw new Error("Passwords do not match.")
  await api.put("/api/auth/password", {
    currentPassword: formData.get("currentPassword") as string,
    newPassword,
  })
  revalidatePath("/admin/account")
}

export default async function AccountPage() {
  const me = await api.get<MeResult>("/api/me")

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Account</h1>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <section className="rounded-lg border bg-card p-4">
          <h2 className="mb-4 text-base font-semibold">Profile</h2>
          <div className="space-y-2 text-sm">
            <div><span className="text-muted-foreground">Name: </span>{me.firstName} {me.lastName}</div>
            <div><span className="text-muted-foreground">Email: </span>{me.email}</div>
            <div><span className="text-muted-foreground">Account type: </span>{me.accountType}</div>
            <div className="flex flex-wrap items-center gap-2">
              <span className="text-muted-foreground">Roles:</span>
              {me.roles.length === 0 && <span>—</span>}
              {me.roles.map((r) => <span key={r} className="inline-flex rounded-full bg-secondary px-2.5 py-0.5 text-xs">{r}</span>)}
            </div>
          </div>
        </section>

        <section className="rounded-lg border bg-card p-4">
          <h2 className="mb-4 text-base font-semibold">Change password</h2>
          <form action={changePassword} className="space-y-4">
            <Field label="Current password">
              <input name="currentPassword" type="password" required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
            </Field>
            <Field label="New password">
              <input name="newPassword" type="password" required minLength={8} className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
            </Field>
            <Field label="Confirm new password">
              <input name="confirm" type="password" required minLength={8} className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
            </Field>
            <button type="submit" className="h-9 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90">Change password</button>
          </form>
        </section>
      </div>
    </div>
  )
}
