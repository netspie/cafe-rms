import { z } from "zod"

export const adjustmentSchema = z.object({
  userId: z.string().min(1, "Pick a user"),
  points: z.number({ message: "Must be a number" }).int("Must be a whole number").refine((n) => n !== 0, "Cannot be zero"),
  reason: z.string().min(1, "Required").max(500, "Max 500 characters"),
})

export type AdjustmentFormValues = z.infer<typeof adjustmentSchema>
