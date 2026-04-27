import { z } from "zod"

export const eventSchema = z.object({
  name: z.string().min(1, "Required").max(200, "Max 200 characters"),
  description: z.string().max(2000, "Max 2000 characters"),
  imageUrl: z.string().max(500, "Max 500 characters"),
  productListId: z.string(),
  priceGroupId: z.string(),
})

export type EventFormValues = z.infer<typeof eventSchema>

export const addDaySchema = z.object({
  date: z.string().min(1, "Required"),
})

export type AddDayFormValues = z.infer<typeof addDaySchema>
