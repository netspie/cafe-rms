"use client"

import { useState } from "react"
import { useMutation, useQueryClient } from "@tanstack/react-query"
import { Trash2 } from "lucide-react"
import { toast } from "sonner"

import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { ApiError } from "@/lib/api"
import { productsApi, type ProductImage } from "./api"

interface Props {
  productId: string
  images: ProductImage[]
}

export function ProductImagesSection({ productId, images }: Props) {
  const queryClient = useQueryClient()
  const [url, setUrl] = useState("")

  const refresh = () => queryClient.invalidateQueries({ queryKey: ["product", productId] })

  const addMutation = useMutation({
    mutationFn: (imageUrl: string) => productsApi.addImage(productId, imageUrl),
    onSuccess: () => {
      toast.success("Image added")
      setUrl("")
      refresh()
    },
    onError: (err) => toast.error(err instanceof ApiError ? err.message : "Could not add image"),
  })

  const removeMutation = useMutation({
    mutationFn: (imageId: string) => productsApi.removeImage(productId, imageId),
    onSuccess: () => {
      toast.success("Image removed")
      refresh()
    },
    onError: (err) => toast.error(err instanceof ApiError ? err.message : "Could not remove image"),
  })

  return (
    <div className="space-y-3">
      <div className="space-y-2">
        {images.length === 0 && <p className="text-sm text-muted-foreground">No images yet.</p>}
        {images.map((img) => (
          <div key={img.id} className="flex items-center gap-3 rounded-md border px-3 py-2">
            <span className="flex-1 truncate text-sm font-mono text-muted-foreground">{img.url}</span>
            <Button
              type="button"
              variant="ghost"
              size="icon"
              className="h-8 w-8"
              onClick={() => removeMutation.mutate(img.id)}
              disabled={removeMutation.isPending}
              aria-label="Remove image"
            >
              <Trash2 className="h-4 w-4" />
            </Button>
          </div>
        ))}
      </div>
      <div className="flex gap-2">
        <Input
          placeholder="https://…"
          value={url}
          onChange={(e) => setUrl(e.target.value)}
          className="max-w-md"
        />
        <Button
          type="button"
          variant="outline"
          size="sm"
          disabled={!url || addMutation.isPending}
          onClick={() => addMutation.mutate(url)}
        >
          Add image
        </Button>
      </div>
    </div>
  )
}
