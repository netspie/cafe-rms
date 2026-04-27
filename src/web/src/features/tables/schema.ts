import { z } from "zod"

export const tableSchema = z.object({
  name: z.string().min(1, "Required").max(100, "Max 100 characters"),
})

export type TableFormValues = z.infer<typeof tableSchema>
