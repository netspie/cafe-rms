"use client"

import Link from "next/link"
import { useEffect, useState } from "react"
import { Pencil, Plus, Trash2 } from "lucide-react"
import { toast } from "sonner"

import { Field } from "@/components/field"
import { ShibaMark } from "@/components/shiba-mark"
import { ApiError, api } from "@/lib/api"
import { ALL_PERMISSIONS, PERMISSION_LABELS } from "@/features/auth/permissions"

interface RoleItem { id: string; name: string; permissions: string[] }

export default function RolesListPage() {
  const [roles, setRoles] = useState<RoleItem[] | null>(null)
  const [loading, setLoading] = useState(true)
  const [refreshKey, setRefreshKey] = useState(0)
  const [showAdd, setShowAdd] = useState(false)
  const [confirmDelete, setConfirmDelete] = useState<RoleItem | null>(null)

  useEffect(() => {
    setLoading(true)
    api.get<RoleItem[]>("/api/roles")
      .then(setRoles)
      .catch((err) => { if (err instanceof ApiError) toast.error(err.message) })
      .finally(() => setLoading(false))
  }, [refreshKey])

  async function handleDelete(item: RoleItem) {
    try {
      await api.delete(`/api/roles/${item.id}`)
      toast.success("Role deleted")
      setConfirmDelete(null)
      setRefreshKey((k) => k + 1)
    } catch (err) {
      if (err instanceof ApiError) toast.error(err.message)
    }
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Roles</h1>
        <p className="text-muted-foreground">Permission groups assigned to staff users.</p>
      </div>

      <div className="flex items-center gap-3">
        <div className="flex-1" />
        <button onClick={() => setShowAdd(true)} className="inline-flex h-9 items-center gap-2 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90">
          <Plus className="h-4 w-4" />New role
        </button>
      </div>

      <div className="overflow-hidden rounded-md border">
        <table className="w-full text-sm">
          <thead className="bg-muted/50 text-left">
            <tr>
              <th className="px-3 py-2 font-medium">Name</th>
              <th className="px-3 py-2 font-medium">Permissions</th>
              <th className="w-20 px-3 py-2" />
            </tr>
          </thead>
          <tbody>
            {loading && <tr><td colSpan={3} className="px-3 py-8 text-center text-muted-foreground">Loading…</td></tr>}
            {!loading && roles?.length === 0 && (
              <tr><td colSpan={3} className="px-3 py-12">
                <div className="flex flex-col items-center gap-3 text-muted-foreground">
                  <ShibaMark className="h-10 w-10 opacity-60" />
                  <p>No roles yet — define one to start grouping permissions.</p>
                </div>
              </td></tr>
            )}
            {!loading && roles?.map((r) => (
              <tr key={r.id} className="border-t">
                <td className="px-3 py-2 font-medium">{r.name}</td>
                <td className="px-3 py-2">
                  <div className="flex flex-wrap gap-1">
                    {r.permissions.length === 0 && <span className="text-xs text-muted-foreground">—</span>}
                    {r.permissions.map((p) => <span key={p} className="inline-flex rounded-full border px-2 py-0.5 font-mono text-xs">{p}</span>)}
                  </div>
                </td>
                <td className="px-3 py-2">
                  <div className="flex justify-end gap-1">
                    <Link href={`/admin/roles/${r.id}`} className="inline-flex h-8 w-8 items-center justify-center rounded-md hover:bg-accent" aria-label="Edit"><Pencil className="h-4 w-4" /></Link>
                    <button onClick={() => setConfirmDelete(r)} className="inline-flex h-8 w-8 items-center justify-center rounded-md text-destructive hover:bg-destructive/10" aria-label="Delete"><Trash2 className="h-4 w-4" /></button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {showAdd && <AddDialog onClose={() => setShowAdd(false)} onCreated={() => { setShowAdd(false); setRefreshKey((k) => k + 1) }} />}
      {confirmDelete && (
        <Modal title="Delete role?" onClose={() => setConfirmDelete(null)}>
          <p className="text-sm text-muted-foreground">Removes &ldquo;{confirmDelete.name}&rdquo;. Users assigned this role will lose its permissions.</p>
          <div className="mt-6 flex justify-end gap-2">
            <button onClick={() => setConfirmDelete(null)} className="h-9 rounded-md px-3 text-sm hover:bg-accent">Cancel</button>
            <button onClick={() => handleDelete(confirmDelete)} className="h-9 rounded-md bg-destructive px-3 text-sm font-medium text-destructive-foreground hover:opacity-90">Delete</button>
          </div>
        </Modal>
      )}
    </div>
  )
}

function AddDialog({ onClose, onCreated }: { onClose: () => void; onCreated: () => void }) {
  const [name, setName] = useState("")
  const [permissions, setPermissions] = useState<string[]>([])
  const [submitting, setSubmitting] = useState(false)
  const [errors, setErrors] = useState<Record<string, string>>({})

  function toggle(perm: string) {
    setPermissions((p) => p.includes(perm) ? p.filter((x) => x !== perm) : [...p, perm])
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setSubmitting(true)
    setErrors({})
    try {
      await api.post("/api/roles", { name, permissions })
      toast.success("Role created")
      onCreated()
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
    <Modal title="New role" onClose={onClose}>
      <form onSubmit={handleSubmit} className="space-y-4">
        <Field label="Name" error={errors.name}>
          <input value={name} onChange={(e) => setName(e.target.value)} required placeholder="Manager / Barista / Cashier" className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <div className="space-y-2">
          <p className="text-sm font-medium">Permissions</p>
          <div className="grid grid-cols-1 gap-1.5 rounded-md border p-3 sm:grid-cols-2 max-h-72 overflow-y-auto">
            {ALL_PERMISSIONS.map((perm) => (
              <label key={perm} className="flex cursor-pointer items-center gap-2 rounded-md px-2 py-1 text-sm hover:bg-accent/50">
                <input type="checkbox" checked={permissions.includes(perm)} onChange={() => toggle(perm)} className="h-4 w-4" />
                <span>{PERMISSION_LABELS[perm]}</span>
              </label>
            ))}
          </div>
        </div>
        <div className="flex justify-end gap-2">
          <button type="button" onClick={onClose} className="h-9 rounded-md px-3 text-sm hover:bg-accent">Cancel</button>
          <button type="submit" disabled={submitting} className="h-9 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90 disabled:opacity-50">{submitting ? "Saving…" : "Create role"}</button>
        </div>
      </form>
    </Modal>
  )
}

function Modal({ title, onClose, children }: { title: string; onClose: () => void; children: React.ReactNode }) {
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4" onClick={onClose}>
      <div onClick={(e) => e.stopPropagation()} className="w-full max-w-lg rounded-lg border bg-card p-6 shadow-lg">
        <h2 className="text-lg font-semibold">{title}</h2>
        <div className="mt-4">{children}</div>
      </div>
    </div>
  )
}
