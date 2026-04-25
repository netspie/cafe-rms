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

- [x] Add `CompanyId` FK + migration to 13 entities: `Product`, `Event`, `PromotionCode`, `Tag`, `Allergen`, `ModifierGroup`, `Modifier`, `PriceGroup`, `ProductList`, `SalesChannel`, `TaxRate`, `Table`, `PrintoutTemplate` (`Outlet` already has it); `PromotionCode` unique index flipped to composite `(CompanyId, Code)` so the same promo string can coexist across companies
- [x] Add `CompanyId` FK + migration to `LoyaltyPointLog` — no transitive path via `UserId` since guests span companies
- [x] `AppDbContext.CurrentCompanyId` — reads the `companyId` claim from JWT directly via `IHttpContextAccessor` (no separate `ICurrentCompany` service — single-impl interface was unnecessary). `X-Company-Id` header branch for SuperAdmin context switch is deferred to the SuperAdmin section below.
- [x] **Global query filter** centralized in `AppDbContext.OnModelCreating`: the 15 tenant-scoped entities now implement `ICompanyScoped` (marker interface). `ApplyQueryFilters` loops `builder.Model.GetEntityTypes()` and dispatches via reflection to three generic helpers (soft-delete only, company-scope only, or both combined) so each entity gets a single `HasQueryFilter(...)` expression — EF allows only one filter per entity, so soft-delete + company filters are merged into `e.CompanyId == CurrentCompanyId && e.DeletedAt == null` for entities that need both.

> **Note (design intent, not a TODO):** entities that reach company transitively don't get their own `CompanyId` / `ICompanyOwned` marker — they're filtered by walking the parent: `Order` (via `Outlet`), `Favorite` (via `Product`), `EventDay` (via `Event`), `UserSettings` (per-guest, mobile-app only), all join tables. EF query filters don't traverse navigations, so handlers must filter through the parent explicitly (e.g. `_db.Orders.Where(o => o.Outlet.CompanyId == _ctx.CurrentCompanyId)`).

### Entity base classes & audit / soft-delete cleanup

Cross-cutting cleanup once the multi-tenant query filter is in place. Removes duplicated audit/soft-delete property declarations across 22 entities, and corrects two entities (`Order`, `Event`) whose soft-delete shape was wrong — they're append-only history once committed, voided via `CancelledAt`, not deleted.

- [x] Rename `ICompanyScoped` → `ICompanyOwned` (interface + all `IsCompanyScoped` / `ApplyCompanyScopeFilter` / `ApplyCompanyScopeAndSoftDeleteFilter` references in `AppDbContext`) to match the `CompanyOwnedEntity` base-class naming
- [x] Add base classes in `src/CafeRMS.Api/Shared/Entities/`:
  - `Entity` — `IAuditable` props
  - `SoftDeletableEntity : Entity, ISoftDeletable`
  - `CompanyOwnedEntity : Entity, ICompanyOwned`
  - `CompanyOwnedSoftDeletableEntity : SoftDeletableEntity, ICompanyOwned`
- [x] Reparent all 22 business entities to the right base, delete now-redundant property declarations:
  - `Entity` (6): EventDay, OrderLine, UserSettings, ProductImage, ProductPrice, **Order** (after soft-delete drop)
  - `SoftDeletableEntity` (1): Company
  - `CompanyOwnedEntity` (2): LoyaltyPointLog, **Event** (after soft-delete drop)
  - `CompanyOwnedSoftDeletableEntity` (13): Allergen, Modifier, ModifierGroup, Outlet, PriceGroup, PrintoutTemplate, Product, ProductList, PromotionCode, SalesChannel, Table, Tag, TaxRate
- [x] Drop `ISoftDeletable` from `Order` (no `DeletedAt` / `DeletedBy`); orders are append-only history, voided via existing `CancelledAt` / `CancellationReason`
- [x] Drop `ISoftDeletable` from `Event`; add `CancelledAt` (`DateTimeOffset?`) + `CancellationReason` (`string?`) + `IsCancelled` computed prop — same lifecycle as Order (history once committed)
- [x] Broaden `ApplyXminConcurrencyTokens` walk: key off `IAuditable` instead of `ISoftDeletable` so mutable non-soft-delete entities (`Order`, `OrderLine`, `Event`) get concurrency tokens too
- [x] Migration `OrderEventLifecycleCleanup`: drops `orders.deleted_at` / `orders.deleted_by` / `events.deleted_by`, renames `events.deleted_at` → `events.cancelled_at` (cleanest DDL — empty dev DB), adds `events.cancellation_reason`. The xmin broadening generated 6 spurious `AddColumn<uint>("xmin", ...)` calls that were stripped by hand (Postgres provides `xmin` as a system column on every table; Npgsql maps to it directly, so no DDL is needed).

### Data model

