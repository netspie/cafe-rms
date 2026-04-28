import { z } from "zod"

export const productListSchema = z.object({
  name: z.string().min(1, "Required").max(200, "Max 200 characters"),
})

export type ProductListFormValues = z.infer<typeof productListSchema>
