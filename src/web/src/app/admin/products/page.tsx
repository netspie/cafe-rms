import { ProductsTable } from "@/features/products/products-table"

export default function ProductsListPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Products</h1>
        <p className="text-muted-foreground">Items sold across the menu.</p>
      </div>
      <ProductsTable />
    </div>
  )
}
