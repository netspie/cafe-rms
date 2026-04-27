"use client"

import { use } from "react"
import { useQuery } from "@tanstack/react-query"

import { Badge } from "@/components/ui/badge"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Skeleton } from "@/components/ui/skeleton"
import { EventForm } from "@/features/events/event-form"
import { EventDaysSection } from "@/features/events/event-days-section"
import { EventActions } from "@/features/events/event-actions"
import { eventsApi, type EventStatus } from "@/features/events/api"

interface Props {
  params: Promise<{ id: string }>
}

function statusVariant(status: EventStatus): "default" | "secondary" | "destructive" | "outline" {
  if (status === "Published") return "default"
  if (status === "Cancelled") return "destructive"
  if (status === "Closed") return "secondary"
  return "outline"
}

export default function EditEventPage({ params }: Props) {
  const { id } = use(params)
  const query = useQuery({
    queryKey: ["event", id],
    queryFn: () => eventsApi.get(id),
  })

  return (
    <div className="space-y-6">
      <div className="flex items-start justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">
            {query.data ? query.data.name : "Loading event…"}
          </h1>
          {query.data && (
            <div className="mt-2 flex items-center gap-3">
              <Badge variant={statusVariant(query.data.status)}>{query.data.status}</Badge>
              {query.data.publishedAt && (
                <span className="text-sm text-muted-foreground">
                  Published {new Date(query.data.publishedAt).toLocaleString()}
                </span>
              )}
            </div>
          )}
        </div>
        {query.data && <EventActions event={query.data} />}
      </div>

      {query.isLoading && <Skeleton className="h-64 w-full max-w-3xl" />}

      {query.data && (
        <div className="grid gap-6 lg:grid-cols-2">
          <Card className="lg:col-span-2">
            <CardHeader>
              <CardTitle className="text-base">Basics</CardTitle>
            </CardHeader>
            <CardContent>
              <EventForm initial={query.data} />
            </CardContent>
          </Card>

          <Card className="lg:col-span-2">
            <CardHeader>
              <CardTitle className="text-base">Days</CardTitle>
            </CardHeader>
            <CardContent>
              <EventDaysSection
                eventId={query.data.id}
                days={query.data.days}
                disabled={query.data.status === "Closed" || query.data.status === "Cancelled"}
              />
            </CardContent>
          </Card>

          {query.data.cancellationReason && (
            <Card className="lg:col-span-2">
              <CardHeader>
                <CardTitle className="text-base">Cancellation reason</CardTitle>
              </CardHeader>
              <CardContent>
                <p className="text-sm text-muted-foreground">{query.data.cancellationReason}</p>
              </CardContent>
            </Card>
          )}
        </div>
      )}
    </div>
  )
}
