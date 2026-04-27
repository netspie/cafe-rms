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
import { taxRatesApi } from "@/features/tax-rates/api"
import { productsApi, type ProductDetail } from "./api"
import { productSchema, type ProductFormValues } from "./schema"

interface Props {
  initial?: ProductDetail
  onCreated?: (id: string) => void
}

export function ProductForm({ initial, onCreated }: Props) {
  const router = useRouter()
  const queryClient = useQueryClient()
  const isEdit = !!initial

  const taxRatesQuery = useQuery({
    queryKey: ["tax-rates", "lookup"],
    queryFn: () => taxRatesApi.list({ page: 1, pageSize: 100, sort: "name" }),
  })

  const form = useForm<ProductFormValues>({
    resolver: zodResolver(productSchema),
    defaultValues: {
      name: initial?.name ?? "",
      description: initial?.description ?? "",
      barcode: initial?.barcode ?? "",
      taxRateId: initial?.taxRateId ?? "",
    },
  })

  const mutation = useMutation({
    mutationFn: async (values: ProductFormValues) => {
      const body = {
        name: values.name,
        description: values.description || null,
        barcode: values.barcode || null,
        taxRateId: values.taxRateId,
      }
      if (isEdit) {
        await productsApi.update(initial!.id, body)
        return initial!.id
      }
      const created = await productsApi.create(body)
      return created.id
    },
    onSuccess: (id) => {
      queryClient.invalidateQueries({ queryKey: ["products"] })
      queryClient.invalidateQueries({ queryKey: ["product", id] })
      toast.success(isEdit ? "Product updated" : "Product created")
      if (isEdit) router.refresh()
      else if (onCreated) onCreated(id)
      else router.push(`/admin/products/${id}`)
    },
    onError: (err) => {
      if (err instanceof ApiError) toast.error(err.problem.detail ?? err.problem.title ?? err.message)
      else toast.error("Could not save product")
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
                <Input {...field} placeholder="Latte" />
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
          name="barcode"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Barcode</FormLabel>
              <FormControl>
                <Input {...field} placeholder="Optional, must be unique" />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="taxRateId"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Tax rate</FormLabel>
              <FormControl>
                <NativeSelect {...field}>
                  <option value="">Select a tax rate…</option>
                  {taxRatesQuery.data?.items.map((tr) => (
                    <option key={tr.id} value={tr.id}>
                      {tr.name} ({tr.rate}%)
                    </option>
                  ))}
                </NativeSelect>
              </FormControl>
              <FormDescription>Pick which VAT rate applies to this product.</FormDescription>
              <FormMessage />
            </FormItem>
          )}
        />
        <div className="flex gap-2">
          <Button type="submit" disabled={mutation.isPending}>
            {mutation.isPending ? "Saving…" : isEdit ? "Save changes" : "Create product"}
          </Button>
          <Button type="button" variant="ghost" onClick={() => router.back()}>
            Cancel
          </Button>
        </div>
      </form>
    </Form>
  )
}
