"use client"

// Account — profile readout + change-password form. Single file.

import { useEffect, useState } from "react"
import { toast } from "sonner"

import { Field } from "@/components/field"
import { ApiError, api } from "@/lib/api"

interface MeResult {
  firstName: string
  lastName: string
  email: string
  accountType: string
  roles: string[]
}

export default function AccountPage() {
  const [me, setMe] = useState<MeResult | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    api.get<MeResult>("/api/me")
      .then(setMe)
      .catch((err) => { if (err instanceof ApiError) toast.error(err.message) })
      .finally(() => setLoading(false))
  }, [])

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Account</h1>
        <p className="text-muted-foreground">Your profile and password.</p>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <ProfileSection me={me} loading={loading} />
        <PasswordSection />
      </div>
    </div>
  )
}

function ProfileSection({ me, loading }: { me: MeResult | null; loading: boolean }) {
  return (
    <section className="rounded-lg border bg-card p-4">
      <h2 className="text-base font-semibold">Profile</h2>
      <p className="mb-4 text-xs text-muted-foreground">Read-only — names are managed from the Users page.</p>
      {loading && <p className="text-sm text-muted-foreground">Loading…</p>}
      {me && (
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
      )}
    </section>
  )
}

function PasswordSection() {
  const [currentPassword, setCurrentPassword] = useState("")
  const [newPassword, setNewPassword] = useState("")
  const [confirm, setConfirm] = useState("")
  const [submitting, setSubmitting] = useState(false)
  const [errors, setErrors] = useState<Record<string, string>>({})
  const [mismatch, setMismatch] = useState<string | undefined>()

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setMismatch(undefined)
    if (newPassword !== confirm) {
      setMismatch("Passwords do not match")
      return
    }
    setSubmitting(true)
    setErrors({})
    try {
      await api.put("/api/auth/password", { currentPassword, newPassword })
      toast.success("Password changed")
      setCurrentPassword("")
      setNewPassword("")
      setConfirm("")
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
      <h2 className="text-base font-semibold">Change password</h2>
      <p className="mb-4 text-xs text-muted-foreground">At least 8 characters.</p>
      <form onSubmit={handleSubmit} className="space-y-4">
        <Field label="Current password" error={errors.currentpassword}>
          <input type="password" value={currentPassword} onChange={(e) => setCurrentPassword(e.target.value)} required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <Field label="New password" hint="Min 8 characters." error={errors.newpassword}>
          <input type="password" value={newPassword} onChange={(e) => setNewPassword(e.target.value)} required minLength={8} className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <Field label="Confirm new password" error={mismatch}>
          <input type="password" value={confirm} onChange={(e) => setConfirm(e.target.value)} required minLength={8} className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <button type="submit" disabled={submitting} className="h-9 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90 disabled:opacity-50">
          {submitting ? "Saving…" : "Change password"}
        </button>
      </form>
    </section>
  )
}
