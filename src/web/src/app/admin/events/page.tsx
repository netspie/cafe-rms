import { EventsTable } from "@/features/events/events-table"

export default function EventsListPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Events</h1>
        <p className="text-muted-foreground">Themed evenings, workshops, anything ticketed.</p>
      </div>
      <EventsTable />
    </div>
  )
}
