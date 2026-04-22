# API TODO — finish the backend

Ordered by phase. Reports & printouts intentionally last.

---

## Phase 0 — Immediate fixes & foundations

- [x] Set up Controllers infrastructure (no endpoints exist yet — this is setup, not migration)
  - [x] `AddControllers()` + `MapControllers()` in `Program.cs`, drop `MapGroup("/api")` scaffold
  - [x] Global fallback auth policy (`RequireAuthenticatedUser`) — no base class; controllers use `[ApiController]` + `[Route("api/[controller]")]`, opt out of auth with `[AllowAnonymous]`
  - [x] Convert `ValidationFilter` from `IEndpointFilter` → `IAsyncActionFilter`, register globally
  - [x] New top-level `Controllers/` folder — deferred: folder materializes when the first feature controller lands in Phase 4; no empty scaffold
- [x] Make `CreatedBy` non-nullable (+ migration)
- [x] Add order cancellation reason — update `Order` entity (nullable `CancellationReason`) + migration
- [x] During an event, keep a standard menu available for customers not participating in the event — no API change; mobile app concern (shows both menus, customer picks)
- [x] Shared **filter / sort / paginate** convention — `?sort=name,-createdAt&page=1&pageSize=20` (`-` prefix = DESC)
  - [x] `PagedQuery` record (`Page`, `PageSize`, `Sort` + feature-specific filter fields via inheritance)
  - [x] `PagedResult<T>` response (`Items`, `Page`, `PageSize`, `Total`)
  - [x] `IQueryable<T>` extension `ApplySort(string sortExpression, SortMap<T> allowedFields)` — mandatory allow-list, typed via `SortMap.Add<TKey>(name, selector)` so no `object`-boxing in `OrderBy`
  - [x] `IQueryable<T>` extension `ToPagedResultAsync(PagedQuery)` (dropped separate `ApplyPaging` — no caller needed just-paging without count; add back when infinite-scroll endpoint appears)
  - [x] Default + max `PageSize` as consts on `PagedQuery` (20 / 100)
  - [ ] Document usage pattern for list use cases (filter record → `ApplySort` → `ToPagedResultAsync`) — deferred to first list use case in Phase 4
- [x] CORS config (admin panel + mobile app origins) — origins read from `Cors:AllowedOrigins` in config
- [x] Basic request logging via built-in `UseHttpLogging` (method, path, query, status, duration)
- [x] Health check endpoint (`/health`) — built-in `AddHealthChecks`, anonymous; DB probe deferred until `AspNetCore.HealthChecks.EntityFrameworkCore` is approved
- [x] Verify `GlobalExceptionHandler` maps all domain exceptions → `ProblemDetails`
  - [x] Brainstorm full domain exception taxonomy — decided on minimal generic set: `DomainException` (base, 400), `NotFoundException` (404), `ConflictException` (409), `ForbiddenException` (403). Feature-specific exceptions (`InsufficientStockException`, `EventFullException`, etc.) added per-feature in Phase 4 by extending the right generic.
  - [x] Decided: **fewer generic types**, feature-specific subclasses per-feature. Rejected a separate `ValidationException` (422) — input validation is FluentValidation → `ValidationFilter` → 400; business-rule violations extend `DomainException` / `ConflictException`.
  - [x] Handler maps DomainException/NotFound/Conflict/Forbidden → 400/404/409/403; unknown → 500 (logged); only emits `Detail` for DomainException subtypes so internal 500 messages aren't leaked.

---

## Phase 1 — Use case brainstorm (per feature)

> Output: [`docs/use-cases.md`](./use-cases.md). Open questions at the bottom need a decision pass before Phase 4.

Go feature by feature and write down every use case. Map each to one of the **3 closed business processes**.

Candidate business processes:
1. **Order lifecycle** — cart → place → prepare → fulfil → close → (optional) cancel w/ reason
2. **Event lifecycle** — create → publish → register → run → close → loyalty payout
3. **Loyalty lifecycle** — enroll → earn points → redeem → tier change → expiry

For each feature folder, list:
- CRUD use cases (Add / Get / List / Update / Delete)
- Business actions (non-CRUD transitions, e.g., `PlaceOrder`, `CancelOrder`, `RegisterForEvent`, `RedeemPoints`)
- Which business process it participates in

Features to walk through:

