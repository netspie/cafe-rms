"use client"

import { useRouter } from "next/navigation"
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
import { priceGroupsApi } from "@/features/price-groups/api"
import { productListsApi } from "@/features/product-lists/api"
import { eventsApi, type EventDetail } from "./api"
import { eventSchema, type EventFormValues } from "./schema"

interface Props {
  initial?: EventDetail
}

export function EventForm({ initial }: Props) {
  const router = useRouter()
  const queryClient = useQueryClient()
  const isEdit = !!initial

  const priceGroupsQuery = useQuery({
    queryKey: ["price-groups", "lookup"],
    queryFn: () => priceGroupsApi.list({ page: 1, pageSize: 100, sort: "name" }),
  })
  const productListsQuery = useQuery({
    queryKey: ["product-lists", "lookup"],
    queryFn: () => productListsApi.list({ page: 1, pageSize: 100, sort: "name" }),
  })

  const form = useForm<EventFormValues>({
    resolver: zodResolver(eventSchema),
    defaultValues: {
      name: initial?.name ?? "",
      description: initial?.description ?? "",
      imageUrl: initial?.imageUrl ?? "",
      productListId: initial?.productListId ?? "",
      priceGroupId: initial?.priceGroupId ?? "",
    },
  })

  const mutation = useMutation({
    mutationFn: async (values: EventFormValues) => {
      const body = {
        name: values.name,
        description: values.description || null,
        imageUrl: values.imageUrl || null,
        productListId: values.productListId || null,
        priceGroupId: values.priceGroupId || null,
      }
      if (isEdit) {
        await eventsApi.update(initial!.id, body)
        return initial!.id
      }
      const created = await eventsApi.create(body)
      return created.id
    },
    onSuccess: (id) => {
      queryClient.invalidateQueries({ queryKey: ["events"] })
      queryClient.invalidateQueries({ queryKey: ["event", id] })
      toast.success(isEdit ? "Event updated" : "Event created")
      if (isEdit) router.refresh()
      else router.push(`/admin/events/${id}`)
    },
    onError: (err) => {
      if (err instanceof ApiError) toast.error(err.problem.detail ?? err.problem.title ?? err.message)
      else toast.error("Could not save event")
    },
  })

  return (
    <Form {...form}>
      <form
        onSubmit={form.handleSubmit((v) => mutation.mutate(v))}
        className="max-w-xl space-y-4"
      >
        <FormField
          control={form.control}
          name="name"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Name</FormLabel>
              <FormControl>
                <Input {...field} placeholder="Spring jazz night" />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="description"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Description</FormLabel>
              <FormControl>
                <Input {...field} placeholder="Optional" />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="imageUrl"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Image URL</FormLabel>
              <FormControl>
                <Input {...field} placeholder="https://… (optional)" />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="productListId"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Product list</FormLabel>
              <FormControl>
                <NativeSelect {...field}>
                  <option value="">— No special list —</option>
                  {productListsQuery.data?.items.map((pl) => (
                    <option key={pl.id} value={pl.id}>{pl.name}</option>
                  ))}
                </NativeSelect>
              </FormControl>
              <FormDescription>Optional — restricts the menu shown for this event.</FormDescription>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="priceGroupId"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Price group</FormLabel>
              <FormControl>
                <NativeSelect {...field}>
                  <option value="">— Default pricing —</option>
                  {priceGroupsQuery.data?.items.map((pg) => (
                    <option key={pg.id} value={pg.id}>{pg.name}</option>
                  ))}
                </NativeSelect>
              </FormControl>
              <FormDescription>Optional — overrides default product pricing.</FormDescription>
              <FormMessage />
            </FormItem>
          )}
        />
        <div className="flex gap-2">
          <Button type="submit" disabled={mutation.isPending}>
            {mutation.isPending ? "Saving…" : isEdit ? "Save changes" : "Create event"}
          </Button>
          <Button type="button" variant="ghost" onClick={() => router.back()}>
            Cancel
          </Button>
        </div>
      </form>
    </Form>
  )
}
