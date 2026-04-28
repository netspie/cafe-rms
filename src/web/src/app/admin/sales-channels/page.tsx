import { SalesChannelsTable } from "@/features/sales-channels/sales-channels-table"

export default function SalesChannelsListPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Sales channels</h1>
        <p className="text-muted-foreground">Where the order came from — dine-in, takeout, etc.</p>
      </div>
      <SalesChannelsTable />
    </div>
  )
}
