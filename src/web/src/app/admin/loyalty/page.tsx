import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { LoyaltyEntriesTable } from "@/features/loyalty/loyalty-entries-table"
import { AdjustmentForm } from "@/features/loyalty/adjustment-form"

export default function LoyaltyPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Loyalty</h1>
        <p className="text-muted-foreground">Point activity across customers — and manual adjustments.</p>
      </div>

      <div className="grid gap-6 lg:grid-cols-3">
        <Card className="lg:col-span-2">
          <CardHeader>
            <CardTitle className="text-base">Activity</CardTitle>
            <CardDescription>Latest first. Filter by customer to audit a single account.</CardDescription>
          </CardHeader>
          <CardContent>
            <LoyaltyEntriesTable />
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="text-base">Manual adjustment</CardTitle>
            <CardDescription>Bonus or withdrawal — leaves an entry in the activity log.</CardDescription>
          </CardHeader>
          <CardContent>
            <AdjustmentForm />
          </CardContent>
        </Card>
      </div>
    </div>
  )
}
