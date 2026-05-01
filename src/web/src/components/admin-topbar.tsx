// Topbar — server component. Fetches /api/me to know whether to show the
// SuperAdmin company switcher. For SuperAdmins also fetches /api/companies
// (gated server-side by RequireSuperAdmin policy) and renders a <select>
// whose form action writes the picked id to the company cookie.
//
// Both /api/me and /api/companies are caught — if the token expired or the
// API is down, the topbar still renders without the switcher rather than
// blowing up the whole admin layout.
//
// Sign-out is a tiny <form> whose action is a server function: clears both
// cookies (token + company) and redirects to /login.

import { LogOut } from "lucide-react"
import { redirect } from "next/navigation"
import { revalidatePath } from "next/cache"
import { clearToken, getCompanyId, setCompanyId } from "@/lib/auth-cookie"
import { api } from "@/lib/server-api"

interface Me {
  email: string
  firstName: string
  lastName: string
  accountType: string
  companyId: string | null
  roles: string[]
}

interface CompanyItem {
  id: string
  legalName: string
}

async function logoutAction() {
  "use server"
  await clearToken()
  redirect("/login")
}

async function switchCompanyAction(formData: FormData) {
  "use server"
  const id = (formData.get("companyId") as string | null) ?? ""
  if (!id) return
  await setCompanyId(id)
  revalidatePath("/admin", "layout")
}

export async function AdminTopbar() {
  const me = await api.get<Me>("/api/me").catch(() => null)
  const isSuperAdmin = me?.accountType === "SuperAdmin"
  const selectedCompanyId = isSuperAdmin ? await getCompanyId() : null
  const companies = isSuperAdmin
    ? await api.get<CompanyItem[]>("/api/companies").catch(() => [])
    : []

  return (
    <header className="sticky top-0 z-40 flex h-14 items-center gap-3 border-b bg-background px-4">
      {isSuperAdmin && (
        <form action={switchCompanyAction} className="flex items-center gap-2">
          <span className="text-xs uppercase tracking-wider text-muted-foreground">Company</span>
          <select
            name="companyId"
            // key changes when the cookie changes — React reuses the DOM
            // <select> across server-action re-renders, which means
            // defaultValue alone wouldn't update the visible choice.
            key={selectedCompanyId ?? "none"}
            defaultValue={selectedCompanyId ?? ""}
            className="h-9 max-w-xs rounded-md border bg-transparent px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/30"
          >
            <option value="">— pick a company —</option>
            {companies.map((c) => (
              <option key={c.id} value={c.id}>{c.legalName}</option>
            ))}
          </select>
          <button type="submit" className="h-9 rounded-md border px-3 text-sm hover:bg-accent">
            Switch
          </button>
        </form>
      )}
      <div className="flex-1" />
      {me && (
        <div className="flex items-center gap-2 text-sm">
          <span className="font-medium">
            {me.firstName || me.lastName ? `${me.firstName} ${me.lastName}`.trim() : me.email}
          </span>
          <span className="text-muted-foreground">·</span>
          <span className="text-xs uppercase tracking-wider text-muted-foreground">
            {me.accountType === "SuperAdmin"
              ? "SuperAdmin"
              : me.roles.length > 0 ? me.roles.join(", ") : me.accountType}
          </span>
        </div>
      )}
      <form action={logoutAction}>
        <button type="submit" className="inline-flex items-center gap-2 rounded-md px-3 py-1.5 text-sm hover:bg-accent">
          <LogOut className="h-4 w-4" />
          Sign out
        </button>
      </form>
    </header>
  )
}
