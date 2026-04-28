// Login — pure server component. The form posts to a server action that
// authenticates against the C# API, sets an httpOnly cookie, and redirects.
// On bad credentials the action throws — Next.js shows the error.tsx
// boundary; the user clicks back to retry. UX is barebones, deliberately.

import { redirect } from "next/navigation"
import { Field } from "@/components/field"
import { ShibaMark } from "@/components/shiba-mark"
import { setToken } from "@/lib/auth-cookie"

const API_BASE = process.env.API_BASE_URL ?? "http://localhost:5179"

async function loginAction(formData: FormData) {
  "use server"
  const email = formData.get("email") as string
  const password = formData.get("password") as string

  const response = await fetch(`${API_BASE}/api/auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email, password }),
    cache: "no-store",
  })

  if (!response.ok) {
    const body = await response.json().catch(() => null)
    throw new Error(body?.detail ?? body?.title ?? "Wrong email or password.")
  }

  const result = await response.json() as { accessToken: string; expiresAt: string }
  await setToken(result.accessToken, new Date(result.expiresAt))
  redirect("/admin")
}

export default function LoginPage() {
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

        <form action={loginAction} className="mt-6 space-y-4">
          <Field label="Email">
            <input name="email" type="email" autoComplete="email" required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
          </Field>
          <Field label="Password">
            <input name="password" type="password" autoComplete="current-password" required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
          </Field>
          <button type="submit" className="h-9 w-full rounded-md bg-primary text-sm font-medium text-primary-foreground hover:opacity-90">
            Sign in
          </button>
        </form>
      </div>
    </div>
  )
}
