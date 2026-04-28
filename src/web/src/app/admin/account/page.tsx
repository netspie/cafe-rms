"use client"

import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { useMutation, useQuery } from "@tanstack/react-query"
import { toast } from "sonner"

import { Button } from "@/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage, FormDescription } from "@/components/ui/form"
import { Input } from "@/components/ui/input"
import { Skeleton } from "@/components/ui/skeleton"
import { Badge } from "@/components/ui/badge"
import { ApiError } from "@/lib/api"
import { meApi } from "@/features/me/api"

const passwordSchema = z.object({
  currentPassword: z.string().min(1, "Required"),
  newPassword: z.string().min(8, "Min 8 characters"),
  confirm: z.string().min(8, "Min 8 characters"),
}).refine((d) => d.newPassword === d.confirm, {
  message: "Passwords do not match",
  path: ["confirm"],
})
type PasswordFormValues = z.infer<typeof passwordSchema>

export default function AccountPage() {
  const meQuery = useQuery({ queryKey: ["me"], queryFn: () => meApi.get() })

  const form = useForm<PasswordFormValues>({
    resolver: zodResolver(passwordSchema),
    defaultValues: { currentPassword: "", newPassword: "", confirm: "" },
  })

  const mutation = useMutation({
    mutationFn: (values: PasswordFormValues) =>
      meApi.changePassword({ currentPassword: values.currentPassword, newPassword: values.newPassword }),
    onSuccess: () => {
      toast.success("Password changed")
      form.reset({ currentPassword: "", newPassword: "", confirm: "" })
    },
    onError: (err) => {
      if (err instanceof ApiError) toast.error(err.problem.detail ?? err.problem.title ?? err.message)
      else toast.error("Could not change password")
    },
  })

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Account</h1>
        <p className="text-muted-foreground">Your profile and password.</p>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Profile</CardTitle>
            <CardDescription>Read-only — names are managed from the Users page.</CardDescription>
          </CardHeader>
          <CardContent className="space-y-2 text-sm">
            {meQuery.isLoading && <Skeleton className="h-24 w-full" />}
            {meQuery.data && (
              <>
                <div><span className="text-muted-foreground">Name: </span>{meQuery.data.firstName} {meQuery.data.lastName}</div>
                <div><span className="text-muted-foreground">Email: </span>{meQuery.data.email}</div>
                <div><span className="text-muted-foreground">Account type: </span>{meQuery.data.accountType}</div>
                <div className="flex flex-wrap items-center gap-2">
                  <span className="text-muted-foreground">Roles:</span>
                  {meQuery.data.roles.length === 0 && <span>—</span>}
                  {meQuery.data.roles.map((r) => <Badge key={r} variant="secondary">{r}</Badge>)}
                </div>
              </>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="text-base">Change password</CardTitle>
            <CardDescription>At least 8 characters.</CardDescription>
          </CardHeader>
          <CardContent>
            <Form {...form}>
              <form onSubmit={form.handleSubmit((v) => mutation.mutate(v))} className="space-y-4">
                <FormField
                  control={form.control}
                  name="currentPassword"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Current password</FormLabel>
                      <FormControl><Input type="password" {...field} /></FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="newPassword"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>New password</FormLabel>
                      <FormControl><Input type="password" {...field} /></FormControl>
                      <FormDescription>Min 8 characters.</FormDescription>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="confirm"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Confirm new password</FormLabel>
                      <FormControl><Input type="password" {...field} /></FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <Button type="submit" disabled={mutation.isPending}>
                  {mutation.isPending ? "Saving…" : "Change password"}
                </Button>
              </form>
            </Form>
          </CardContent>
        </Card>
      </div>
    </div>
  )
}
