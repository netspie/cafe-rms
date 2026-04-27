import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"

export default function AdminDashboard() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Welcome back</h1>
        <p className="text-muted-foreground">Use the sidebar to manage your cafe&apos;s catalog, orders, events, and team.</p>
      </div>
      <Card>
        <CardHeader>
          <CardTitle>Getting started</CardTitle>
          <CardDescription>This is the admin dashboard placeholder.</CardDescription>
        </CardHeader>
        <CardContent className="text-sm text-muted-foreground">
          Pick an entity from the sidebar to start managing your cafe. The Tags page is the first one wired up — the rest are stubs that will be filled in subsequent sessions.
        </CardContent>
      </Card>
    </div>
  )
}