- [x] Allergens
- [x] Auth
- [x] Companies
- [x] Events
- [x] Favorites
- [x] Loyalty
- [x] ModifierGroups
- [x] Modifiers
- [x] Orders
- [x] Outlets
- [x] PriceGroups
- [x] PrintoutTemplates
- [x] ProductLists
- [x] Products
- [x] PromotionCodes
- [x] SalesChannels
- [x] Tables
- [x] Tags
- [x] TaxRates
- [x] UserSettings

---

## Phase 2 — EF Core infrastructure

- [x] `IAuditable` interface + `SaveChangesInterceptor` to auto-set `CreatedAt/CreatedBy/UpdatedAt/UpdatedBy` — interceptor owns all four fields; factories no longer take `createdBy`
- [x] `ISoftDeletable` interface + interceptor to set `DeletedAt/DeletedBy` on delete (convert `Remove` → soft delete) — Auditable runs first (skips `Deleted` state), SoftDeletable flips state to `Unchanged` + sets `DeletedAt/By`, so only the two columns update and no stray `UpdatedAt/By` stamp
- [x] **Global query filter** for `ISoftDeletable` (skip join tables) — centralized in `AppDbContext.OnModelCreating` via expression-tree over `builder.Model.GetEntityTypes()`; removed 16 duplicated `HasQueryFilter(x => x.DeletedAt == null)` lines from feature configs. No join tables implement `ISoftDeletable`, so none had to be skipped.
- [ ] **Join-table cleanup on soft-delete — Phase 4 convention**: every `DeleteX` handler whose entity is on one side of a join table must hard-delete the matching join rows in the same unit of work (`await db.ProductTags.Where(pt => pt.TagId == id).ExecuteDeleteAsync()`). Required for: `Tag` (→ ProductTag), `Allergen` (→ ProductAllergen), `ModifierGroup` / `Product` (→ ProductModifierGroup), `ProductList` / `Product` (→ ProductListItem), `SalesChannel` / `PriceGroup` (→ SalesChannelPriceGroup), `Product` (→ Favorite). Rationale: a join row *is* the relationship — when one side logically goes away, the link goes away. Explicit one-liner per handler is easier to explain in thesis defense than an auto-cascade interceptor.
- [x] **Optimistic concurrency**: use Postgres `xmin` as shadow row-version property on entities edited by multiple users (zero app-logic overhead, native PG); map `DbUpdateConcurrencyException` → `ConflictException` in global handler — applied to every `ISoftDeletable` entity via `ApplyXminConcurrencyTokens` in `AppDbContext.OnModelCreating` (shadow `uint xmin` property, `IsConcurrencyToken + ValueGeneratedOnAddOrUpdate`; Npgsql maps it to the existing Postgres system column, so the migration body is empty — no DDL needed). `GlobalExceptionHandler` now maps `DbUpdateConcurrencyException` → 409 with a "record was modified by another user" detail message. Also moved `Migrations/` → `Persistence/Migrations/` to colocate with `AppDbContext.cs`.
- [x] Review every generated migration before applying — standing discipline; already caught the xmin `AddColumn` mis-emission and emptied the migration body before it hit the DB.

---

## Phase 3 — Auth, identity, permissions

Identity tables: **4 only** (`asp_net_users`, `asp_net_roles`, `asp_net_user_roles`, `asp_net_role_claims`) — the 3 default-Identity tables `UserClaims`, `UserLogins`, `UserTokens` are excluded from the schema (see Data model below). Roles are **dynamic, per-company**; claims attached to roles. User / role / role-claim CRUD lives in the **Auth feature use cases** brainstormed in Phase 1 — this phase is cross-cutting infra + data-model decisions.

### Multi-tenancy — `CompanyId` FK sweep

One-time schema sweep before the feature work starts. Every top-level config entity is owned by a company.

- [ ] Add `CompanyId` FK + migration to 13 entities: `Product`, `Event`, `PromotionCode`, `Tag`, `Allergen`, `ModifierGroup`, `Modifier`, `PriceGroup`, `ProductList`, `SalesChannel`, `TaxRate`, `Table`, `PrintoutTemplate` (`Outlet` already has it)
- [ ] Add `CompanyId` FK + migration to `LoyaltyPointLog` — no transitive path via `UserId` since guests span companies
- [ ] `ICurrentCompany` service — reads `companyId` from JWT (Staff) or `X-Company-Id` header (SuperAdmin context switch); scoped DI lifetime
- [ ] **Global query filter** centralized in `AppDbContext.OnModelCreating`: for every entity with `CompanyId`, auto-filter by `_currentCompany.Id` — same loop-over-`builder.Model.GetEntityTypes()` pattern as the existing `ISoftDeletable` filter, so handlers never type `.Where(x => x.CompanyId == …)` by hand
- [ ] Entities that **don't** need direct `CompanyId` (reach company transitively): `Order` (via `Outlet`), `Favorite` (via `Product`), `EventDay` (via `Event`), `UserSettings` (global per-guest, mobile-app only), all join tables

