// Hand-drawn line-art Shiba face. Pure monochrome — uses `currentColor` so it
// picks up whatever color the surrounding text is. Inspired by the kawaii
// merch you'd find at Mame Shiba Cafe in Harajuku.
export function ShibaMark({ className, ...props }: React.SVGProps<SVGSVGElement>) {
  return (
    <svg
      viewBox="0 0 64 64"
      fill="none"
      stroke="currentColor"
      strokeWidth={2}
      strokeLinecap="round"
      strokeLinejoin="round"
      className={`h-6 w-6 ${className ?? ""}`}
      aria-hidden
      {...props}
    >
      {/* outer ears — two pointed triangles up top */}
      <path d="M14 22 L20 6 L28 18" />
      <path d="M50 22 L44 6 L36 18" />
      {/* inner ear marks */}
      <path d="M19 14 L21 11 L23 14" />
      <path d="M45 14 L43 11 L41 14" />
      {/* face — round-ish curve, slightly squared at the muzzle */}
      <path d="M14 22 C14 40 22 52 32 52 C42 52 50 40 50 22" />
      {/* cheek ruffs (small notches near the lower face) */}
      <path d="M16 36 C18 38 20 38 22 36" />
      <path d="M48 36 C46 38 44 38 42 36" />
      {/* eyes — two filled dots */}
      <circle cx="24" cy="30" r="1.6" fill="currentColor" stroke="none" />
      <circle cx="40" cy="30" r="1.6" fill="currentColor" stroke="none" />
      {/* tiny eye sparkle */}
      <circle cx="24.7" cy="29.3" r="0.4" fill="white" stroke="none" />
      <circle cx="40.7" cy="29.3" r="0.4" fill="white" stroke="none" />
      {/* nose */}
      <path d="M30 38 L34 38 L32 41 Z" fill="currentColor" />
      {/* mouth — gentle "w" curve */}
      <path d="M28 44 C30 46 32 46 32 44 C32 46 34 46 36 44" />
    </svg>
  )
}
