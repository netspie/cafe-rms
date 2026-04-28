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
import { modifierGroupsApi, type ModifierGroupDetail } from "./api"
import { modifierGroupSchema, type ModifierGroupFormValues } from "./schema"

interface Props { initial?: ModifierGroupDetail }

export function ModifierGroupForm({ initial }: Props) {
  const router = useRouter()
  const queryClient = useQueryClient()
  const isEdit = !!initial

  const form = useForm<ModifierGroupFormValues>({
    resolver: zodResolver(modifierGroupSchema),
    defaultValues: { name: initial?.name ?? "" },
  })

  const mutation = useMutation({
    mutationFn: async (values: ModifierGroupFormValues) => {
      if (isEdit) await modifierGroupsApi.update(initial!.id, values)
      else await modifierGroupsApi.create(values)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["modifier-groups"] })
      toast.success(isEdit ? "Modifier group updated" : "Modifier group created")
      router.push("/admin/modifier-groups")
      router.refresh()
    },
    onError: (err) => {
      if (err instanceof ApiError) toast.error(err.problem.detail ?? err.problem.title ?? err.message)
      else toast.error("Could not save modifier group")
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
              <FormControl><Input {...field} placeholder="Milk choice" /></FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <div className="flex gap-2">
          <Button type="submit" disabled={mutation.isPending}>
            {mutation.isPending ? "Saving…" : isEdit ? "Save changes" : "Create modifier group"}
          </Button>
          <Button type="button" variant="ghost" onClick={() => router.back()}>Cancel</Button>
        </div>
      </form>
    </Form>
  )
}
