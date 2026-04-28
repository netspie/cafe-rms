"use client"

import { useRouter } from "next/navigation"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { useMutation, useQueryClient } from "@tanstack/react-query"
import { toast } from "sonner"

import { Button } from "@/components/ui/button"
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from "@/components/ui/form"
import { Input } from "@/components/ui/input"
import { ApiError } from "@/lib/api"
import { productListsApi, type ProductListDetail } from "./api"
import { productListSchema, type ProductListFormValues } from "./schema"

interface Props { initial?: ProductListDetail; onCreated?: (id: string) => void }

export function ProductListForm({ initial, onCreated }: Props) {
  const router = useRouter()
  const queryClient = useQueryClient()
  const isEdit = !!initial

  const form = useForm<ProductListFormValues>({
    resolver: zodResolver(productListSchema),
    defaultValues: { name: initial?.name ?? "" },
  })

  const mutation = useMutation({
    mutationFn: async (values: ProductListFormValues) => {
      if (isEdit) {
        await productListsApi.update(initial!.id, values)
        return initial!.id
      }
      const created = await productListsApi.create(values)
      return created.id
    },
    onSuccess: (id) => {
      queryClient.invalidateQueries({ queryKey: ["product-lists"] })
      queryClient.invalidateQueries({ queryKey: ["product-list", id] })
      toast.success(isEdit ? "Product list updated" : "Product list created")
      if (isEdit) router.refresh()
      else if (onCreated) onCreated(id)
      else router.push(`/admin/product-lists/${id}`)
    },
    onError: (err) => {
      if (err instanceof ApiError) toast.error(err.problem.detail ?? err.problem.title ?? err.message)
      else toast.error("Could not save product list")
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
              <FormControl><Input {...field} placeholder="Brunch menu / Vegan corner" /></FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <div className="flex gap-2">
          <Button type="submit" disabled={mutation.isPending}>
            {mutation.isPending ? "Saving…" : isEdit ? "Save changes" : "Create product list"}
          </Button>
          <Button type="button" variant="ghost" onClick={() => router.back()}>Cancel</Button>
        </div>
      </form>
    </Form>
  )
}
