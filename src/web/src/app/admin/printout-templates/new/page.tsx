import { PrintoutTemplateForm } from "@/features/printout-templates/printout-template-form"

export default function NewPrintoutTemplatePage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">New template</h1>
        <p className="text-muted-foreground">Point at a Word file you can swap out later.</p>
      </div>
      <PrintoutTemplateForm />
    </div>
  )
}
