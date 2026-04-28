"use client"

// User edit page. Single file, two inline sections: profile (rename) and
// roles (assign/unassign role from this user).

import { use, useEffect, useState } from "react"
import { useRouter } from "next/navigation"
import { X } from "lucide-react"
import { toast } from "sonner"

import { Field } from "@/components/field"
import { ApiError, api } from "@/lib/api"

interface UserDetail { id: string; email: string; firstName: string; lastName: string; roles: string[] }
interface RoleRow { id: string; name: string; permissions: string[] }

export default function EditUserPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params)
  const router = useRouter()

  const [user, setUser] = useState<UserDetail | null>(null)
  const [roles, setRoles] = useState<RoleRow[]>([])
  const [loading, setLoading] = useState(true)
  const [refreshKey, setRefreshKey] = useState(0)

  useEffect(() => {
    async function load() {
      try {
        const [u, r] = await Promise.all([
          api.get<UserDetail>(`/api/users/${id}`),
          api.get<RoleRow[]>(`/api/roles`),
        ])
        setUser(u)
        setRoles(r)
      } catch (err) {
        if (err instanceof ApiError) toast.error(err.message)
      } finally { setLoading(false) }
    }
    load()
  }, [id, refreshKey])

  if (loading || !user) return <p className="text-sm text-muted-foreground">Loading…</p>

  const refresh = () => setRefreshKey((k) => k + 1)
  const attached = roles.filter((r) => user.roles.includes(r.name))
  const available = roles.filter((r) => !user.roles.includes(r.name))

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">{user.firstName} {user.lastName}</h1>
        <p className="text-muted-foreground">Staff profile and role assignment.</p>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <ProfileSection user={user} onSaved={refresh} />
        <RolesSection userId={user.id} attached={attached} available={available} onChanged={refresh} />
      </div>

      <button type="button" onClick={() => router.push("/admin/users")} className="h-9 rounded-md px-3 text-sm hover:bg-accent">
        Back to list
      </button>
    </div>
  )
}

function ProfileSection({ user, onSaved }: { user: UserDetail; onSaved: () => void }) {
  const [firstName, setFirstName] = useState(user.firstName)
  const [lastName, setLastName] = useState(user.lastName)
  const [submitting, setSubmitting] = useState(false)
  const [errors, setErrors] = useState<Record<string, string>>({})

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setSubmitting(true)
    setErrors({})
    try {
      await api.put(`/api/users/${user.id}`, { firstName, lastName })
      toast.success("User updated")
      onSaved()
    } catch (err) {
      if (err instanceof ApiError) {
        if (err.problem.errors) {
          const flat: Record<string, string> = {}
          for (const [k, v] of Object.entries(err.problem.errors)) flat[k.toLowerCase()] = v[0]
          setErrors(flat)
        } else toast.error(err.message)
      }
    } finally { setSubmitting(false) }
  }

  return (
    <section className="rounded-lg border bg-card p-4">
      <h2 className="mb-4 text-base font-semibold">Profile</h2>
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <p className="text-sm font-medium text-muted-foreground">Email</p>
          <p className="text-sm font-mono">{user.email}</p>
        </div>
        <div className="grid grid-cols-2 gap-3">
          <Field label="First name" error={errors.firstname}>
            <input value={firstName} onChange={(e) => setFirstName(e.target.value)} required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
          </Field>
          <Field label="Last name" error={errors.lastname}>
            <input value={lastName} onChange={(e) => setLastName(e.target.value)} required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
          </Field>
        </div>
        <button type="submit" disabled={submitting} className="h-9 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90 disabled:opacity-50">
          {submitting ? "Saving…" : "Save changes"}
        </button>
      </form>
    </section>
  )
}

function RolesSection({ userId, attached, available, onChanged }: { userId: string; attached: RoleRow[]; available: RoleRow[]; onChanged: () => void }) {
  const [selectedId, setSelectedId] = useState("")

  async function assign() {
    if (!selectedId) return
    try {
      await api.post(`/api/users/${userId}/roles/${selectedId}`)
      toast.success("Role assigned")
      setSelectedId("")
      onChanged()
    } catch (err) {
      if (err instanceof ApiError) toast.error(err.message)
    }
  }

  async function unassign(roleId: string) {
    try {
      await api.delete(`/api/users/${userId}/roles/${roleId}`)
      toast.success("Role removed")
      onChanged()
    } catch (err) {
      if (err instanceof ApiError) toast.error(err.message)
    }
  }

  return (
    <section className="rounded-lg border bg-card p-4">
      <h2 className="mb-4 text-base font-semibold">Roles</h2>
      <div className="space-y-3">
        <div className="flex flex-wrap gap-2">
          {attached.length === 0 && <p className="text-sm text-muted-foreground">No roles assigned.</p>}
          {attached.map((r) => (
            <span key={r.id} className="inline-flex items-center gap-1.5 rounded-full bg-secondary px-2.5 py-0.5 text-xs">
              {r.name}
              <button type="button" onClick={() => unassign(r.id)} className="hover:text-destructive" aria-label={`Remove ${r.name}`}>
                <X className="h-3 w-3" />
              </button>
            </span>
          ))}
        </div>
        <div className="flex gap-2">
          <select value={selectedId} onChange={(e) => setSelectedId(e.target.value)} className="h-9 max-w-xs flex-1 rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30">
            <option value="">Assign role…</option>
            {available.map((r) => <option key={r.id} value={r.id}>{r.name}</option>)}
          </select>
          <button type="button" onClick={assign} disabled={!selectedId} className="h-9 rounded-md border px-3 text-sm hover:bg-accent disabled:opacity-50">Assign</button>
        </div>
      </div>
    </section>
  )
}
