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
- [x] `AppUser` and `AppRole` implement `IAuditable` + `ISoftDeletable` — Identity-rooted entities can't reparent to the `Entity` base class (single inheritance with `IdentityUser<Guid>` / `IdentityRole<Guid>`), so they declare the 6 audit/soft-delete properties manually. Soft-delete on `AppUser` is mandatory: hard-deleting a user with past `Order` / `LoyaltyPointLog` rows would either FK-fail or cascade-delete history (which we made non-deletable on purpose). `AppUser` is **not** `ICompanyOwned` — see entry above for why.
- [x] `AppRole.CompanyId` FK to `Company`, **NOT NULL** — every role belongs to exactly one company. SuperAdmin works off `AccountType`, not roles, so no "global role" branch. `OnDelete: Cascade` (a role has no meaning outside its company; if the company is hard-deleted, its roles go too). `AppRole` also implements `ICompanyOwned` so the global query filter auto-scopes role lookups (`RoleManager.FindByNameAsync` etc.) — `AppUser` is **not** `ICompanyOwned` because Guests and SuperAdmins have null `CompanyId` and the simple filter would exclude them; user scoping happens explicitly in handlers.
- [x] Override `AppRole`'s default unique index on `NormalizedName` with a composite **`(CompanyId, NormalizedName)`** unique index so two companies can each have their own "Owner" / "Cashier" / etc. role. Default `RoleNameIndex` is downgraded to non-unique (`HasDatabaseName("RoleNameIndex").IsUnique(false)`) rather than removed, since EF tracks Identity's named index by metadata.
- [x] Auto-seed an **`Owner`** role (name `"Owner"`, **no** persisted claims) for each company. Owner is a *concept*, not a permission bag — the permission policy handler gives Owner a blanket allow without checking claims. This means new permissions added in future features automatically apply to existing Owners with **zero backfill** (no role-claim drift). `SystemRoles.Owner = "Owner"` constant added so the bypass isn't a magic string. Implemented in `StartupSeeder` for the demo company; will be reused by the Phase 4 `POST /api/companies` use case.
- [x] `Company` fields: `LegalName`, `TaxId`, `InvoicingAddress`, `BillingEmail`, `BillingPhone` — legal/accounting shell; drops today's `Name` / `Address` / `Currency` / `TimeZone` (moved to `Outlet` per the 1:1 split)
- [x] `Outlet` fields: rename `Name` → `DisplayName` (customer-facing brand), rename `Address` → `StreetAddress`, add `Phone`, `TimeZone`, `LogoUrl`; keep `Currency` (enum)
- [x] Enforce **1:1 Company↔Outlet** via unique index on `Outlet.CompanyId` — all three Company/Outlet changes ship in one migration; staff scoping stays company-wide as a result
- [x] `AppUser : IdentityUser<Guid>`, `AppRole : IdentityRole<Guid>`, `AppDbContext : IdentityDbContext<AppUser, AppRole, Guid>` — keeps every user-FK as `uuid`, not `varchar(GUID-as-string)`
- [x] In `AppDbContext.OnModelCreating`, `builder.Ignore<IdentityUserClaim<Guid>>()`, `Ignore<IdentityUserLogin<Guid>>()`, `Ignore<IdentityUserToken<Guid>>()` so only the 4 Identity tables materialize in the migration

### JWT claim design

> **Design notes (consumer side already wired in `ClaimsPrincipalExtensions` / policies / handler — these tick when the Phase 4 login endpoint emits them):**

- [ ] `accountType` — `SuperAdmin` / `Staff` / `Guest`
- [ ] `companyId` — Staff only
- [ ] `role` — Staff only, multi-valued; emitted automatically by Identity's default `UserClaimsPrincipalFactory` from the user's role assignments. The Owner-bypass policy reads this claim.
- [ ] `permission` — Staff only, multi-valued, flattened from role-claims at login (skipped for Owners since they bypass).
- [ ] `sub` — user id
- [x] Signing key from `Jwt:Key` — user-secrets in dev, env var in prod, never `appsettings.json`
- [x] Token lifetime: **24h** — `Jwt:ExpiryHours = 24` in `appsettings.json` (single value for admin panel + mobile). Read by the Phase 4 token-generation code.

### Policies (two layers)

