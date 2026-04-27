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
import { ApiError } from "@/lib/api"
import { companiesApi, type CompanyDetail } from "./api"

const companySchema = z.object({
  legalName: z.string().min(1, "Required").max(200),
  taxId: z.string().min(1, "Required").max(50),
  invoicingAddress: z.string().min(1, "Required").max(500),
  billingEmail: z.string().min(1, "Required").email("Invalid email").max(200),
  billingPhone: z.string().min(1, "Required").max(50),
  isPublic: z.boolean(),
})
type CompanyFormValues = z.infer<typeof companySchema>

interface Props {
  company: CompanyDetail
}

export function CompanyForm({ company }: Props) {
  const queryClient = useQueryClient()

  const form = useForm<CompanyFormValues>({
    resolver: zodResolver(companySchema),
    defaultValues: {
      legalName: company.legalName,
      taxId: company.taxId,
      invoicingAddress: company.invoicingAddress,
      billingEmail: company.billingEmail,
      billingPhone: company.billingPhone,
      isPublic: company.isPublic,
    },
  })

  const mutation = useMutation({
    mutationFn: (values: CompanyFormValues) => companiesApi.update(company.id, values),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["company", company.id] })
      toast.success("Company updated")
    },
    onError: (err) => {
      if (err instanceof ApiError) toast.error(err.problem.detail ?? err.problem.title ?? err.message)
      else toast.error("Could not update company")
    },
  })

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit((v) => mutation.mutate(v))} className="space-y-4">
        <FormField
          control={form.control}
          name="legalName"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Legal name</FormLabel>
              <FormControl><Input {...field} /></FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <div className="grid gap-4 sm:grid-cols-2">
          <FormField
            control={form.control}
            name="taxId"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Tax ID</FormLabel>
                <FormControl><Input {...field} placeholder="NIP" /></FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            control={form.control}
            name="billingPhone"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Billing phone</FormLabel>
                <FormControl><Input {...field} /></FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>
        <FormField
          control={form.control}
          name="billingEmail"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Billing email</FormLabel>
              <FormControl><Input type="email" {...field} /></FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="invoicingAddress"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Invoicing address</FormLabel>
              <FormControl><Input {...field} /></FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="isPublic"
          render={({ field }) => (
            <FormItem>
              <label className="flex cursor-pointer items-center gap-2 text-sm">
                <input
                  type="checkbox"
                  checked={field.value}
                  onChange={(e) => field.onChange(e.target.checked)}
                  className="h-4 w-4"
                />
                <span>Listed publicly in the CafeRMS public directory</span>
              </label>
              <FormDescription>Off for soft-launch, demo, or private cafés.</FormDescription>
            </FormItem>
          )}
        />
        <Button type="submit" disabled={mutation.isPending}>
          {mutation.isPending ? "Saving…" : "Save company"}
        </Button>
      </form>
    </Form>
  )
}
