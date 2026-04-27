import { z } from "zod"

export const allergenSchema = z.object({
  name: z.string().min(1, "Required").max(200, "Max 200 characters"),
})

export type AllergenFormValues = z.infer<typeof allergenSchema>
