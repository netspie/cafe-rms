import Link from "next/link"
import { redirect } from "next/navigation"
import { Pencil, Plus, Trash2 } from "lucide-react"
import { revalidatePath } from "next/cache"
import { Field } from "@/components/field"
import { ShibaMark } from "@/components/shiba-mark"
import { api } from "@/lib/server-api"

interface UserItem { id: string; email: string; firstName: string; lastName: string; roles: string[] }

async function registerStaff(formData: FormData) {
  "use server"
  const result = await api.post<{ userId: string }>("/api/auth/register/staff", {
    email: formData.get("email") as string,
    password: formData.get("password") as string,
    firstName: formData.get("firstName") as string,
    lastName: formData.get("lastName") as string,
  })
  revalidatePath("/admin/users")
  redirect(`/admin/users/${result.userId}`)
}

async function deleteUser(id: string) {
  "use server"
  await api.delete(`/api/users/${id}`)
  revalidatePath("/admin/users")
}

export default async function UsersListPage() {
  const users = await api.get<UserItem[]>("/api/users")

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Users</h1>
        <p className="text-muted-foreground">Staff members with admin panel access.</p>
      </div>

      <form action={registerStaff} className="grid gap-3 rounded-md border bg-card p-3 sm:grid-cols-[2fr_1fr_1fr_1fr_auto] sm:items-end">
        <Field label="Email">
          <input name="email" type="email" required placeholder="staff@cafe.example" className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <Field label="Password" hint="Min 8 chars.">
          <input name="password" type="password" required minLength={8} className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <Field label="First name">
          <input name="firstName" required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <Field label="Last name">
          <input name="lastName" required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <button type="submit" className="inline-flex h-9 items-center gap-2 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90">
          <Plus className="h-4 w-4" />Add
        </button>
      </form>

      <div className="overflow-hidden rounded-md border">
        <table className="w-full text-sm">
          <thead className="bg-muted/50 text-left">
            <tr>
              <th className="px-3 py-2 font-medium">Name</th>
              <th className="px-3 py-2 font-medium">Email</th>
              <th className="px-3 py-2 font-medium">Roles</th>
              <th className="w-20 px-3 py-2" />
            </tr>
          </thead>
          <tbody>
            {users.length === 0 && (
              <tr><td colSpan={4} className="px-3 py-12">
                <div className="flex flex-col items-center gap-3 text-muted-foreground">
                  <ShibaMark className="h-10 w-10 opacity-60" />
                  <p>No staff users yet — add the first one above.</p>
                </div>
              </td></tr>
            )}
            {users.map((u) => (
              <tr key={u.id} className="border-t">
                <td className="px-3 py-2 font-medium">{u.firstName} {u.lastName}</td>
                <td className="px-3 py-2 text-muted-foreground">{u.email}</td>
                <td className="px-3 py-2">
                  <div className="flex flex-wrap gap-1">
                    {u.roles.length === 0 && <span className="text-xs text-muted-foreground">—</span>}
                    {u.roles.map((r) => <span key={r} className="inline-flex rounded-full bg-secondary px-2.5 py-0.5 text-xs">{r}</span>)}
                  </div>
                </td>
                <td className="px-3 py-2">
                  <div className="flex justify-end gap-1">
                    <Link href={`/admin/users/${u.id}`} className="inline-flex h-8 w-8 items-center justify-center rounded-md hover:bg-accent" aria-label="Edit"><Pencil className="h-4 w-4" /></Link>
                    <form action={deleteUser.bind(null, u.id)}>
                      <button type="submit" className="inline-flex h-8 w-8 items-center justify-center rounded-md text-destructive hover:bg-destructive/10" aria-label="Delete"><Trash2 className="h-4 w-4" /></button>
                    </form>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}
