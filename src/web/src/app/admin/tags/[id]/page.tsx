"use client"

// Tags — edit page. Loads the tag, lets the user rename / change the image
// URL, posts back. Uses raw fetch + useState; server validates and we surface
// per-field errors via the ApiError.fieldError map.

import { use, useEffect, useState } from "react"
import { useRouter } from "next/navigation"
import { toast } from "sonner"

import { Field } from "@/components/field"
import { ApiError, api } from "@/lib/api"

interface TagDetail {
  id: string
  name: string
  imageUrl: string | null
  createdAt: string
  updatedAt: string | null
}

export default function EditTagPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params)
  const router = useRouter()

  const [name, setName] = useState("")
  const [imageUrl, setImageUrl] = useState("")
  const [loading, setLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)
  const [errors, setErrors] = useState<Record<string, string>>({})

  useEffect(() => {
    api.get<TagDetail>(`/api/tags/${id}`)
      .then((r) => {
        setName(r.name)
        setImageUrl(r.imageUrl ?? "")
      })
      .catch((err) => {
        if (err instanceof ApiError) toast.error(err.message)
      })
      .finally(() => setLoading(false))
  }, [id])

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setSubmitting(true)
    setErrors({})
    try {
      await api.put(`/api/tags/${id}`, { name, imageUrl: imageUrl || null })
      toast.success("Tag updated")
      router.push("/admin/tags")
    } catch (err) {
      if (err instanceof ApiError) {
        if (err.problem.errors) {
          const flat: Record<string, string> = {}
          for (const [k, v] of Object.entries(err.problem.errors)) flat[k.toLowerCase()] = v[0]
          setErrors(flat)
        } else {
          toast.error(err.message)
        }
      }
    } finally {
      setSubmitting(false)
    }
  }

  if (loading) return <p className="text-sm text-muted-foreground">Loading…</p>

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Edit tag</h1>
        <p className="text-muted-foreground">Update name or image URL.</p>
      </div>

      <form onSubmit={handleSubmit} className="max-w-xl space-y-4">
        <Field label="Name" error={errors.name}>
          <input
            value={name}
            onChange={(e) => setName(e.target.value)}
            required
            className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30"
          />
        </Field>
        <Field label="Image URL" hint="Optional. Used for badges in the UI." error={errors.imageurl}>
          <input
            value={imageUrl}
            onChange={(e) => setImageUrl(e.target.value)}
            placeholder="https://…"
            className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30"
          />
        </Field>
        <div className="flex gap-2">
          <button
            type="submit"
            disabled={submitting}
            className="h-9 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90 disabled:opacity-50"
          >
            {submitting ? "Saving…" : "Save changes"}
          </button>
          <button type="button" onClick={() => router.back()} className="h-9 rounded-md px-3 text-sm hover:bg-accent">
            Cancel
          </button>
        </div>
      </form>
    </div>
  )
}
