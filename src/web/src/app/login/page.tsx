"use client"

import { useRouter, useSearchParams } from "next/navigation"
import { useEffect, useState, Suspense } from "react"
import { toast } from "sonner"

import { ShibaMark } from "@/components/shiba-mark"
import { Field } from "@/components/field"
import { ApiError, api } from "@/lib/api"
import { getToken, setSession } from "@/lib/auth"

interface LoginResult {
  accessToken: string
  expiresAt: string
  accountType: string
}

function LoginInner() {
  const router = useRouter()
  const searchParams = useSearchParams()
  const [email, setEmail] = useState("")
  const [password, setPassword] = useState("")
  const [submitting, setSubmitting] = useState(false)
  const [errors, setErrors] = useState<Record<string, string>>({})

  // If already signed in, skip the form.
  useEffect(() => {
    if (getToken()) router.replace(searchParams.get("redirect") ?? "/admin")
  }, [router, searchParams])

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setSubmitting(true)
    setErrors({})
    try {
      const result = await api.post<LoginResult>("/api/auth/login", { email, password })
      setSession(result.accessToken, result.accountType)
      router.replace(searchParams.get("redirect") ?? "/admin")
    } catch (err) {
      if (err instanceof ApiError) {
        if (err.problem.errors) {
          const flat: Record<string, string> = {}
          for (const [k, v] of Object.entries(err.problem.errors)) flat[k.toLowerCase()] = v[0]
          setErrors(flat)
        } else {
          toast.error(err.problem.detail ?? err.problem.title ?? err.message)
        }
      } else {
        toast.error("Network error — could not reach the server")
      }
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="flex min-h-dvh items-center justify-center p-4">
      <div className="w-full max-w-sm rounded-lg border bg-card p-6 shadow-sm">
        <div className="flex flex-col items-center gap-2 text-center">
          <ShibaMark className="h-12 w-12" />
          <h1 className="text-lg font-semibold">CafeRMS Admin</h1>
          <p className="text-sm text-muted-foreground">Sign in to manage your cafe.</p>
          <p className="text-[10px] uppercase tracking-[0.2em] text-muted-foreground/80">
            よろしく ・ welcome back
          </p>
        </div>

        <form onSubmit={handleSubmit} className="mt-6 space-y-4">
          <Field label="Email" error={errors.email}>
            <input
              type="email"
              autoComplete="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30"
            />
          </Field>
          <Field label="Password" error={errors.password}>
            <input
              type="password"
              autoComplete="current-password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30"
            />
          </Field>
          <button
            type="submit"
            disabled={submitting}
            className="h-9 w-full rounded-md bg-primary text-sm font-medium text-primary-foreground hover:opacity-90 disabled:opacity-50"
          >
            {submitting ? "Signing in…" : "Sign in"}
          </button>
        </form>
      </div>
    </div>
  )
}

export default function LoginPage() {
  return (
    <Suspense fallback={null}>
      <LoginInner />
    </Suspense>
  )
}
