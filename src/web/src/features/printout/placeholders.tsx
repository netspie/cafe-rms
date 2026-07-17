export interface Placeholder {
  token: string
  description: string
}

export const EVENT_CONFIRMATION_PLACEHOLDERS: Placeholder[] = [
  { token: "{{event_name}}", description: "Event name" },
  { token: "{{event_description}}", description: "Event description" },
  { token: "{{event_status}}", description: "Status (Draft / Published / Closed / Cancelled)" },
  { token: "{{event_days_count}}", description: "Number of event days" },
  { token: "{{event_first_day}}", description: "First day (dd.MM.yyyy)" },
  { token: "{{event_last_day}}", description: "Last day (dd.MM.yyyy)" },
  { token: "{{event_published_at}}", description: "Date the event was published" },
  { token: "{{event_id}}", description: "Event identifier" },
  { token: "{{generated_at}}", description: "Date the document was generated" },
]

export function PlaceholderReference() {
  return (
    <div className="rounded-md border bg-card p-4">
      <h2 className="text-sm font-semibold">Available placeholders</h2>
      <p className="mt-1 text-sm text-muted-foreground">
        Type these tokens into your <span className="font-mono">.docx</span> and they are replaced with the event&apos;s
        data when the confirmation is generated. Paste each token in one go so Word keeps it as a single piece of text.
      </p>
      <ul className="mt-3 divide-y text-sm">
        {EVENT_CONFIRMATION_PLACEHOLDERS.map((p) => (
          <li key={p.token} className="flex items-baseline gap-3 py-1.5">
            <code className="shrink-0 font-mono text-xs">{p.token}</code>
            <span className="text-muted-foreground">{p.description}</span>
          </li>
        ))}
      </ul>
    </div>
  )
}
