"use client"

import { useRouter } from "next/navigation"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
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
} from "@/components/ui/form"
import { Input } from "@/components/ui/input"
import { ApiError } from "@/lib/api"
import { allergensApi, type AllergenDetail } from "./api"
import { allergenSchema, type AllergenFormValues } from "./schema"

interface Props {
  initial?: AllergenDetail
}

export function AllergenForm({ initial }: Props) {
  const router = useRouter()
  const queryClient = useQueryClient()
  const isEdit = !!initial

  const form = useForm<AllergenFormValues>({
    resolver: zodResolver(allergenSchema),
    defaultValues: {
      name: initial?.name ?? "",
    },
  })

  const mutation = useMutation({
    mutationFn: async (values: AllergenFormValues) => {
      if (isEdit) await allergensApi.update(initial!.id, values)
      else await allergensApi.create(values)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["allergens"] })
      toast.success(isEdit ? "Allergen updated" : "Allergen created")
      router.push("/admin/allergens")
      router.refresh()
    },
    onError: (err) => {
      if (err instanceof ApiError) toast.error(err.problem.detail ?? err.problem.title ?? err.message)
      else toast.error("Could not save allergen")
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
                <Input {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <div className="flex gap-2">
          <Button type="submit" disabled={mutation.isPending}>
            {mutation.isPending ? "Saving…" : isEdit ? "Save changes" : "Create allergen"}
          </Button>
          <Button type="button" variant="ghost" onClick={() => router.back()}>
            Cancel
          </Button>
        </div>
      </form>
    </Form>
  )
}
