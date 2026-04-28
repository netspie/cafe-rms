// Stores the JWT in localStorage. Defense answer: a production deployment would
// hold this in an httpOnly cookie to dodge XSS exfiltration; for the academic
// scope we accept the risk to keep the auth flow easy to read end to end.

const TOKEN_KEY = "caferms.auth.token"
const ACCOUNT_TYPE_KEY = "caferms.auth.accountType"

export function getToken(): string | null {
  if (typeof window === "undefined") return null
  return window.localStorage.getItem(TOKEN_KEY)
}

export function setSession(token: string, accountType: string) {
  window.localStorage.setItem(TOKEN_KEY, token)
  window.localStorage.setItem(ACCOUNT_TYPE_KEY, accountType)
}

export function clearSession() {
  window.localStorage.removeItem(TOKEN_KEY)
  window.localStorage.removeItem(ACCOUNT_TYPE_KEY)
}

export function redirectToLogin() {
  if (typeof window === "undefined") return
  const here = window.location.pathname + window.location.search
  window.location.href = `/login?redirect=${encodeURIComponent(here)}`
}
