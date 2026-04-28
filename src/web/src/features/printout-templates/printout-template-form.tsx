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
import { printoutTemplatesApi, type PrintoutTemplateDetail } from "./api"
import { printoutTemplateSchema, type PrintoutTemplateFormValues } from "./schema"

interface Props { initial?: PrintoutTemplateDetail }

export function PrintoutTemplateForm({ initial }: Props) {
  const router = useRouter()
  const queryClient = useQueryClient()
  const isEdit = !!initial

  const form = useForm<PrintoutTemplateFormValues>({
    resolver: zodResolver(printoutTemplateSchema),
    defaultValues: {
      name: initial?.name ?? "",
      templateFileUrl: initial?.templateFileUrl ?? "",
    },
  })

  const mutation = useMutation({
    mutationFn: async (values: PrintoutTemplateFormValues) => {
      if (isEdit) await printoutTemplatesApi.update(initial!.id, values)
      else await printoutTemplatesApi.create(values)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["printout-templates"] })
      toast.success(isEdit ? "Template updated" : "Template created")
      router.push("/admin/printout-templates")
      router.refresh()
    },
    onError: (err) => {
      if (err instanceof ApiError) toast.error(err.problem.detail ?? err.problem.title ?? err.message)
      else toast.error("Could not save template")
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
              <FormControl><Input {...field} placeholder="Receipt / Invoice / Order ticket" /></FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="templateFileUrl"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Template file URL</FormLabel>
              <FormControl><Input {...field} placeholder="https://… (.docx)" /></FormControl>
              <FormDescription>The Word template Staff can update outside the app.</FormDescription>
              <FormMessage />
            </FormItem>
          )}
        />
        <div className="flex gap-2">
          <Button type="submit" disabled={mutation.isPending}>
            {mutation.isPending ? "Saving…" : isEdit ? "Save changes" : "Create template"}
          </Button>
          <Button type="button" variant="ghost" onClick={() => router.back()}>Cancel</Button>
        </div>
      </form>
    </Form>
  )
}
