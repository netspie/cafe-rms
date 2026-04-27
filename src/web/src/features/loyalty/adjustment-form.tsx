"use client"

import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { toast } from "sonner"

import { Button } from "@/components/ui/button"
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
  FormDescription,
} from "@/components/ui/form"
import { Input } from "@/components/ui/input"
import { NativeSelect } from "@/components/ui/native-select"
import { ApiError } from "@/lib/api"
import { usersApi } from "@/features/users/api"
import { loyaltyApi } from "./api"
import { adjustmentSchema, type AdjustmentFormValues } from "./schema"

export function AdjustmentForm() {
  const queryClient = useQueryClient()

  const usersQuery = useQuery({
    queryKey: ["users"],
    queryFn: () => usersApi.list(),
  })

  const form = useForm<AdjustmentFormValues>({
    resolver: zodResolver(adjustmentSchema),
    defaultValues: { userId: "", points: 0, reason: "" },
  })

  const mutation = useMutation({
    mutationFn: (values: AdjustmentFormValues) => loyaltyApi.addAdjustment(values),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["loyalty-entries"] })
      toast.success("Adjustment recorded")
      form.reset({ userId: "", points: 0, reason: "" })
    },
    onError: (err) => {
      if (err instanceof ApiError) toast.error(err.problem.detail ?? err.problem.title ?? err.message)
      else toast.error("Could not record adjustment")
    },
  })

  return (
    <Form {...form}>
      <form
        onSubmit={form.handleSubmit((v) => mutation.mutate(v))}
        className="space-y-4"
      >
        <FormField
          control={form.control}
          name="userId"
          render={({ field }) => (
            <FormItem>
              <FormLabel>User</FormLabel>
              <FormControl>
                <NativeSelect {...field}>
                  <option value="">Pick a user…</option>
                  {usersQuery.data?.map((u) => (
                    <option key={u.id} value={u.id}>
                      {u.firstName} {u.lastName} — {u.email}
                    </option>
                  ))}
                </NativeSelect>
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="points"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Points</FormLabel>
              <FormControl>
                <Input
                  type="number"
                  step="1"
                  name={field.name}
                  ref={field.ref}
                  onBlur={field.onBlur}
                  value={Number.isNaN(field.value) ? "" : field.value}
                  onChange={(e) => field.onChange(e.target.value === "" ? Number.NaN : e.target.valueAsNumber)}
                  className="max-w-32"
                />
              </FormControl>
              <FormDescription>Positive for a bonus, negative to withdraw.</FormDescription>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="reason"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Reason</FormLabel>
              <FormControl>
                <Input {...field} placeholder="e.g. complaint resolution, birthday gift" />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <Button type="submit" disabled={mutation.isPending}>
          {mutation.isPending ? "Saving…" : "Record adjustment"}
        </Button>
      </form>
    </Form>
  )
}
