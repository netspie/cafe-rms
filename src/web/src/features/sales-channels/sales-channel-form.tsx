"use client"

import { useRouter } from "next/navigation"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { useMutation, useQueryClient } from "@tanstack/react-query"
import { toast } from "sonner"

import { Button } from "@/components/ui/button"
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage, FormDescription } from "@/components/ui/form"
import { Input } from "@/components/ui/input"
import { ApiError } from "@/lib/api"
import { salesChannelsApi, type SalesChannelDetail } from "./api"
import { salesChannelSchema, type SalesChannelFormValues } from "./schema"

interface Props { initial?: SalesChannelDetail; onCreated?: (id: string) => void }

export function SalesChannelForm({ initial, onCreated }: Props) {
  const router = useRouter()
  const queryClient = useQueryClient()
  const isEdit = !!initial

  const form = useForm<SalesChannelFormValues>({
    resolver: zodResolver(salesChannelSchema),
    defaultValues: {
      name: initial?.name ?? "",
      isTakeout: initial?.isTakeout ?? false,
    },
  })

  const mutation = useMutation({
    mutationFn: async (values: SalesChannelFormValues) => {
      if (isEdit) {
        await salesChannelsApi.update(initial!.id, values)
        return initial!.id
      }
      const created = await salesChannelsApi.create(values)
      return created.id
    },
    onSuccess: (id) => {
      queryClient.invalidateQueries({ queryKey: ["sales-channels"] })
      queryClient.invalidateQueries({ queryKey: ["sales-channel", id] })
      toast.success(isEdit ? "Sales channel updated" : "Sales channel created")
      if (isEdit) router.refresh()
      else if (onCreated) onCreated(id)
      else router.push(`/admin/sales-channels/${id}`)
    },
    onError: (err) => {
      if (err instanceof ApiError) toast.error(err.problem.detail ?? err.problem.title ?? err.message)
      else toast.error("Could not save sales channel")
    },
  })

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit((v) => mutation.mutate(v))} className="max-w-xl space-y-4">
        <FormField
          control={form.control}
          name="name"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Name</FormLabel>
              <FormControl><Input {...field} placeholder="Dine-in / Takeout / Delivery" /></FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="isTakeout"
          render={({ field }) => (
            <FormItem>
              <label className="flex cursor-pointer items-center gap-2 text-sm">
                <input
                  type="checkbox"
                  checked={field.value}
                  onChange={(e) => field.onChange(e.target.checked)}
                  className="h-4 w-4"
                />
                <span>This channel is takeout (order made off-premises)</span>
              </label>
              <FormDescription>Affects tax handling for some jurisdictions.</FormDescription>
            </FormItem>
          )}
        />
        <div className="flex gap-2">
          <Button type="submit" disabled={mutation.isPending}>
            {mutation.isPending ? "Saving…" : isEdit ? "Save changes" : "Create sales channel"}
          </Button>
          <Button type="button" variant="ghost" onClick={() => router.back()}>Cancel</Button>
        </div>
      </form>
    </Form>
  )
}