- [x] **Type-gate policies**: `RequireGuest`, `RequireStaff`, `RequireSuperAdmin`, `RequireStaffOrSuperAdmin` — registered via `AddAuthorizationBuilder.AddPolicy`, each requires the `accountType` claim to match. Names live as constants in `Features/Auth/Policies.cs`.
- [x] **Permission policies** (Staff granularity): one policy per entry in `Permissions.All`, registered upfront in a `foreach` loop in `Program.cs`. Policy name == permission string, so endpoints can use `[Authorize(Policy = Permissions.ProductsManage)]`. The `PermissionAuthorizationHandler` short-circuits on SuperAdmin **or** `Owner` role (`SystemRoles.Owner`) before falling back to the explicit `permission` claim check.
- [x] Single custom `PermissionRequirement` + `PermissionAuthorizationHandler` to evaluate permission policies uniformly. Handler registered as a singleton `IAuthorizationHandler`.
- [x] **Permission constants registry** — `Permissions` static class (flat PascalCase: `Permissions.ProductsManage = "ProductsManage"`; no `permissions:` prefix since the claim type already carries "permission"; staff-config only — Guest actions like `PlaceOrders` / `ManageFavorites` excluded) as single source of truth for policy attributes + role-claim seed + admin UI

### Endpoint scoping (flat routes, gated by policies)

- [ ] Guest-only endpoints → `RequireGuest` (`POST /api/my/orders`, favorites, loyalty redeem, …)
- [ ] Staff config endpoints → permission policy (`DELETE /api/products/{id}` → `Permissions.ProductsDelete`)
- [ ] SuperAdmin-only → `RequireSuperAdmin` (`POST /api/companies`, cross-company ops)
- [ ] Shared endpoints (e.g. `GET /api/products`) — `RequireAuthorization()`; handler adapts response by `accountType`
- [ ] **Company isolation**: enforced by the global query filter (`CompanyId == AppDbContext.CurrentCompanyId`, see multi-tenancy block) — handlers don't re-filter by company

### Auth infrastructure prep

Small infra pieces landing **before** the auth endpoints. Not strict blockers, but cleaner if in place day one.

- [x] Strongly-typed `JwtOptions` record (`Issuer`, `Audience`, `Key`, `ExpiryHours`) bound from the `Jwt:` config section + registered via `IOptions<JwtOptions>`. Single source for both the JWT validation block in `Program.cs` and the upcoming token-generation code.
- [x] `AppDbContext.CurrentCompanyId` upgrade: when `accountType = SuperAdmin`, read the `X-Company-Id` header instead of the `companyId` claim. Lets SuperAdmin endpoints target a specific tenant without bypassing the global filter. Header constant lives on `ClaimsPrincipalExtensions.CompanyIdSwitchHeader`.
- [x] **`Company.IsPublic`** — boolean, NOT NULL, default `false`, public setter. Marks which companies are visible to guests in the public mobile-app discovery listing. SuperAdmin-managed (only flipped by `PUT /api/companies/{id}`). Default-false means new tenants are private until promoted; demo and internal tenants stay invisible until SuperAdmin enables them. Migration `AddCompanyIsPublic` adds the column.

### Auth endpoints

- [x] `POST /api/auth/login` — email + password → JWT (24h) carrying every claim from the JWT design above. No `/admin` vs `/mobile` split — JWT carries `accountType`. Returns same generic message on bad email vs bad password to prevent user-enumeration. Owner role-claim emission skips permission flattening (Owner bypasses).
- [x] `POST /api/auth/register/guest` — `[AllowAnonymous]`, self-serve; creates `AppUser` (`AccountType = Guest`, `CompanyId = null`). Auto-login: returns a JWT on success (mirrors Login response). Email duplicate returns 409 via explicit `FindByEmailAsync` precheck. Identity password complexity errors surface as `DomainException` (400) with the original messages joined.
- [ ] `POST /api/auth/register/staff` — `Permissions.UsersManage`; creates `AppUser` (`AccountType = Staff`, `CompanyId` from the current context) and assigns role(s).
- [ ] `PUT /api/auth/password` — `RequireAuthorization`; current + new password.
- [ ] No email verification, no refresh tokens, no forgotten-password reset (skipped for scope).
- [ ] Logout: client drops the token (stateless JWT, no server-side invalidation).

### Company management endpoints

