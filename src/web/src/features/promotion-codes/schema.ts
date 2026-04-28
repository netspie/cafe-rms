import { z } from "zod"

export const promotionCodeSchema = z.object({
  code: z.string().min(1, "Required").max(50, "Max 50 characters"),
  discountPercentage: z.number({ message: "Must be a number" }).min(0, "Min 0").max(100, "Max 100"),
  validFrom: z.string(),
  validUntil: z.string(),
  maxUses: z.string(),
})

export type PromotionCodeFormValues = z.infer<typeof promotionCodeSchema>
