# Frontend admin panel — session plan

Targets the API at `src/api/` (270 backend tests green; all endpoints shipped). Lives at `src/web/`.

## Stack (decided)

- **Framework:** Next.js 15 (App Router, TypeScript)
- **UI:** shadcn/ui + Tailwind CSS (source-in-repo — easy to defend in thesis)
- **Server state:** TanStack Query
- **Tables:** TanStack Table v8 + shadcn `<DataTable>` recipe
- **Forms:** react-hook-form + zod (zod schemas mirror FluentValidation rules from the API)
- **API client:** hand-written fetch wrapper (no codegen; explainable)
- **Auth:** JWT in httpOnly cookie set by Next.js Route Handler; client never touches the token
- **i18n:** English-only for now (skipping Polish UI to keep scope down — flag if thesis needs PL)
- **Charts (later, for reports phase):** Recharts

## Hard requirements covered here

- **Min 50 professional UI views** — every CRUD entity gets a list + add + edit page (3 views), plus dashboard, login, settings, etc. ~25 entities × 3 = 75 views, comfortably over the bar.
- **All visible content managed from the admin panel** — every business entity has CRUD here.
- **Auth + permission group management via UI** — Roles + Users + assign-roles pages.

---

## Sessions

### F1 — Foundation (done)

- [x] Bootstrap `src/web/` via `create-next-app` (TypeScript, Tailwind, App Router, src dir, alias `@/*`).
- [x] `shadcn init` + baseline components — settled on the **base-nova** style (default in current shadcn CLI), which uses `@base-ui/react` underneath instead of Radix. **Important difference:** components use a `render` prop (e.g. `<Button render={<Link href="…" />}>`) instead of `asChild`. Same composability, different API.
- [x] Install + configure: `@tanstack/react-query`, `@tanstack/react-table`, `react-hook-form`, `zod`, `@hookform/resolvers`.
- [x] **API client** (`src/lib/api.ts`) — calls land at `/api/proxy/<path>` (a Route Handler that forwards to the C# API with the JWT attached); throws `ApiError` with parsed `ProblemDetails`.
- [x] **Auth flow:** `/login` page → `POST /api/auth/login` Route Handler sets httpOnly `auth-token` cookie + a non-httpOnly `account-type` cookie. **Next.js 16 breaking change**: `middleware.ts` is now `proxy.ts` and the function name is `proxy(...)` — file renamed accordingly. Logout clears both cookies.
- [x] **Layout shell** (`app/admin/layout.tsx`) — shadcn `Sidebar` (with `SidebarProvider` + `SidebarInset`) + topbar with sign-out. Sidebar groups: Catalog / Pricing & sales / Operations / Settings.
- [x] **Tags CRUD** — `app/admin/tags/{page,new,[id]}/page.tsx` + `features/tags/{api,schema,tag-form,tags-table}.tsx`. Settles every pattern: TanStack Query for list, mutation for create/update/delete with toast feedback, react-hook-form + zod for form validation (zod schema mirrors the API's FluentValidation rules), shadcn DataTable with filter input + paging controls, confirm-delete dialog.
- [x] `npm run build` clean: 10 routes, `proxy` middleware registered. Dev verification deferred to user (start API at `:5179`, then `npm run dev` in `src/web` → hit `:3000` → log in with seeded SuperAdmin).

### F2 — Settled CRUDs (replicate the F1 pattern)

10 entities, 3 pages each (list + new + edit) = 30 views. Each is essentially a copy of the Tags page with different fields.

- [ ] Allergens, TaxRates, PriceGroups, SalesChannels, PromotionCodes, ProductLists, ModifierGroups, Modifiers, Tables, PrintoutTemplates.
- Forms differ by ~3-5 fields each. Should be ~3-4 hours per entity at this point.

### F3 — Products + sub-resources

The catalog heavyweight. Single product detail page with tabs/sections for sub-collections.

- [ ] Product list (filter by name/barcode/taxRate, sort).
- [ ] Product add/edit (basic fields).
- [ ] Product detail page with sections:
  - Tags (attach/detach picker)
  - Allergens (attach/detach picker)
  - Modifier groups (attach/detach picker)
  - Images (URL upload + remove — actual file upload deferred to Phase 8)
  - Prices per PriceGroup (matrix-style edit)
- [ ] ProductList sub-resource UI: add/remove products from a list.

### F4 — Orders + Order management

- [ ] Order list with status filters (Placed / Accepted / InProgress / Ready / Closed / Cancelled), date range, outlet filter.
- [ ] Order detail with state transition buttons (`Accept`, `Start preparing`, `Mark ready`, `Close`, `Cancel`) — buttons enabled per current status.
- [ ] Walk-in order create form (Staff POS) — line items via product picker.

### F5 — Events + EventDays

- [ ] Event list with status filter.
- [ ] Event create/edit (name, description, image, optional ProductList + PriceGroup).
- [ ] Event detail: lifecycle buttons (Publish, Cancel, Close) + EventDays editor (add/remove dates).

### F6 — Auth: Users + Roles + Company settings

- [ ] Users list (Staff in current company) + edit (name) + assign roles UI.
- [ ] Register new staff form.
- [ ] Roles list + create + edit (with permission checkbox grid).
- [ ] Company settings page (legal name, tax id, billing fields, IsPublic flag for SuperAdmin).
- [ ] Outlet settings page (display name, address, phone, currency, logo URL).

### F7 — Loyalty admin views

- [ ] Loyalty entries list (filter by user).
- [ ] Manual adjustment form (user picker, signed points, required reason).

### F8 — SuperAdmin company switcher (optional, deferred from Phase 3)

- [ ] Companies list (SuperAdmin only).
- [ ] Create company form.
- [ ] Company context switcher in topbar (sets `X-Company-Id` header on every request).

---

## Mobile app (separate arc, after admin)

- [ ] React Native Expo project at `src/mobile/`. Sessions M1–M5 to be planned after F1–F7 settle.
- [ ] Customer flows: register/login → browse companies → browse products → place order with promo + loyalty → my orders → favorites → loyalty balance/history → settings.

---

## Naming convention

- Sessions prefixed `F` for frontend admin, `M` for mobile.
- Each session targets a self-contained slice that ships green.