- [ ] `POST /api/companies` — `RequireSuperAdmin`. **Atomic** in one transaction: creates Company + Outlet (1:1) + auto-seeded `Owner` role + first Owner Staff user (payload includes their email + name + initial password). Reuses the same Owner-seeding logic as `StartupSeeder` (factor it out into a shared service).
- [ ] `GET /api/companies` — `RequireSuperAdmin`; paged + filtered + sorted list of all companies.
- [ ] `GET /api/companies/{id}` — `RequireAuthorization` + handler check: SuperAdmin sees any; Staff sees only their own (`user.CompanyId == id`), otherwise 403.
- [ ] `PUT /api/companies/{id}` — `RequireSuperAdmin`; edits `LegalName`, `TaxId`, billing fields, `IsPublic`.
- [ ] `DELETE /api/companies/{id}` — `RequireSuperAdmin`; soft-delete (cascades to Outlet, roles, users via FKs).
- [ ] `GET /api/companies/public` — `[AllowAnonymous]`; lists `IsPublic = true` companies with the Outlet's customer-facing fields (DisplayName, StreetAddress, LogoUrl, Currency, Phone) for the mobile app's discover view.

### Role management endpoints

All scoped to the current company via the global `ICompanyOwned` query filter on `AppRole`. SuperAdmin operates against the `X-Company-Id`-selected tenant.

- [ ] `GET /api/roles` — `Permissions.RolesManage`; lists roles in the current company (paged).
- [ ] `POST /api/roles` — `Permissions.RolesManage`; creates a role with its initial permission claims.
- [ ] `PUT /api/roles/{id}` — `Permissions.RolesManage`; updates name + claims. **Rejects with 400** if the target is the `Owner` role (system-managed).
- [ ] `DELETE /api/roles/{id}` — `Permissions.RolesManage`; soft-delete. **Rejects** if target is the `Owner` role.
- [ ] `POST /api/users/{userId}/roles/{roleId}` — `Permissions.RolesManage`; assigns role to user.
- [ ] `DELETE /api/users/{userId}/roles/{roleId}` — `Permissions.RolesManage`; unassigns role. **Rejects** if it would leave the company with zero Owner-role holders.

### User management endpoints

