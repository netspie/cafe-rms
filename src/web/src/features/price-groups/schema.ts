import { z } from "zod"

export const priceGroupSchema = z.object({
  name: z.string().min(1, "Required").max(200, "Max 200 characters"),
})

export type PriceGroupFormValues = z.infer<typeof priceGroupSchema>
