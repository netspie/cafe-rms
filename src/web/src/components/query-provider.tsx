"use client"

// Transitional shim. Untranslated pages still call useQuery; this keeps them
// alive while S2–S5 chip away at them. Translated pages don't touch the client.
// Delete this file (and drop @tanstack/react-query) in S6.

import { QueryClient, QueryClientProvider } from "@tanstack/react-query"
import { useState } from "react"

export function QueryProvider({ children }: { children: React.ReactNode }) {
  const [client] = useState(() => new QueryClient())
  return <QueryClientProvider client={client}>{children}</QueryClientProvider>
}
