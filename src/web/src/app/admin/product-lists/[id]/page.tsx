"use client"

import { use } from "react"
import { useQuery } from "@tanstack/react-query"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Skeleton } from "@/components/ui/skeleton"
import { ProductListForm } from "@/features/product-lists/product-list-form"
import { ProductListItemsSection } from "@/features/product-lists/product-list-items-section"
import { productListsApi } from "@/features/product-lists/api"

interface Props { params: Promise<{ id: string }> }

export default function EditProductListPage({ params }: Props) {
  const { id } = use(params)
  const query = useQuery({ queryKey: ["product-list", id], queryFn: () => productListsApi.get(id) })

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">
          {query.data ? query.data.name : "Loading product list…"}
        </h1>
        <p className="text-muted-foreground">Rename or pick which products belong to this list.</p>
      </div>
      {query.isLoading && <Skeleton className="h-64 w-full max-w-3xl" />}
      {query.data && (
        <div className="grid gap-6 lg:grid-cols-2">
          <Card>
            <CardHeader>
              <CardTitle className="text-base">Basics</CardTitle>
            </CardHeader>
            <CardContent>
              <ProductListForm initial={query.data} />
            </CardContent>
          </Card>
          <Card>
            <CardHeader>
              <CardTitle className="text-base">Products in this list</CardTitle>
            </CardHeader>
            <CardContent>
              <ProductListItemsSection listId={query.data.id} productIds={query.data.productIds} />
            </CardContent>
          </Card>
        </div>
      )}
    </div>
  )
}
