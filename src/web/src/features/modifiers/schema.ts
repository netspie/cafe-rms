import { z } from "zod"

export const modifierSchema = z.object({
  modifierGroupId: z.string().min(1, "Group is required"),
  name: z.string().min(1, "Required").max(200, "Max 200 characters"),
  priceDelta: z.number({ message: "Must be a number" }),
})

export type ModifierFormValues = z.infer<typeof modifierSchema>
