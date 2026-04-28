"use client"

import { useRouter } from "next/navigation"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { toast } from "sonner"

import { Button } from "@/components/ui/button"
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage, FormDescription } from "@/components/ui/form"
import { Input } from "@/components/ui/input"
import { NativeSelect } from "@/components/ui/native-select"
import { ApiError } from "@/lib/api"
import { modifierGroupsApi } from "@/features/modifier-groups/api"
import { modifiersApi, type ModifierDetail } from "./api"
import { modifierSchema, type ModifierFormValues } from "./schema"

interface Props { initial?: ModifierDetail }

export function ModifierForm({ initial }: Props) {
  const router = useRouter()
  const queryClient = useQueryClient()
  const isEdit = !!initial

  const groupsQuery = useQuery({
    queryKey: ["modifier-groups", "lookup"],
    queryFn: () => modifierGroupsApi.list({ page: 1, pageSize: 100, sort: "name" }),
  })

  const form = useForm<ModifierFormValues>({
    resolver: zodResolver(modifierSchema),
    defaultValues: {
      modifierGroupId: initial?.modifierGroupId ?? "",
      name: initial?.name ?? "",
      priceDelta: initial?.priceDelta ?? 0,
    },
  })

  const mutation = useMutation({
    mutationFn: async (values: ModifierFormValues) => {
      if (isEdit) {
        await modifiersApi.update(initial!.id, { name: values.name, priceDelta: values.priceDelta })
      } else {
        await modifiersApi.create(values)
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["modifiers"] })
      toast.success(isEdit ? "Modifier updated" : "Modifier created")
      router.push("/admin/modifiers")
      router.refresh()
    },
    onError: (err) => {
      if (err instanceof ApiError) toast.error(err.problem.detail ?? err.problem.title ?? err.message)
      else toast.error("Could not save modifier")
    },
  })

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit((v) => mutation.mutate(v))} className="max-w-xl space-y-4">
        <FormField
          control={form.control}
          name="modifierGroupId"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Modifier group</FormLabel>
              <FormControl>
                <NativeSelect {...field} disabled={isEdit}>
                  <option value="">Pick a group…</option>
                  {groupsQuery.data?.items.map((g) => (
                    <option key={g.id} value={g.id}>{g.name}</option>
                  ))}
                </NativeSelect>
              </FormControl>
              {isEdit && <FormDescription>Group cannot be moved after creation.</FormDescription>}
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="name"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Name</FormLabel>
              <FormControl><Input {...field} placeholder="Oat milk" /></FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="priceDelta"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Price delta</FormLabel>
              <FormControl>
                <Input
                  type="number"
                  step="0.01"
                  name={field.name}
                  ref={field.ref}
                  onBlur={field.onBlur}
                  value={Number.isNaN(field.value) ? "" : field.value}
                  onChange={(e) => field.onChange(e.target.value === "" ? Number.NaN : e.target.valueAsNumber)}
                  className="max-w-32"
                />
              </FormControl>
              <FormDescription>Added to the product price (can be negative).</FormDescription>
              <FormMessage />
            </FormItem>
          )}
        />
        <div className="flex gap-2">
          <Button type="submit" disabled={mutation.isPending}>
            {mutation.isPending ? "Saving…" : isEdit ? "Save changes" : "Create modifier"}
          </Button>
          <Button type="button" variant="ghost" onClick={() => router.back()}>Cancel</Button>
        </div>
      </form>
    </Form>
  )
}
