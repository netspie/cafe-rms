"use client"

import { useActionState } from "react"
import { Field } from "@/components/field"

export interface LoginState {
  error: string | null
}

interface Props {
  action: (previousState: LoginState, formData: FormData) => Promise<LoginState>
}

const initialState: LoginState = { error: null }

export function LoginForm({ action }: Props) {
  const [state, formAction, isPending] = useActionState(action, initialState)

  return (
    <form action={formAction} className="mt-6 space-y-4">
      <Field label="Email">
        <input
          name="email"
          type="email"
          autoComplete="email"
          defaultValue="admin@shiba.pl"
          required
          className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30"
        />
      </Field>
      <Field label="Password">
        <input
          name="password"
          type="password"
          autoComplete="current-password"
          defaultValue="Demo1234"
          required
          className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30"
        />
      </Field>
      {state.error && (
        <p className="text-sm text-destructive" aria-live="polite">
          {state.error}
        </p>
      )}
      <button
        type="submit"
        disabled={isPending}
        className="h-9 w-full rounded-md bg-primary text-sm font-medium text-primary-foreground hover:opacity-90 disabled:opacity-60"
      >
        {isPending ? "Signing in…" : "Sign in"}
      </button>
    </form>
  )
}
