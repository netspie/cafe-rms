import { z } from "zod"

export const roleSchema = z.object({
  name: z.string().min(1, "Required").max(100, "Max 100 characters"),
  permissions: z.array(z.string()),
})

export type RoleFormValues = z.infer<typeof roleSchema>
