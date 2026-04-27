import { ProductForm } from "@/features/products/product-form"

export default function NewProductPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">New product</h1>
        <p className="text-muted-foreground">Set the basics. Tags, allergens, prices and images come next.</p>
      </div>
      <ProductForm />
    </div>
  )
}
