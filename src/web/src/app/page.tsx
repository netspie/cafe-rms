import { redirect } from "next/navigation"
import { getToken } from "@/lib/auth-cookie"

export default async function HomePage() {
  const token = await getToken()
  redirect(token ? "/admin" : "/login")
}
