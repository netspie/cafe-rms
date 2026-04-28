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
import { promotionCodesApi, type PromotionCodeDetail } from "./api"
import { promotionCodeSchema, type PromotionCodeFormValues } from "./schema"

interface Props { initial?: PromotionCodeDetail }

const toDateInput = (iso: string | null) => iso ? iso.slice(0, 10) : ""
const fromDateInput = (s: string): string | null => s ? new Date(s).toISOString() : null

export function PromotionCodeForm({ initial }: Props) {
  const router = useRouter()
  const queryClient = useQueryClient()
  const isEdit = !!initial

  const form = useForm<PromotionCodeFormValues>({
    resolver: zodResolver(promotionCodeSchema),
    defaultValues: {
      code: initial?.code ?? "",
      discountPercentage: initial?.discountPercentage ?? 10,
      validFrom: toDateInput(initial?.validFrom ?? null),
      validUntil: toDateInput(initial?.validUntil ?? null),
      maxUses: initial?.maxUses != null ? String(initial.maxUses) : "",
    },
  })

  const mutation = useMutation({
    mutationFn: async (values: PromotionCodeFormValues) => {
      const body = {
        code: values.code,
        discountPercentage: values.discountPercentage,
        validFrom: fromDateInput(values.validFrom),
        validUntil: fromDateInput(values.validUntil),
        maxUses: values.maxUses ? Number(values.maxUses) : null,
      }
      if (isEdit) await promotionCodesApi.update(initial!.id, body)
      else await promotionCodesApi.create(body)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["promotion-codes"] })
      toast.success(isEdit ? "Promotion code updated" : "Promotion code created")
      router.push("/admin/promotion-codes")
      router.refresh()
    },
    onError: (err) => {
      if (err instanceof ApiError) toast.error(err.problem.detail ?? err.problem.title ?? err.message)
      else toast.error("Could not save promotion code")
    },
  })

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit((v) => mutation.mutate(v))} className="max-w-xl space-y-4">
        <FormField
          control={form.control}
          name="code"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Code</FormLabel>
              <FormControl><Input {...field} placeholder="SUMMER20" disabled={isEdit} /></FormControl>
              {isEdit && <FormDescription>Code cannot be changed after creation.</FormDescription>}
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="discountPercentage"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Discount (%)</FormLabel>
              <FormControl>
                <Input
                  type="number" step="0.01" min="0" max="100"
                  name={field.name} ref={field.ref} onBlur={field.onBlur}
                  value={Number.isNaN(field.value) ? "" : field.value}
                  onChange={(e) => field.onChange(e.target.value === "" ? Number.NaN : e.target.valueAsNumber)}
                  className="max-w-32"
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <div className="grid grid-cols-2 gap-4">
          <FormField
            control={form.control}
            name="validFrom"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Valid from</FormLabel>
                <FormControl><Input type="date" {...field} /></FormControl>
                <FormDescription>Optional.</FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            control={form.control}
            name="validUntil"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Valid until</FormLabel>
                <FormControl><Input type="date" {...field} /></FormControl>
                <FormDescription>Optional.</FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>
        <FormField
          control={form.control}
          name="maxUses"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Max uses</FormLabel>
              <FormControl><Input type="number" min="1" step="1" {...field} className="max-w-32" /></FormControl>
              <FormDescription>Empty = unlimited.</FormDescription>
              <FormMessage />
            </FormItem>
          )}
        />
        <div className="flex gap-2">
          <Button type="submit" disabled={mutation.isPending}>
            {mutation.isPending ? "Saving…" : isEdit ? "Save changes" : "Create promotion code"}
          </Button>
          <Button type="button" variant="ghost" onClick={() => router.back()}>Cancel</Button>
        </div>
      </form>
    </Form>
  )
}
