import { z } from "zod"

export const productSchema = z.object({
  name: z.string().min(1, "Required").max(200, "Max 200 characters"),
  description: z.string().max(1000, "Max 1000 characters"),
  barcode: z.string().max(100, "Max 100 characters"),
  taxRateId: z.string().min(1, "Tax rate is required"),
})

export type ProductFormValues = z.infer<typeof productSchema>

export const addImageSchema = z.object({
  url: z.string().min(1, "Required").max(500, "Max 500 characters"),
})

export type AddImageFormValues = z.infer<typeof addImageSchema>

export const setPriceSchema = z.object({
  net: z.number({ message: "Must be a number" }).min(0, "Min 0"),
})

export type SetPriceFormValues = z.infer<typeof setPriceSchema>
