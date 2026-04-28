import { z } from "zod"

export const modifierGroupSchema = z.object({
  name: z.string().min(1, "Required").max(200, "Max 200 characters"),
})

export type ModifierGroupFormValues = z.infer<typeof modifierGroupSchema>
