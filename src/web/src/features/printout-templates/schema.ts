import { z } from "zod"

export const printoutTemplateSchema = z.object({
  name: z.string().min(1, "Required").max(200, "Max 200 characters"),
  templateFileUrl: z.string().min(1, "Required").max(500, "Max 500 characters"),
})

export type PrintoutTemplateFormValues = z.infer<typeof printoutTemplateSchema>
