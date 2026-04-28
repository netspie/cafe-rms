import { ProductListsTable } from "@/features/product-lists/product-lists-table"

export default function ProductListsListPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Product lists</h1>
        <p className="text-muted-foreground">Curated subsets of products — used to scope an event menu.</p>
      </div>
      <ProductListsTable />
    </div>
  )
}