- [ ] `GET /api/users` — `Permissions.UsersManage`; paged + filtered + sorted list of Staff users in the current company.
- [ ] `GET /api/users/{id}` — `Permissions.UsersManage`; single user.
- [ ] `PUT /api/users/{id}` — `Permissions.UsersManage`; edits name, email (NOT password — that's only the user themselves via `PUT /api/auth/password`).
- [ ] `DELETE /api/users/{id}` — `Permissions.UsersManage`; soft-delete (the `SoftDeletableSaveChangesInterceptor` converts the `UserManager.DeleteAsync` hard-delete into a soft-delete). **Rejects** if user is the last Owner-role holder in the company.

### Seeding

- [x] **Startup seed** runs when `ASPNETCORE_ENVIRONMENT != "Testing"` — `ApiFactory` (Phase 3.5) will override env to `Testing`, so tests get an empty DB automatically. In Development we also auto-`MigrateAsync()` first; in Production migrations are applied out-of-band (CI/CD) and only seeding runs.
- [x] SuperAdmin account: email + initial password from `Seed:SuperAdminEmail` / `Seed:SuperAdminPassword`. Idempotent — only created if no SuperAdmin exists. Skipped (with warning log) if config values missing.
- [x] Seed demo company + Outlet + Owner role + demo Owner Staff user for dev/demo. Demo Owner password from `Seed:DemoOwnerPassword`; if missing, the demo block is skipped. Owner-role lookup uses `IgnoreQueryFilters()` because the seeder runs without an HTTP context (so `CurrentCompanyId == Guid.Empty` and the global `ICompanyOwned` filter would hide the role).
- [x] **Where the seed values live**: dev defaults committed in `appsettings.Development.json` (only loaded when `ASPNETCORE_ENVIRONMENT=Development` — Production won't see them) so an evaluator can clone + `dotnet run` and log in immediately. Production deployments override via environment variables (`Seed__SuperAdminPassword` etc.). Never put real prod secrets in any committed file. Trade-off documented for thesis defense: clean config layering, throwaway dev creds are public-repo-safe because they protect nothing real.

### SuperAdmin company-context switching

SuperAdmin is **always seeded at startup** (idempotent in `StartupSeeder`) — no API endpoint to create one. Promotion is intentionally not exposed.

- [ ] SuperAdmin frontend lists all companies; picks one to work on (context switcher) — frontend-only concern
- [ ] Request carries `X-Company-Id` header (or company id re-baked into JWT on switch)
- [ ] Server-side resolution: covered by the **Auth infrastructure prep** bullet (`AppDbContext.CurrentCompanyId` reads `X-Company-Id` when `accountType = SuperAdmin`). Once that lands, the global query filter Just Works for the selected tenant.
- [ ] Owner-style endpoints (products, outlets, events) work normally for the selected context — no special handling needed once the resolution works
- [ ] **Thesis note**: in real SaaS, silent cross-tenant impersonation has GDPR / contract implications (needs audit logging, tenant consent, etc.). For this thesis it's acceptable as a scoped simplification — mention as a compliance consideration in the thesis documentation and log SuperAdmin actions for audit.

### Safeguards

- [ ] Cannot delete/demote the **last user holding the `Owner` role** in a company. Multiple Owners are allowed — the rule is "at least one." Enforced inline in `DELETE /api/users/{id}` and `DELETE /api/users/{userId}/roles/{roleId}`.
- [ ] **`Owner` role is system-managed** — cannot be renamed or deleted (the permission bypass keys off the role name `"Owner"`). `PUT/DELETE /api/roles/{id}` reject when the target is the Owner role.
- [ ] **`Owner` role membership** is gated by `Permissions.RolesManage` (which Owners always have via bypass). A lower-rank Staff user cannot grant themselves the Owner role unless an Owner explicitly gave them `RolesManage` first.

### Other

- [x] `ClaimsPrincipal` extensions: `UserId`, `AccountType` (nullable, returns null if claim missing/invalid), `CompanyId` (nullable — Guests/SuperAdmins have none), `HasPermission(string)`. Claim-name constants colocated on `ClaimsPrincipalExtensions` (`AccountTypeClaim`, `CompanyIdClaim`, `PermissionClaim`).
- [ ] Brainstorm **resource-based authorization** (guest A can't mutate guest B's order / favorite via passed IDs) — options: `IAuthorizationService` + handlers, endpoint filters, or inline ownership check; decide per resource (Orders, Favorites, UserSettings, Loyalty, …)

---

## Phase 3.5 — Test project setup + retroactive auth tests

Lands **after the Phase 3 auth / role / user / company endpoints** (their tests get written here, retroactively) and **before Phase 4** so every subsequent feature slice can ship with its tests in the same commit. Phase 4 features are authored test-first (or tests-alongside) using the scaffolding set up here.

**Scaffolding:**

- [ ] Create `CafeRMS.Api.Tests` project (NUnit, `Microsoft.AspNetCore.Mvc.Testing`, `Microsoft.EntityFrameworkCore.InMemory`, FluentAssertions)
- [ ] `ApiFactory : WebApplicationFactory<Program>` — overrides `AppDbContext` to use InMemory DB (unique GUID per test) so tests are isolated and parallel-safe; forces `ASPNETCORE_ENVIRONMENT = "Testing"` so `StartupSeeder` skips
- [ ] JWT test-token helper — `factory.CreateClientAs(accountType, companyId?, permissions[])` returning an `HttpClient` with `Authorization: Bearer ...` set; uses the same `JwtOptions` as the app, exercises the real auth pipeline (no bypass)
- [ ] Anonymous-client helper for unauthenticated cases (`factory.CreateAnonymousClient()`)
- [ ] Common JSON + `ProblemDetails` assertion helpers (colocated, no base class)
- [ ] Folder layout mirrors `Features/` per `CLAUDE.md`: `CafeRMS.Api.Tests/Features/<Feature>/<UseCase>Tests.cs`
- [ ] Document philosophy in test project README: **no mocking** unless unavoidable; happy path first, then key failure cases; one test class per use case file
- [ ] Hook `dotnet test` into local dev loop

**Retroactive tests for the Phase 3 endpoints:**

- [ ] Auth — login (happy / wrong password / non-existent), register/guest (happy / duplicate email / weak password), register/staff (happy / forbidden by permission / forbidden by no-company), password change (happy / wrong current).
- [ ] Companies — create (happy / non-SuperAdmin forbidden / duplicate tax id), list, get-own / get-other gating, edit (`IsPublic` flip), delete; public listing returns only `IsPublic = true`.
- [ ] Roles — CRUD with permission gating; `Owner` role rename/delete rejection; last-Owner orphan prevention.
- [ ] Users — list / get / delete with permission gating; last-Owner-deletion rejection.

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
