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
import { ALL_PERMISSIONS, PERMISSION_LABELS } from "@/features/auth/permissions"
import { rolesApi, type RoleListItem } from "./api"
import { roleSchema, type RoleFormValues } from "./schema"

interface Props {
  initial?: RoleListItem
}

export function RoleForm({ initial }: Props) {
  const router = useRouter()
  const queryClient = useQueryClient()
  const isEdit = !!initial

  const form = useForm<RoleFormValues>({
    resolver: zodResolver(roleSchema),
    defaultValues: {
      name: initial?.name ?? "",
      permissions: initial?.permissions ?? [],
    },
  })

  const mutation = useMutation({
    mutationFn: async (values: RoleFormValues) => {
      const body = { name: values.name, permissions: values.permissions }
      if (isEdit) {
        await rolesApi.update(initial!.id, body)
        return initial!.id
      }
      const created = await rolesApi.create(body)
      return created.roleId
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["roles"] })
      toast.success(isEdit ? "Role updated" : "Role created")
      router.push("/admin/roles")
      router.refresh()
    },
    onError: (err) => {
      if (err instanceof ApiError) toast.error(err.problem.detail ?? err.problem.title ?? err.message)
      else toast.error("Could not save role")
    },
  })

  const selected = form.watch("permissions")

  const togglePermission = (perm: string) => {
    const next = selected.includes(perm)
      ? selected.filter((p) => p !== perm)
      : [...selected, perm]
    form.setValue("permissions", next, { shouldDirty: true })
  }

  return (
    <Form {...form}>
      <form
        onSubmit={form.handleSubmit((v) => mutation.mutate(v))}
        className="max-w-2xl space-y-6"
      >
        <FormField
          control={form.control}
          name="name"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Name</FormLabel>
              <FormControl>
                <Input {...field} placeholder="Manager / Barista / Cashier" className="max-w-sm" />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <div className="space-y-3">
          <FormLabel>Permissions</FormLabel>
          <div className="grid grid-cols-1 gap-2 rounded-md border p-4 sm:grid-cols-2">
            {ALL_PERMISSIONS.map((perm) => (
              <label
                key={perm}
                className="flex cursor-pointer items-center gap-2 rounded-md px-2 py-1 text-sm hover:bg-accent/50"
              >
                <input
                  type="checkbox"
                  checked={selected.includes(perm)}
                  onChange={() => togglePermission(perm)}
                  className="h-4 w-4"
                />
                <span>{PERMISSION_LABELS[perm]}</span>
              </label>
            ))}
          </div>
        </div>

        <div className="flex gap-2">
          <Button type="submit" disabled={mutation.isPending}>
            {mutation.isPending ? "Saving…" : isEdit ? "Save changes" : "Create role"}
          </Button>
          <Button type="button" variant="ghost" onClick={() => router.back()}>
            Cancel
          </Button>
        </div>
      </form>
    </Form>
  )
}
