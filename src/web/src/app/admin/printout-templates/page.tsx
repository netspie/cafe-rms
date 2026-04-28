import { PrintoutTemplatesTable } from "@/features/printout-templates/printout-templates-table"

export default function PrintoutTemplatesListPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Printout templates</h1>
        <p className="text-muted-foreground">Word documents used to render receipts and invoices.</p>
      </div>
      <PrintoutTemplatesTable />
    </div>
  )
}
