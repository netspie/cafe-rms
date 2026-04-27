import { z } from "zod"

// Mirrors the FluentValidation rules in src/api/.../Features/Tags/Controllers/.
export const tagSchema = z.object({
  name: z.string().min(1, "Required").max(200, "Max 200 characters"),
  imageUrl: z
    .string()
    .max(500, "Max 500 characters")
    .optional()
    .or(z.literal("").transform(() => undefined)),
})

export type TagFormValues = z.infer<typeof tagSchema>