### Data model

- [ ] `AppUser.AccountType` enum on `AppUser`: `SuperAdmin` | `Staff` | `Guest` (single source of truth)
- [ ] `Guest` entity — 1:1 with `AppUser` (AccountType = Guest); holds guest profile (loyalty link, preferences, etc.)
- [ ] `StaffMember` entity — 1:1 with `AppUser` (AccountType = Staff); FK to `Company`, attached to dynamic roles
- [ ] `AppRole` scoped per-company (add `CompanyId` FK; null = global, only used by the SuperAdmin flow if at all)
- [ ] Auto-seed an **`Owner`** role with full claims when a new company is registered (creator of the company → assigned Owner)
- [x] `Company` fields: `LegalName`, `TaxId`, `InvoicingAddress`, `BillingEmail`, `BillingPhone` — legal/accounting shell; drops today's `Name` / `Address` / `Currency` / `TimeZone` (moved to `Outlet` per the 1:1 split)
- [x] `Outlet` fields: rename `Name` → `DisplayName` (customer-facing brand), rename `Address` → `StreetAddress`, add `Phone`, `TimeZone`, `LogoUrl`; keep `Currency` (enum)
- [x] Enforce **1:1 Company↔Outlet** via unique index on `Outlet.CompanyId` — all three Company/Outlet changes ship in one migration; staff scoping stays company-wide as a result
- [ ] `AppUser : IdentityUser<Guid>`, `AppRole : IdentityRole<Guid>`, `AppDbContext : IdentityDbContext<AppUser, AppRole, Guid>` — keeps every user-FK as `uuid`, not `varchar(GUID-as-string)`
- [ ] In `AppDbContext.OnModelCreating`, `builder.Ignore<IdentityUserClaim<Guid>>()`, `Ignore<IdentityUserLogin<Guid>>()`, `Ignore<IdentityUserToken<Guid>>()` so only the 4 Identity tables materialize in the migration

### JWT claim design

- [ ] `accountType` — `SuperAdmin` / `Staff` / `Guest`
- [ ] `companyId` — Staff only
- [ ] `permission` — Staff only, multi-valued, flattened from role-claims at login
- [ ] `sub` — user id
- [ ] Signing key from `Jwt:SigningKey` — user-secrets in dev, env var in prod, never `appsettings.json`
- [ ] Token lifetime: **24h** (single value for admin panel + mobile)

### Policies (two layers)

- [ ] **Type-gate policies**: `RequireGuest`, `RequireStaff`, `RequireSuperAdmin`, `RequireStaffOrSuperAdmin`
- [ ] **Permission policies** (Staff granularity): `products:write`, `products:delete`, `orders:refund`, `events:manage`, … — each passes if SuperAdmin, OR Staff with matching `permission` claim
- [ ] Single custom `PermissionRequirement` + handler to evaluate permission policies uniformly
- [ ] **Permission constants registry** — `Permissions` static class (e.g. `Permissions.Products.Delete = "products:delete"`) as single source of truth for policy attributes + role-claim seed + admin UI

### Endpoint scoping (flat routes, gated by policies)

- [ ] Guest-only endpoints → `RequireGuest` (`POST /api/my/orders`, favorites, loyalty redeem, …)
- [ ] Staff config endpoints → permission policy (`DELETE /api/products/{id}` → `products:delete`)
- [ ] SuperAdmin-only → `RequireSuperAdmin` (`POST /api/companies`, cross-company ops)
- [ ] Shared endpoints (e.g. `GET /api/products`) — `RequireAuthorization()`; handler adapts response by `accountType`
- [ ] **Company isolation**: enforced by the global query filter on `ICurrentCompany` (see multi-tenancy block) — handlers don't re-filter by company

### Auth use cases (lives in Phase 1 Auth brainstorm; cross-ref here)

