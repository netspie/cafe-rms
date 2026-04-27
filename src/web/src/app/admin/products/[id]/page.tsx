"use client"

import { use } from "react"
import { useQuery } from "@tanstack/react-query"
import { Skeleton } from "@/components/ui/skeleton"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { ProductForm } from "@/features/products/product-form"
import { ProductTagsSection } from "@/features/products/product-tags-section"
import { ProductAllergensSection } from "@/features/products/product-allergens-section"
import { ProductModifierGroupsSection } from "@/features/products/product-modifier-groups-section"
import { ProductImagesSection } from "@/features/products/product-images-section"
import { ProductPricesSection } from "@/features/products/product-prices-section"
import { productsApi } from "@/features/products/api"

interface Props {
  params: Promise<{ id: string }>
}

export default function EditProductPage({ params }: Props) {
  const { id } = use(params)
  const query = useQuery({
    queryKey: ["product", id],
    queryFn: () => productsApi.get(id),
  })

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Edit product</h1>
        <p className="text-muted-foreground">
          {query.data ? query.data.name : "Loading…"}
        </p>
      </div>

      {query.isLoading && <Skeleton className="h-64 w-full max-w-3xl" />}

      {query.data && (
        <div className="grid gap-6 lg:grid-cols-2">
          <Card className="lg:col-span-2">
            <CardHeader>
              <CardTitle className="text-base">Basics</CardTitle>
            </CardHeader>
            <CardContent>
              <ProductForm initial={query.data} />
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle className="text-base">Tags</CardTitle>
            </CardHeader>
            <CardContent>
              <ProductTagsSection productId={query.data.id} attachedIds={query.data.tagIds} />
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle className="text-base">Allergens</CardTitle>
            </CardHeader>
            <CardContent>
              <ProductAllergensSection productId={query.data.id} attachedIds={query.data.allergenIds} />
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle className="text-base">Modifier groups</CardTitle>
            </CardHeader>
            <CardContent>
              <ProductModifierGroupsSection
                productId={query.data.id}
                attachedIds={query.data.modifierGroupIds}
              />
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle className="text-base">Images</CardTitle>
            </CardHeader>
            <CardContent>
              <ProductImagesSection productId={query.data.id} images={query.data.images} />
            </CardContent>
          </Card>

          <Card className="lg:col-span-2">
            <CardHeader>
              <CardTitle className="text-base">Prices per price group</CardTitle>
            </CardHeader>
            <CardContent>
              <ProductPricesSection productId={query.data.id} prices={query.data.prices} />
            </CardContent>
          </Card>
        </div>
      )}
    </div>
  )
}
