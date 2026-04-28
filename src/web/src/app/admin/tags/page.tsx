"use client"

// Tags — list view + inline "Add" dialog.
// Pattern reference for all other admin entity pages on the strip-down branch.
//
// No TanStack Query, no react-hook-form, no zod, no @base-ui — just useState,
// useEffect, raw fetch via the api wrapper, and the server's ProblemDetails for
// per-field validation errors.

import Link from "next/link"
import { useEffect, useState } from "react"
import { Pencil, Plus, Trash2 } from "lucide-react"
import { toast } from "sonner"

import { Field } from "@/components/field"
import { ShibaMark } from "@/components/shiba-mark"
import { ApiError, api, type PagedResult } from "@/lib/api"

interface TagItem {
  id: string
  name: string
  imageUrl: string | null
  createdAt: string
}

const PAGE_SIZE = 20

export default function TagsListPage() {
  const [page, setPage] = useState(1)
  const [filter, setFilter] = useState("")
  const [data, setData] = useState<PagedResult<TagItem> | null>(null)
  const [loading, setLoading] = useState(true)
  const [refreshKey, setRefreshKey] = useState(0)
  const [showAdd, setShowAdd] = useState(false)
  const [confirmDelete, setConfirmDelete] = useState<TagItem | null>(null)

  // Fetch list whenever page / filter / refreshKey changes.
  useEffect(() => {
    setLoading(true)
    const params = new URLSearchParams({ page: String(page), pageSize: String(PAGE_SIZE), sort: "name" })
    if (filter) params.set("name", filter)
    api.get<PagedResult<TagItem>>(`/api/tags?${params}`)
      .then((r) => setData(r))
      .catch((err) => {
        if (err instanceof ApiError) toast.error(err.message)
      })
      .finally(() => setLoading(false))
  }, [page, filter, refreshKey])

  async function handleDelete(tag: TagItem) {
    try {
      await api.delete(`/api/tags/${tag.id}`)
      toast.success("Tag deleted")
      setConfirmDelete(null)
      setRefreshKey((k) => k + 1)
    } catch (err) {
      if (err instanceof ApiError) toast.error(err.message)
    }
  }

  const totalPages = data ? Math.max(1, Math.ceil(data.total / PAGE_SIZE)) : 1

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Tags</h1>
        <p className="text-muted-foreground">Labels shown alongside products.</p>
      </div>

      <div className="flex items-center gap-3">
        <input
          placeholder="Filter by name…"
          value={filter}
          onChange={(e) => { setPage(1); setFilter(e.target.value) }}
          className="h-9 max-w-xs rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30"
        />
        <div className="flex-1" />
        <button
          onClick={() => setShowAdd(true)}
          className="inline-flex h-9 items-center gap-2 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90"
        >
          <Plus className="h-4 w-4" />
          New tag
        </button>
      </div>

      <div className="overflow-hidden rounded-md border">
        <table className="w-full text-sm">
          <thead className="bg-muted/50 text-left">
            <tr>
              <th className="px-3 py-2 font-medium">Name</th>
              <th className="px-3 py-2 font-medium">Image</th>
              <th className="px-3 py-2 font-medium">Created</th>
              <th className="w-20 px-3 py-2" />
            </tr>
          </thead>
          <tbody>
            {loading && (
              <tr>
                <td colSpan={4} className="px-3 py-8 text-center text-muted-foreground">
                  Loading…
                </td>
              </tr>
            )}
            {!loading && data?.items.length === 0 && (
              <tr>
                <td colSpan={4} className="px-3 py-12">
                  <div className="flex flex-col items-center gap-3 text-muted-foreground">
                    <ShibaMark className="h-10 w-10 opacity-60" />
                    <p>No tags yet — let&apos;s make the first one.</p>
                  </div>
                </td>
              </tr>
            )}
            {!loading && data?.items.map((t) => (
              <tr key={t.id} className="border-t">
                <td className="px-3 py-2 font-medium">{t.name}</td>
                <td className="px-3 py-2 text-muted-foreground">
                  {t.imageUrl ? <span className="font-mono text-xs">{t.imageUrl}</span> : "—"}
                </td>
                <td className="px-3 py-2 text-muted-foreground">{new Date(t.createdAt).toLocaleDateString()}</td>
                <td className="px-3 py-2">
                  <div className="flex justify-end gap-1">
                    <Link
                      href={`/admin/tags/${t.id}`}
                      className="inline-flex h-8 w-8 items-center justify-center rounded-md hover:bg-accent"
                      aria-label="Edit"
                    >
                      <Pencil className="h-4 w-4" />
                    </Link>
                    <button
                      onClick={() => setConfirmDelete(t)}
                      className="inline-flex h-8 w-8 items-center justify-center rounded-md text-destructive hover:bg-destructive/10"
                      aria-label="Delete"
                    >
                      <Trash2 className="h-4 w-4" />
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <div className="flex items-center justify-between">
        <p className="text-sm text-muted-foreground">{data ? `${data.total} total` : ""}</p>
        <div className="flex items-center gap-2">
          <button
            onClick={() => setPage((p) => Math.max(1, p - 1))}
            disabled={page === 1 || loading}
            className="h-8 rounded-md border px-3 text-sm hover:bg-accent disabled:opacity-50"
          >
            Previous
          </button>
          <span className="text-sm text-muted-foreground">Page {page} of {totalPages}</span>
          <button
            onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
            disabled={page >= totalPages || loading}
            className="h-8 rounded-md border px-3 text-sm hover:bg-accent disabled:opacity-50"
          >
            Next
          </button>
        </div>
      </div>

      {showAdd && (
        <AddDialog
          onClose={() => setShowAdd(false)}
          onCreated={() => { setShowAdd(false); setRefreshKey((k) => k + 1) }}
        />
      )}

      {confirmDelete && (
        <ConfirmDialog
          title="Delete tag?"
          description={`Removes "${confirmDelete.name}". Products using it will not be deleted.`}
          onConfirm={() => handleDelete(confirmDelete)}
          onCancel={() => setConfirmDelete(null)}
        />
      )}
    </div>
  )
}

function AddDialog({ onClose, onCreated }: { onClose: () => void; onCreated: () => void }) {
  const [name, setName] = useState("")
  const [imageUrl, setImageUrl] = useState("")
  const [submitting, setSubmitting] = useState(false)
  const [errors, setErrors] = useState<Record<string, string>>({})

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setSubmitting(true)
    setErrors({})
    try {
      await api.post("/api/tags", { name, imageUrl: imageUrl || null })
      toast.success("Tag created")
      onCreated()
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

  return (
    <Modal title="New tag" onClose={onClose}>
      <form onSubmit={handleSubmit} className="space-y-4">
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
        <div className="flex justify-end gap-2">
          <button type="button" onClick={onClose} className="h-9 rounded-md px-3 text-sm hover:bg-accent">
            Cancel
          </button>
          <button
            type="submit"
            disabled={submitting}
            className="h-9 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:opacity-90 disabled:opacity-50"
          >
            {submitting ? "Saving…" : "Create tag"}
          </button>
        </div>
      </form>
    </Modal>
  )
}

function ConfirmDialog({ title, description, onConfirm, onCancel }: { title: string; description: string; onConfirm: () => void; onCancel: () => void }) {
  return (
    <Modal title={title} onClose={onCancel}>
      <p className="text-sm text-muted-foreground">{description}</p>
      <div className="mt-6 flex justify-end gap-2">
        <button onClick={onCancel} className="h-9 rounded-md px-3 text-sm hover:bg-accent">Cancel</button>
        <button
          onClick={onConfirm}
          className="h-9 rounded-md bg-destructive px-3 text-sm font-medium text-destructive-foreground hover:opacity-90"
        >
          Delete
        </button>
      </div>
    </Modal>
  )
}

function Modal({ title, onClose, children }: { title: string; onClose: () => void; children: React.ReactNode }) {
  // Plain centered modal — backdrop click closes. No portal needed.
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4" onClick={onClose}>
      <div onClick={(e) => e.stopPropagation()} className="w-full max-w-md rounded-lg border bg-card p-6 shadow-lg">
        <h2 className="text-lg font-semibold">{title}</h2>
        <div className="mt-4">{children}</div>
      </div>
    </div>
  )
}
