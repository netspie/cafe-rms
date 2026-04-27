"use client"

import { useState } from "react"
import { useMutation, useQueryClient } from "@tanstack/react-query"
import { Trash2 } from "lucide-react"
import { toast } from "sonner"

import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { ApiError } from "@/lib/api"
import { eventsApi, type EventDay } from "./api"

interface Props {
  eventId: string
  days: EventDay[]
  disabled?: boolean
}

export function EventDaysSection({ eventId, days, disabled }: Props) {
  const queryClient = useQueryClient()
  const [date, setDate] = useState("")

  const refresh = () => queryClient.invalidateQueries({ queryKey: ["event", eventId] })

  const addMutation = useMutation({
    mutationFn: (d: string) => eventsApi.addDay(eventId, d),
    onSuccess: () => {
      toast.success("Day added")
      setDate("")
      refresh()
    },
    onError: (err) => toast.error(err instanceof ApiError ? err.message : "Could not add day"),
  })

  const removeMutation = useMutation({
    mutationFn: (dayId: string) => eventsApi.removeDay(eventId, dayId),
    onSuccess: () => {
      toast.success("Day removed")
      refresh()
    },
    onError: (err) => toast.error(err instanceof ApiError ? err.message : "Could not remove day"),
  })

  return (
    <div className="space-y-3">
      <div className="space-y-2">
        {days.length === 0 && <p className="text-sm text-muted-foreground">No days scheduled yet.</p>}
        {days.map((d) => (
          <div key={d.id} className="flex items-center gap-3 rounded-md border px-3 py-2">
            <span className="flex-1 text-sm font-medium">
              {new Date(d.date).toLocaleDateString(undefined, {
                weekday: "long",
                year: "numeric",
                month: "long",
                day: "numeric",
              })}
            </span>
            <Button
              type="button"
              variant="ghost"
              size="icon"
              className="h-8 w-8"
              onClick={() => removeMutation.mutate(d.id)}
              disabled={disabled || removeMutation.isPending}
              aria-label="Remove day"
            >
              <Trash2 className="h-4 w-4" />
            </Button>
          </div>
        ))}
      </div>
      {!disabled && (
        <div className="flex gap-2">
          <Input
            type="date"
            value={date}
            onChange={(e) => setDate(e.target.value)}
            className="max-w-xs"
          />
          <Button
            type="button"
            variant="outline"
            size="sm"
            disabled={!date || addMutation.isPending}
            onClick={() => addMutation.mutate(date)}
          >
            Add day
          </Button>
        </div>
      )}
    </div>
  )
}
