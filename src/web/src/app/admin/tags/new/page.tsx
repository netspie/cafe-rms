import { TagForm } from "@/features/tags/tag-form"

export default function NewTagPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">New tag</h1>
        <p className="text-muted-foreground">Create a new tag for your product catalog.</p>
      </div>
      <TagForm />
    </div>
  )
}