- [x] `AppUser.AccountType` enum on `AppUser`: `SuperAdmin` | `Staff` | `Guest`. Stored as **string** column (DB-inspectable, thesis-explainable). Configured via `AppUserConfiguration` (`HasConversion<string>().HasMaxLength(20).IsRequired()`).
- [x] `AppUser.CompanyId` — nullable FK to `Company`, set **only** when `AccountType = Staff`. `Company?` nav added; `OnDelete: Restrict` (defensive — Company is soft-delete anyway). **No separate `Guest` / `StaffMember` 1:1 entities** — they'd carry no fields beyond what's already on `AppUser` / `UserSettings` / `LoyaltyPointLog`, so they'd be empty wrappers. Add a sub-table later only if a Staff- or Guest-only field appears that genuinely doesn't fit on `AppUser`.
- [ ] `AppRole.CompanyId` FK to `Company`, **NOT NULL** — every role belongs to exactly one company. SuperAdmin works off `AccountType`, not roles, so no "global role" branch.
- [ ] Override `AppRole`'s default unique index on `NormalizedName` with a composite **`(CompanyId, NormalizedName)`** so two companies can each have their own "Owner" / "Cashier" / etc. role.
- [ ] Auto-seed an **`Owner`** role with every permission when a new company is registered. Permissions enumerated via **reflection** over `typeof(Permissions).GetFields()` (every `public const string` becomes a role-claim) — single line, no manual `Permissions.All` to drift out of sync. The creator of the company gets the seeded `Owner` role assigned.
- [ ] Company registration is **atomic** — Company + Outlet (1:1) + auto-seeded `Owner` role + (if the endpoint creates one) the first Owner Staff user + role assignment all succeed together or none do, in a single EF transaction.
- [x] `Company` fields: `LegalName`, `TaxId`, `InvoicingAddress`, `BillingEmail`, `BillingPhone` — legal/accounting shell; drops today's `Name` / `Address` / `Currency` / `TimeZone` (moved to `Outlet` per the 1:1 split)
- [x] `Outlet` fields: rename `Name` → `DisplayName` (customer-facing brand), rename `Address` → `StreetAddress`, add `Phone`, `TimeZone`, `LogoUrl`; keep `Currency` (enum)
- [x] Enforce **1:1 Company↔Outlet** via unique index on `Outlet.CompanyId` — all three Company/Outlet changes ship in one migration; staff scoping stays company-wide as a result
- [x] `AppUser : IdentityUser<Guid>`, `AppRole : IdentityRole<Guid>`, `AppDbContext : IdentityDbContext<AppUser, AppRole, Guid>` — keeps every user-FK as `uuid`, not `varchar(GUID-as-string)`
- [x] In `AppDbContext.OnModelCreating`, `builder.Ignore<IdentityUserClaim<Guid>>()`, `Ignore<IdentityUserLogin<Guid>>()`, `Ignore<IdentityUserToken<Guid>>()` so only the 4 Identity tables materialize in the migration

### JWT claim design

- [ ] `accountType` — `SuperAdmin` / `Staff` / `Guest`
- [ ] `companyId` — Staff only
- [ ] `permission` — Staff only, multi-valued, flattened from role-claims at login
- [ ] `sub` — user id
- [x] Signing key from `Jwt:Key` — user-secrets in dev, env var in prod, never `appsettings.json`
- [ ] Token lifetime: **24h** (single value for admin panel + mobile)

### Policies (two layers)

- [ ] **Type-gate policies**: `RequireGuest`, `RequireStaff`, `RequireSuperAdmin`, `RequireStaffOrSuperAdmin`
- [ ] **Permission policies** (Staff granularity): `Permissions.ProductsManage`, `Permissions.ProductsDelete`, `Permissions.OrdersRefund`, `Permissions.EventsManage`, … (flat PascalCase — see Permission constants registry above). Each passes if SuperAdmin, OR Staff with matching `permission` claim.
- [ ] Single custom `PermissionRequirement` + handler to evaluate permission policies uniformly
- [x] **Permission constants registry** — `Permissions` static class (flat PascalCase: `Permissions.ProductsManage = "ProductsManage"`; no `permissions:` prefix since the claim type already carries "permission"; staff-config only — Guest actions like `PlaceOrders` / `ManageFavorites` excluded) as single source of truth for policy attributes + role-claim seed + admin UI

### Endpoint scoping (flat routes, gated by policies)

- [ ] Guest-only endpoints → `RequireGuest` (`POST /api/my/orders`, favorites, loyalty redeem, …)
- [ ] Staff config endpoints → permission policy (`DELETE /api/products/{id}` → `Permissions.ProductsDelete`)
- [ ] SuperAdmin-only → `RequireSuperAdmin` (`POST /api/companies`, cross-company ops)
- [ ] Shared endpoints (e.g. `GET /api/products`) — `RequireAuthorization()`; handler adapts response by `accountType`
- [ ] **Company isolation**: enforced by the global query filter (`CompanyId == AppDbContext.CurrentCompanyId`, see multi-tenancy block) — handlers don't re-filter by company

### Auth use cases (lives in Phase 1 Auth brainstorm; cross-ref here)

- [ ] Single `/auth/login` endpoint (no `/admin` vs `/mobile` split — JWT carries `accountType`)
- [ ] `POST /auth/register/guest` — self-serve, creates `AppUser` (`AccountType = Guest`, `CompanyId = null`)
- [ ] `POST /auth/register/staff` — Owner/SuperAdmin creates staff in a company; creates `AppUser` (`AccountType = Staff`, `CompanyId` set) and assigns role(s)
- [ ] `PUT /auth/password` — logged-in user changes own password (current + new)
- [ ] No email verification, no refresh tokens, no forgotten-password reset (skipped for scope)
- [ ] Logout: client drops the token (stateless JWT, no server-side invalidation)

### Seeding

- [ ] **Startup seed** runs when `ASPNETCORE_ENVIRONMENT != "Testing"` — `ApiFactory` overrides env to `Testing`, so tests get an empty DB automatically (no extra flag)
- [ ] SuperAdmin account: email + initial password from `Seed:SuperAdminEmail` / `Seed:SuperAdminPassword` (user-secrets in dev, env var in prod, never hardcoded). Idempotent — only created if no SuperAdmin exists.
- [ ] Seed demo company + Owner user for dev/demo

### SuperAdmin company-context switching

- [ ] SuperAdmin frontend lists all companies; picks one to work on (context switcher)
- [ ] Request carries `X-Company-Id` header (or company id re-baked into JWT on switch)
- [ ] Server: if `accountType = SuperAdmin`, `AppDbContext.CurrentCompanyId` resolves from the `X-Company-Id` header instead of the `companyId` claim; global query filter Just Works
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
