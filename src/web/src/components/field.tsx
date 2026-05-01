// Single shared form helper: label, child input, error message.
// Errors come from the server's ProblemDetails.errors map (set by ApiError.fieldError).

interface FieldProps {
  label: string
  error?: string
  children: React.ReactNode
}

export function Field({ label, error, children }: FieldProps) {
  return (
    <div className="space-y-1">
      <label className="block text-sm font-medium">{label}</label>
      {children}
      {error && <p className="text-xs text-destructive">{error}</p>}
    </div>
  )
}
