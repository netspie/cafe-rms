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