- [ ] Single `/auth/login` endpoint (no `/admin` vs `/mobile` split — JWT carries `accountType`)
- [ ] `POST /auth/register/guest` — self-serve, creates `AppUser` + `Guest`
- [ ] `POST /auth/register/staff` — Owner/SuperAdmin creates staff in their company, creates `AppUser` + `StaffMember`
- [ ] `PUT /auth/password` — logged-in user changes own password (current + new)
- [ ] No email verification, no refresh tokens, no forgotten-password reset (skipped for scope)
- [ ] Logout: client drops the token (stateless JWT, no server-side invalidation)

### Seeding

- [ ] **Startup seed** runs when `ASPNETCORE_ENVIRONMENT != "Testing"` — `ApiFactory` overrides env to `Testing`, so tests get an empty DB automatically (no extra flag)
- [ ] SuperAdmin account: email + initial password from `Seed:SuperAdminEmail` / `Seed:SuperAdminPassword` (user-secrets in dev, env var in prod, never hardcoded). Idempotent — only created if no SuperAdmin exists.
- [ ] Seed demo company + Owner user for dev/demo
- [ ] On `POST /api/companies` (SuperAdmin): auto-create default `Owner` role with full claims + assign creator

### SuperAdmin company-context switching

- [ ] SuperAdmin frontend lists all companies; picks one to work on (context switcher)
- [ ] Request carries `X-Company-Id` header (or company id re-baked into JWT on switch)
- [ ] Server: if `accountType = SuperAdmin`, `ICurrentCompany` resolves from `X-Company-Id` instead of the `companyId` claim; global query filter Just Works
- [ ] Owner-style endpoints (products, outlets, events) work normally for the selected context
- [ ] Promotion path `POST /api/superadmins` (SuperAdmin-only) — optional, skip for thesis scope unless needed
- [ ] **Thesis note**: in real SaaS, silent cross-tenant impersonation has GDPR / contract implications (needs audit logging, tenant consent, etc.). For this thesis it's acceptable as a scoped simplification — mention as a compliance consideration in the thesis documentation and log SuperAdmin actions for audit.

### Safeguards

- [ ] Cannot delete/demote the **last user holding the `Owner` role** in a company (prevents orphaning)
- [ ] Role-CRUD endpoints (create role, assign claims, assign users to roles) are Owner-only — prevents a Staff user from self-granting elevated permissions

### Other

- [ ] `ClaimsPrincipal` extensions: `UserId`, `AccountType`, `CompanyId`, `HasPermission(string)`
- [ ] Brainstorm **resource-based authorization** (guest A can't mutate guest B's order / favorite via passed IDs) — options: `IAuthorizationService` + handlers, endpoint filters, or inline ownership check; decide per resource (Orders, Favorites, UserSettings, Loyalty, …)

---

## Phase 3.5 — Test project setup

Must land **before Phase 4** so every feature slice can ship with its tests in the same commit. Per-endpoint tests are authored **inside Phase 4** alongside their use case; only the shared scaffolding + e2e tests live outside it.

- [ ] Create `CafeRMS.Api.Tests` project (NUnit, `Microsoft.AspNetCore.Mvc.Testing`, `Microsoft.EntityFrameworkCore.InMemory`, FluentAssertions)
- [ ] `ApiFactory : WebApplicationFactory<Program>` — overrides `AppDbContext` to use InMemory DB (unique GUID per test) so tests are isolated and parallel-safe
- [ ] JWT test-token helper — `factory.CreateClientAs(accountType, companyId?, permissions[])` returning an `HttpClient` with `Authorization: Bearer ...` set; exercises the real auth pipeline, no auth bypass
- [ ] Anonymous-client helper for unauthenticated cases (`factory.CreateAnonymousClient()`)
- [ ] Common JSON + `ProblemDetails` assertion helpers (colocated, no base class)
- [ ] Folder layout mirrors `Features/` per `CLAUDE.md`: `CafeRMS.Api.Tests/Features/<Feature>/<UseCase>Tests.cs`
- [ ] Document philosophy in test project README: **no mocking** unless unavoidable; happy path first, then key failure cases; one test class per use case file
- [ ] Hook `dotnet test` into local dev loop before the first Phase 4 feature lands

---

## Phase 4 — Implement use cases

