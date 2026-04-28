import { z } from "zod"

export const salesChannelSchema = z.object({
  name: z.string().min(1, "Required").max(200, "Max 200 characters"),
  isTakeout: z.boolean(),
})

export type SalesChannelFormValues = z.infer<typeof salesChannelSchema>
