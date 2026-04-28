// Clickable column header. Toggles between `<field>` (asc) and `-<field>` (desc)
// in the URL ?sort param when clicked, and shows an arrow next to the active
// column. Preserves any other filter params the page wants to keep around (q,
// group, etc.) — page should NOT pass `page` since changing the sort resets
// pagination to 1.

import Link from "next/link"
import { ArrowDown, ArrowUp } from "lucide-react"

interface Props {
  label: string
  field: string
  currentSort: string
  preserve?: Record<string, string>
  className?: string
}

export function SortableTh({ label, field, currentSort, preserve = {}, className = "" }: Props) {
  const isActiveAsc = currentSort === field
  const isActiveDesc = currentSort === `-${field}`
  const nextSort = isActiveAsc ? `-${field}` : field

  return (
    <th className={`px-3 py-2 font-medium ${className}`}>
      <Link
        href={{ query: { ...preserve, sort: nextSort } }}
        className="inline-flex items-center gap-1 hover:text-foreground"
      >
        {label}
        {isActiveAsc && <ArrowUp className="h-3 w-3" />}
        {isActiveDesc && <ArrowDown className="h-3 w-3" />}
      </Link>
    </th>
  )
}
