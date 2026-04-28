import { SalesChannelForm } from "@/features/sales-channels/sales-channel-form"

export default function NewSalesChannelPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">New sales channel</h1>
        <p className="text-muted-foreground">Link price groups from the edit page after creating.</p>
      </div>
      <SalesChannelForm />
    </div>
  )
}
