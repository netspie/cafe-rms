"use client"

import { useState } from "react"
import { useMutation, useQueryClient } from "@tanstack/react-query"
import { toast } from "sonner"

import { Button } from "@/components/ui/button"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog"
import { Input } from "@/components/ui/input"
import { ApiError } from "@/lib/api"
import { eventsApi, type EventDetail } from "./api"

interface Props {
  event: EventDetail
}

export function EventActions({ event }: Props) {
  const queryClient = useQueryClient()
  const [confirmCancel, setConfirmCancel] = useState(false)
  const [reason, setReason] = useState("")

  const refresh = () => queryClient.invalidateQueries({ queryKey: ["event", event.id] })

  const publishMutation = useMutation({
    mutationFn: () => eventsApi.publish(event.id),
    onSuccess: () => {
      toast.success("Event published")
      refresh()
    },
    onError: (err) => toast.error(err instanceof ApiError ? err.message : "Could not publish event"),
  })

  const closeMutation = useMutation({
    mutationFn: () => eventsApi.close(event.id),
    onSuccess: () => {
      toast.success("Event closed")
      refresh()
    },
    onError: (err) => toast.error(err instanceof ApiError ? err.message : "Could not close event"),
  })

  const cancelMutation = useMutation({
    mutationFn: () => eventsApi.cancel(event.id, reason || null),
    onSuccess: () => {
      toast.success("Event cancelled")
      setConfirmCancel(false)
      setReason("")
      refresh()
    },
    onError: (err) => toast.error(err instanceof ApiError ? err.message : "Could not cancel event"),
  })

  const isDraft = event.status === "Draft"
  const isPublished = event.status === "Published"
  const isClosable = isPublished
  const isCancellable = isDraft || isPublished
  const isPublishable = isDraft && event.days.length > 0

  return (
    <>
      <div className="flex flex-wrap gap-2">
        {isDraft && (
          <Button
            variant="default"
            disabled={!isPublishable || publishMutation.isPending}
            onClick={() => publishMutation.mutate()}
          >
            {publishMutation.isPending ? "Publishing…" : "Publish"}
          </Button>
        )}
        {isClosable && (
          <Button
            variant="default"
            disabled={closeMutation.isPending}
            onClick={() => closeMutation.mutate()}
          >
            {closeMutation.isPending ? "Closing…" : "Close event"}
          </Button>
        )}
        {isCancellable && (
          <Button variant="destructive" onClick={() => setConfirmCancel(true)}>
            Cancel event
          </Button>
        )}
        {isDraft && !isPublishable && (
          <p className="self-center text-sm text-muted-foreground">
            Add at least one day before publishing.
          </p>
        )}
      </div>

      <Dialog open={confirmCancel} onOpenChange={setConfirmCancel}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Cancel this event?</DialogTitle>
            <DialogDescription>
              Optional reason — appears on the event record afterwards.
            </DialogDescription>
          </DialogHeader>
          <Input
            placeholder="Reason (optional)"
            value={reason}
            onChange={(e) => setReason(e.target.value)}
          />
          <DialogFooter>
            <Button variant="ghost" onClick={() => setConfirmCancel(false)}>Keep event</Button>
            <Button
              variant="destructive"
              onClick={() => cancelMutation.mutate()}
              disabled={cancelMutation.isPending}
            >
              {cancelMutation.isPending ? "Cancelling…" : "Cancel event"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  )
}
