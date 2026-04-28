"use client"

// Role edit page. There's no GetRoleById endpoint — we fetch ListRoles and
// pick the one matching the route param. Permission grid mirrors the C#
// Permissions constants.

import { use, useEffect, useState } from "react"
import { useRouter } from "next/navigation"
import { toast } from "sonner"

import { Field } from "@/components/field"
import { ApiError, api } from "@/lib/api"
import { ALL_PERMISSIONS, PERMISSION_LABELS } from "@/features/auth/permissions"

interface RoleItem { id: string; name: string; permissions: string[] }

export default function EditRolePage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params)
  const router = useRouter()

  const [role, setRole] = useState<RoleItem | null>(null)
  const [name, setName] = useState("")
  const [permissions, setPermissions] = useState<string[]>([])
  const [loading, setLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)
  const [errors, setErrors] = useState<Record<string, string>>({})

  useEffect(() => {
    api.get<RoleItem[]>("/api/roles")
      .then((rs) => {
        const r = rs.find((x) => x.id === id)
        if (r) {
          setRole(r)
          setName(r.name)
          setPermissions(r.permissions)
        }
      })
      .catch((err) => { if (err instanceof ApiError) toast.error(err.message) })
      .finally(() => setLoading(false))
  }, [id])

  function toggle(perm: string) {
    setPermissions((p) => p.includes(perm) ? p.filter((x) => x !== perm) : [...p, perm])
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setSubmitting(true)
    setErrors({})
    try {
      await api.put(`/api/roles/${id}`, { name, permissions })
      toast.success("Role updated")
      router.push("/admin/roles")
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

  if (loading) return <p className="text-sm text-muted-foreground">Loading…</p>
  if (!role) return <p className="text-sm text-muted-foreground">Role not found.</p>

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Edit role</h1>
        <p className="text-muted-foreground">Rename or change which permissions this role grants.</p>
      </div>

      <form onSubmit={handleSubmit} className="max-w-2xl space-y-6">
        <Field label="Name" error={errors.name}>
          <input value={name} onChange={(e) => setName(e.target.value)} required className="h-9 max-w-sm rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>

        <div className="space-y-2">
          <p className="text-sm font-medium">Permissions</p>
          <div className="grid grid-cols-1 gap-1.5 rounded-md border p-3 sm:grid-cols-2">
            {ALL_PERMISSIONS.map((perm) => (
              <label key={perm} className="flex cursor-pointer items-center gap-2 rounded-md px-2 py-1 text-sm hover:bg-accent/50">
                <input type="checkbox" checked={permissions.includes(perm)} onChange={() => toggle(perm)} className="h-4 w-4" />
                <span>{PERMISSION_LABELS[perm]}</span>
              </label>
            ))}
          </div>
        </div>

        <div className="flex gap-2">
          <button type="submit" disabled={submitting} className="h-9 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90 disabled:opacity-50">
            {submitting ? "Saving…" : "Save changes"}
          </button>
          <button type="button" onClick={() => router.push("/admin/roles")} className="h-9 rounded-md px-3 text-sm hover:bg-accent">Cancel</button>
        </div>
      </form>
    </div>
  )
}
