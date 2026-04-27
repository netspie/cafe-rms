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
import { usersApi, type UserDetail } from "./api"
import { updateUserSchema, type UpdateUserFormValues } from "./schema"

interface Props {
  user: UserDetail
}

export function UserForm({ user }: Props) {
  const router = useRouter()
  const queryClient = useQueryClient()

  const form = useForm<UpdateUserFormValues>({
    resolver: zodResolver(updateUserSchema),
    defaultValues: { firstName: user.firstName, lastName: user.lastName },
  })

  const mutation = useMutation({
    mutationFn: (values: UpdateUserFormValues) => usersApi.update(user.id, values),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["user", user.id] })
      queryClient.invalidateQueries({ queryKey: ["users"] })
      toast.success("User updated")
      router.refresh()
    },
    onError: (err) => {
      if (err instanceof ApiError) toast.error(err.problem.detail ?? err.problem.title ?? err.message)
      else toast.error("Could not update user")
    },
  })

  return (
    <Form {...form}>
      <form
        onSubmit={form.handleSubmit((v) => mutation.mutate(v))}
        className="max-w-xl space-y-4"
      >
        <div>
          <p className="text-sm font-medium text-muted-foreground">Email</p>
          <p className="text-sm font-mono">{user.email}</p>
        </div>
        <div className="grid grid-cols-2 gap-4">
          <FormField
            control={form.control}
            name="firstName"
            render={({ field }) => (
              <FormItem>
                <FormLabel>First name</FormLabel>
                <FormControl>
                  <Input {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            control={form.control}
            name="lastName"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Last name</FormLabel>
                <FormControl>
                  <Input {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>
        <Button type="submit" disabled={mutation.isPending}>
          {mutation.isPending ? "Saving…" : "Save changes"}
        </Button>
      </form>
    </Form>
  )
}
