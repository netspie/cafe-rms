import { EventForm } from "@/features/events/event-form"

export default function NewEventPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">New event</h1>
        <p className="text-muted-foreground">Set the basics. You can add days and publish from the edit page.</p>
      </div>
      <EventForm />
    </div>
  )
}
