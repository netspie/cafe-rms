"use client"

import { use, useEffect, useState } from "react"
import { useRouter } from "next/navigation"
import { toast } from "sonner"

import { Field } from "@/components/field"
import { ApiError, api } from "@/lib/api"

interface TemplateDetail { id: string; name: string; templateFileUrl: string }

export default function EditPrintoutTemplatePage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params)
  const router = useRouter()
  const [name, setName] = useState("")
  const [templateFileUrl, setTemplateFileUrl] = useState("")
  const [loading, setLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)
  const [errors, setErrors] = useState<Record<string, string>>({})

  useEffect(() => {
    api.get<TemplateDetail>(`/api/printout-templates/${id}`)
      .then((r) => { setName(r.name); setTemplateFileUrl(r.templateFileUrl) })
      .catch((err) => { if (err instanceof ApiError) toast.error(err.message) })
      .finally(() => setLoading(false))
  }, [id])

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setSubmitting(true)
    setErrors({})
    try {
      await api.put(`/api/printout-templates/${id}`, { name, templateFileUrl })
      toast.success("Template updated")
      router.push("/admin/printout-templates")
    } catch (err) {
      if (err instanceof ApiError) {
        if (err.problem.errors) {
          const flat: Record<string, string> = {}
          for (const [k, v] of Object.entries(err.problem.errors)) flat[k.toLowerCase()] = v[0]
          setErrors(flat)
        } else toast.error(err.message)
      }
    } finally { setSubmitting(false) }
  }

  if (loading) return <p className="text-sm text-muted-foreground">Loading…</p>

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Edit template</h1>
        <p className="text-muted-foreground">Update name or file URL.</p>
      </div>
      <form onSubmit={handleSubmit} className="max-w-xl space-y-4">
        <Field label="Name" error={errors.name}>
          <input value={name} onChange={(e) => setName(e.target.value)} required className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <Field label="Template file URL" error={errors.templatefileurl}>
          <input value={templateFileUrl} onChange={(e) => setTemplateFileUrl(e.target.value)} required placeholder="https://… (.docx)" className="h-9 w-full rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30" />
        </Field>
        <div className="flex gap-2">
          <button type="submit" disabled={submitting} className="h-9 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90 disabled:opacity-50">{submitting ? "Saving…" : "Save changes"}</button>
          <button type="button" onClick={() => router.back()} className="h-9 rounded-md px-3 text-sm hover:bg-accent">Cancel</button>
        </div>
      </form>
    </div>
  )
}
