import { TableForm } from "@/features/tables/table-form"

export default function NewTablePage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">New table</h1>
        <p className="text-muted-foreground">Add a table to your outlet.</p>
      </div>
      <TableForm />
    </div>
  )
}
