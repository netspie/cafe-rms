// Login page — server shell + inline server action. The form itself lives in
// login-form.tsx because we need useActionState to surface errors inline. On
// bad credentials the action returns {error}; on success it sets the cookie
// and redirects to /admin (the redirect throws a special signal that Next
// catches — that's not the same as a "real" error, so no dev overlay pops).

import { redirect } from "next/navigation"
import { ShibaMark } from "@/components/shiba-mark"
import { setToken } from "@/lib/auth-cookie"
import { LoginForm, type LoginState } from "./login-form"

const API_BASE = process.env.API_BASE_URL ?? "http://localhost:5179"

async function loginAction(_previousState: LoginState, formData: FormData): Promise<LoginState> {
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
    return { error: body?.detail ?? body?.title ?? "Wrong email or password." }
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

        <LoginForm action={loginAction} />
      </div>
    </div>
  )
}
