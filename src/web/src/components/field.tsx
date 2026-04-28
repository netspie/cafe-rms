// Single shared form helper: label, child input, error message.
// Errors come from the server's ProblemDetails.errors map (set by ApiError.fieldError).

interface FieldProps {
  label: string
  hint?: string
  error?: string
  children: React.ReactNode
}

export function Field({ label, hint, error, children }: FieldProps) {
  return (
    <div className="space-y-1">
      <label className="block text-sm font-medium">{label}</label>
      {children}
      {hint && !error && <p className="text-xs text-muted-foreground">{hint}</p>}
      {error && <p className="text-xs text-destructive">{error}</p>}
    </div>
  )
}
