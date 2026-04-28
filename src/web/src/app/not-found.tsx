import Link from "next/link"
import { Button } from "@/components/ui/button"
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
        <Button render={<Link href="/admin" />} nativeButton={false}>Back to admin</Button>
        <Button render={<Link href="/" />} nativeButton={false} variant="outline">Public site</Button>
      </div>
    </div>
  )
}
