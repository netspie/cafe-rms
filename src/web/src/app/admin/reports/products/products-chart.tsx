"use client"

import {
  Bar,
  BarChart,
  CartesianGrid,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts"

interface ProductRow {
  productId: string
  productName: string
  quantitySold: number
  net: number
  vat: number
  gross: number
}

export function ProductsChart({ products }: { products: ProductRow[] }) {
  if (products.length === 0) {
    return (
      <div className="flex h-64 items-center justify-center text-sm text-muted-foreground">
        No sales in the selected range.
      </div>
    )
  }
  return (
    <div className="h-72 w-full">
      <ResponsiveContainer width="100%" height="100%">
        <BarChart data={products} margin={{ top: 8, right: 16, left: 0, bottom: 40 }}>
          <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" />
          <XAxis
            dataKey="productName"
            stroke="var(--muted-foreground)"
            fontSize={11}
            interval={0}
            angle={-30}
            textAnchor="end"
            height={60}
          />
          <YAxis stroke="var(--muted-foreground)" fontSize={11} />
          <Tooltip
            contentStyle={{ background: "var(--card)", border: "1px solid var(--border)", borderRadius: 6, fontSize: 12 }}
            formatter={(value) => [`${Number(value).toFixed(2)} PLN`, "Gross"]}
          />
          <Bar dataKey="gross" fill="var(--primary)" radius={[2, 2, 0, 0]} />
        </BarChart>
      </ResponsiveContainer>
    </div>
  )
}
