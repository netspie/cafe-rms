import { PriceGroupsTable } from "@/features/price-groups/price-groups-table"

export default function PriceGroupsListPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Price groups</h1>
        <p className="text-muted-foreground">Named pricing tiers — Standard, Member, Event, etc.</p>
      </div>
      <PriceGroupsTable />
    </div>
  )
}