- [ ] Implement every use case from Phase 1, feature by feature
- [ ] Apply filter/sort/paginate convention consistently on list endpoints
- [ ] FluentValidation on every command/request
- [ ] Domain exceptions only (no `Result<T>`)
- [ ] Each use case = one file (route + request + command + handler + validator + response)
- [ ] **Per feature**: add indexes for every sortable + filterable column; composite index where filter + sort combine (e.g., `(outlet_id, created_at DESC)`); always include `deleted_at` in soft-deletable tables (+ migration per feature)
- [ ] **Per feature**: define sort whitelist (mandatory — never pass raw user input to `OrderBy`) and filter record colocated with the list use case
- [ ] **Per feature — tests alongside endpoints**: every use case ships with API tests in the same commit (happy path + key failure cases: not-found, unauthorized, forbidden, validation, conflict). Use the shared scaffolding from Phase 3.5. No feature is "done" without its tests green.
- [ ] **Wrap multi-entity writes in transactions** — order placement, event registration, loyalty redemption, etc. Each handler that mutates multiple entities wraps the work in `db.Database.BeginTransactionAsync()` (or relies on the `SaveChanges` implicit transaction when a single call suffices). Moved here from Phase 2 — can't be done before the handlers exist.

---

## Phase 5 — DB artifacts for reports (views / stored procs / functions)

Required by university. Design these as the **query layer for the 3 reports in Phase 7** — don't build artifacts that don't feed a report. Add via raw-SQL migrations.

> Non-report indexes are handled **per-feature in Phase 4**. This phase is strictly reporting SQL.

### Report 1 — Sales
- [ ] `v_order_details` — flat view: order + lines + modifiers + product + outlet + customer
- [ ] `v_outlet_daily_revenue` — per outlet × day: orders count, gross, net, tax, discounts
- [ ] `v_product_sales_summary` — per product: qty sold, revenue, over date range
- [ ] `fn_order_total(order_id)` — total with tax + modifiers + promos applied
- [ ] `sp_sales_report(outlet_id?, from, to)` — full sales report rowset

### Report 2 — Event performance
- [ ] `v_event_attendance` — per event: capacity, registered, attended, cancellations, revenue
- [ ] `sp_event_performance_report(from, to)` — event report rowset

### Report 3 — Loyalty
- [ ] `v_loyalty_customer_summary` — per user: points balance, tier, lifetime earned, lifetime redeemed, last activity
- [ ] `fn_loyalty_points_earned(order_id)` — points earned for a single order
- [ ] `sp_loyalty_report(from, to)` — loyalty report rowset (points issued, redeemed, active members)

---

## Phase 6 — End-to-end tests for closed processes

Test-project scaffolding is in Phase 3.5; per-endpoint tests ship inside Phase 4 alongside each use case. This phase covers only the cross-feature, end-to-end scenarios that can't be written until the relevant features exist.

- [ ] **E2E: Order lifecycle** — register customer → browse menu → place order (with promo + loyalty redemption) → staff accept → start preparing → mark ready → close → loyalty points credited
- [ ] **E2E: Event lifecycle** — staff create event + days → publish → customer register → staff check-in → close event → loyalty payout for attendees
- [ ] **E2E: Loyalty lifecycle** — customer enrollment (implicit at registration) → earn from order → earn from event → redeem in next order → balance + history reflect all activity
- [ ] Sanity pass: all per-feature tests from Phase 4 still green after the system is wired end-to-end

---

## Phase 7 — Reports (PDF + Excel export)

2–3 business reports, each exportable as both PDF and Excel.

Candidates:
- [ ] Sales report (per outlet, per period, per product)
- [ ] Event performance report (attendance, revenue, cancellations)
- [ ] Loyalty report (active members, points issued/redeemed)

Tasks:
- [ ] Pick libraries (PDF: QuestPDF; Excel: ClosedXML) — confirm before adding NuGet
- [ ] Report query layer (views / stored procs from Phase 5)
- [ ] `/reports/{name}?format=pdf|xlsx` endpoints with filter params

---

## Phase 8 — Word template printouts

Parameterized printouts from user-editable Word templates.

- [ ] Template upload + storage (admin)
- [ ] Template metadata entity (name, target entity type, placeholders)
- [ ] Rendering engine (e.g., `DocX` or `OpenXML` + placeholder replacement) — confirm library before adding
- [ ] Endpoint: `/printouts/{templateId}?entityId=...` → rendered `.docx` / PDF
- [ ] Seed default templates (receipt, event confirmation, etc.)
