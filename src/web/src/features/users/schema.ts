import { z } from "zod"

export const registerStaffSchema = z.object({
  email: z.string().min(1, "Required").email("Invalid email").max(200, "Max 200 characters"),
  password: z.string().min(8, "Min 8 characters"),
  firstName: z.string().min(1, "Required").max(100, "Max 100 characters"),
  lastName: z.string().min(1, "Required").max(100, "Max 100 characters"),
})

export type RegisterStaffFormValues = z.infer<typeof registerStaffSchema>

export const updateUserSchema = z.object({
  firstName: z.string().min(1, "Required").max(100, "Max 100 characters"),
  lastName: z.string().min(1, "Required").max(100, "Max 100 characters"),
})

export type UpdateUserFormValues = z.infer<typeof updateUserSchema>
