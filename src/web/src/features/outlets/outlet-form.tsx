"use client"

import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { useMutation, useQueryClient } from "@tanstack/react-query"
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
import { ALL_CURRENCIES, outletsApi, type Currency, type OutletDetail } from "./api"

const outletSchema = z.object({
  displayName: z.string().min(1, "Required").max(200),
  streetAddress: z.string().min(1, "Required").max(500),
  phone: z.string().min(1, "Required").max(50),
  timeZone: z.string().min(1, "Required").max(100),
  currency: z.enum(ALL_CURRENCIES as [Currency, ...Currency[]]),
  logoUrl: z.string().max(500),
})
type OutletFormValues = z.infer<typeof outletSchema>

interface Props {
  outlet: OutletDetail
}

export function OutletForm({ outlet }: Props) {
  const queryClient = useQueryClient()

  const form = useForm<OutletFormValues>({
    resolver: zodResolver(outletSchema),
    defaultValues: {
      displayName: outlet.displayName,
      streetAddress: outlet.streetAddress,
      phone: outlet.phone,
      timeZone: outlet.timeZone,
      currency: outlet.currency as Currency,
      logoUrl: outlet.logoUrl ?? "",
    },
  })

  const mutation = useMutation({
    mutationFn: (values: OutletFormValues) =>
      outletsApi.update(outlet.id, { ...values, logoUrl: values.logoUrl || null }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["outlet", outlet.id] })
      toast.success("Outlet updated")
    },
    onError: (err) => {
      if (err instanceof ApiError) toast.error(err.problem.detail ?? err.problem.title ?? err.message)
      else toast.error("Could not update outlet")
    },
  })

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit((v) => mutation.mutate(v))} className="space-y-4">
        <FormField
          control={form.control}
          name="displayName"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Display name</FormLabel>
              <FormControl><Input {...field} /></FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="streetAddress"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Street address</FormLabel>
              <FormControl><Input {...field} /></FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <div className="grid gap-4 sm:grid-cols-2">
          <FormField
            control={form.control}
            name="phone"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Phone</FormLabel>
                <FormControl><Input {...field} /></FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            control={form.control}
            name="timeZone"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Time zone</FormLabel>
                <FormControl><Input {...field} placeholder="Europe/Warsaw" /></FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>
        <FormField
          control={form.control}
          name="currency"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Currency</FormLabel>
              <FormControl>
                <NativeSelect {...field} className="max-w-xs">
                  {ALL_CURRENCIES.map((c) => (
                    <option key={c} value={c}>{c}</option>
                  ))}
                </NativeSelect>
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="logoUrl"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Logo URL</FormLabel>
              <FormControl><Input {...field} placeholder="https://… (optional)" /></FormControl>
              <FormDescription>Shown on receipts and the public listing.</FormDescription>
              <FormMessage />
            </FormItem>
          )}
        />
        <Button type="submit" disabled={mutation.isPending}>
          {mutation.isPending ? "Saving…" : "Save outlet"}
        </Button>
      </form>
    </Form>
  )
}
