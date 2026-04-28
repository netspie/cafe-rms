import Link from "next/link"
import { ShibaMark } from "@/components/shiba-mark"

export default function NotFound() {
  return (
    <div className="flex min-h-dvh flex-col items-center justify-center gap-6 px-6 text-center">
      <ShibaMark className="h-20 w-20 opacity-70" />
      <div className="space-y-2">
        <h1 className="text-3xl font-semibold tracking-tight">404 — that page wandered off</h1>
        <p className="text-muted-foreground">The Shiba sniffed around but couldn&apos;t find what you asked for.</p>
      </div>
      <div className="flex gap-2">
        <Link href="/admin" className="inline-flex h-9 items-center rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90">Back to admin</Link>
        <Link href="/" className="inline-flex h-9 items-center rounded-md border px-3 text-sm hover:bg-accent">Public site</Link>
      </div>
    </div>
  )
}
