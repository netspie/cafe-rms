import { z } from "zod"

export const taxRateSchema = z.object({
  name: z.string().min(1, "Required").max(100, "Max 100 characters"),
  description: z.string().max(500, "Max 500 characters"),
  rate: z.number({ message: "Must be a number" }).min(0, "Min 0").max(100, "Max 100"),
})

export type TaxRateFormValues = z.infer<typeof taxRateSchema>
