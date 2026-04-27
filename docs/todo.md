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
  - [x] Document usage pattern for list use cases (filter record → `ApplySort` → `ToPagedResultAsync`) — added to `src/api/CLAUDE.md` with a worked `ListProducts` example showing `PagedQuery`-derived filter record + `SortMap<TEntity>` + `Where → ApplySort → Select → ToPagedResultAsync` chain, plus a per-feature checklist (sort allow-list, indexes for sortable + filterable columns).
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

- [x] `accountType` — `SuperAdmin` / `Staff` / `Guest`. Emitted by `JwtTokenService.GenerateAsync`; round-tripped + read by every `[Authorize(Policy = Policies.RequireX)]` and verified by the auth tests.
- [x] `companyId` — Staff only. Emitted when `user.CompanyId is Guid`; consumed by `AppDbContext.CurrentCompanyId` for the global query filter.
- [x] `role` — Staff only, multi-valued. Emitted directly by `JwtTokenService` (one `ClaimTypes.Role` per assigned role, queried via `IgnoreQueryFilters` so the login flow can see roles regardless of HTTP context). The Owner-bypass policy reads it via `User.IsInRole(SystemRoles.Owner)`.
- [x] `permission` — Staff only, multi-valued, flattened from role-claims at login. Skipped for the Owner role (Owner has zero persisted claims and bypasses at the policy handler).
- [x] `sub` — user id. Maps to `ClaimTypes.NameIdentifier` via JwtBearer's default inbound claim mapping; read by `User.UserId`.
- [x] Signing key from `Jwt:Key` — user-secrets in dev, env var in prod, never `appsettings.json`
- [x] Token lifetime: **24h** — `Jwt:ExpiryHours = 24` in `appsettings.json` (single value for admin panel + mobile). Read by the Phase 4 token-generation code.

### Policies (two layers)

- [x] **Type-gate policies**: `RequireGuest`, `RequireStaff`, `RequireSuperAdmin`, `RequireStaffOrSuperAdmin` — registered via `AddAuthorizationBuilder.AddPolicy`, each requires the `accountType` claim to match. Names live as constants in `Features/Auth/Policies.cs`.
- [x] **Permission policies** (Staff granularity): one policy per entry in `Permissions.All`, registered upfront in a `foreach` loop in `Program.cs`. Policy name == permission string, so endpoints can use `[Authorize(Policy = Permissions.ProductsManage)]`. The `PermissionAuthorizationHandler` short-circuits on SuperAdmin **or** `Owner` role (`SystemRoles.Owner`) before falling back to the explicit `permission` claim check.
- [x] Single custom `PermissionRequirement` + `PermissionAuthorizationHandler` to evaluate permission policies uniformly. Handler registered as a singleton `IAuthorizationHandler`.
- [x] **Permission constants registry** — `Permissions` static class (flat PascalCase: `Permissions.ProductsManage = "ProductsManage"`; no `permissions:` prefix since the claim type already carries "permission"; staff-config only — Guest actions like `PlaceOrders` / `ManageFavorites` excluded) as single source of truth for policy attributes + role-claim seed + admin UI

### Endpoint scoping (flat routes, gated by policies)

- [x] Guest-only endpoints → `[Authorize(Policy = Policies.RequireGuest)]`. Pattern established; will appear on Phase 4's guest-flow endpoints (favorites, loyalty redeem, place-my-order).
- [x] Staff config endpoints → permission policy. Applied across all 16 Staff-config endpoints landed in Phase 3 (`Permissions.RolesManage`, `Permissions.UsersManage`, etc.).
- [x] SuperAdmin-only → `[Authorize(Policy = Policies.RequireSuperAdmin)]`. Applied on `POST /api/companies`, `GET /api/companies`, `PUT /api/companies/{id}`, `DELETE /api/companies/{id}`.
- [x] Shared endpoints — `[Authorize]` (default policy = `RequireAuthenticatedUser` from the global fallback). Used on `GET /api/companies/{id}` (handler differentiates SuperAdmin-sees-any vs Staff-sees-own) and `PUT /api/auth/password` (handler reads `User.UserId`).
- [x] **Company isolation** — enforced by the global query filter (`CompanyId == AppDbContext.CurrentCompanyId`) on every `ICompanyOwned` entity. Handlers don't re-filter by company; cross-company access via leaked ids returns 404 (entity hidden by filter) rather than 403. Verified by the cross-company isolation tests in `ListRolesTests.does_not_include_other_companies_roles` and `ListUsersTests.returns_staff_in_current_company_only`.

### Auth infrastructure prep

Small infra pieces landing **before** the auth endpoints. Not strict blockers, but cleaner if in place day one.

- [x] Strongly-typed `JwtOptions` record (`Issuer`, `Audience`, `Key`, `ExpiryHours`) bound from the `Jwt:` config section + registered via `IOptions<JwtOptions>`. Single source for both the JWT validation block in `Program.cs` and the upcoming token-generation code.
- [x] `AppDbContext.CurrentCompanyId` upgrade: when `accountType = SuperAdmin`, read the `X-Company-Id` header instead of the `companyId` claim. Lets SuperAdmin endpoints target a specific tenant without bypassing the global filter. Header constant lives on `ClaimsPrincipalExtensions.CompanyIdSwitchHeader`.
- [x] **`Company.IsPublic`** — boolean, NOT NULL, default `false`, public setter. Marks which companies are visible to guests in the public mobile-app discovery listing. SuperAdmin-managed (only flipped by `PUT /api/companies/{id}`). Default-false means new tenants are private until promoted; demo and internal tenants stay invisible until SuperAdmin enables them. Migration `AddCompanyIsPublic` adds the column.

### Auth endpoints

- [x] `POST /api/auth/login` — email + password → JWT (24h) carrying every claim from the JWT design above. No `/admin` vs `/mobile` split — JWT carries `accountType`. Returns same generic message on bad email vs bad password to prevent user-enumeration. Owner role-claim emission skips permission flattening (Owner bypasses).
- [x] `POST /api/auth/register/guest` — `[AllowAnonymous]`, self-serve; creates `AppUser` (`AccountType = Guest`, `CompanyId = null`). Auto-login: returns a JWT on success (mirrors Login response). Email duplicate returns 409 via explicit `FindByEmailAsync` precheck. Identity password complexity errors surface as `DomainException` (400) with the original messages joined.
- [x] `POST /api/auth/register/staff` — `Permissions.UsersManage`; creates `AppUser` (`AccountType = Staff`, `CompanyId` from `AppDbContext.CurrentCompanyId` — claim for Staff caller, header for SuperAdmin) and assigns role(s) by name. **No auto-login** (new user logs in separately with their initial password). **Forbids Owner** in the role list — Owner is granted only via `POST /api/users/{userId}/roles/{roleId}` (gated by `RolesManage`). User creation + role assignments wrapped in `db.Database.BeginTransactionAsync()` to avoid orphan users on partial failure.
- [x] `PUT /api/auth/password` — any authenticated user can change their own password (current + new). UserId pulled from `User.UserId` on the controller. Splits Identity errors by code: `"PasswordMismatch"` → `UnauthorizedAccessException` (401, matches login's auth-mistake handling); everything else (new-password complexity, etc.) → `DomainException` (400). Returns 204 No Content. **Stateless JWT**: existing tokens issued before the change continue to work until expiry — no revocation list (out of scope).
- [ ] No email verification, no refresh tokens, no forgotten-password reset (skipped for scope).
- [ ] Logout: client drops the token (stateless JWT, no server-side invalidation).

### Company management endpoints

- [x] `POST /api/companies` — `RequireSuperAdmin`. **Atomic** in one transaction: creates Company + Outlet (1:1) + `Owner` role + first Owner Staff user. Owner role-assignment uses `db.UserRoles.Add(...)` directly (bypasses the `ICompanyOwned` filter that hides the just-created role from `UserManager.AddToRoleAsync` in the SuperAdmin's request context). TaxId + email duplicates rejected up-front with 409. `IsPublic` from the request (default false). **Follow-ups:** add a DB unique index on `Company.TaxId` (currently app-level check only — race-condition window); DRY the seeder against this use case (the seeder still has its own copy of the same logic).
- [x] **Follow-up cleanup**: add unique index on `Company.TaxId` so duplicate creation hits 23505 at the DB level instead of slipping through the app-level race window. `HasIndex(x => x.TaxId).IsUnique()` in `CompanyConfiguration` + migration `AddCompanyTaxIdUniqueIndex`.
- [x] **Follow-up cleanup**: extract a shared `CreateCompanyWithOwner` flow used by both `StartupSeeder.SeedDemoCompanyAsync` and `CreateCompany.Execute`, so the bootstrap logic has one source. → `Features/Companies/CompanyProvisioning.cs` owns the shared flow; both callers are now thin wrappers (idempotency-skip vs uniqueness-409 pre-checks, then delegate).
- [x] `GET /api/companies` — `RequireSuperAdmin`; alphabetical list of all companies (id, LegalName, TaxId, BillingEmail, IsPublic, CreatedAt). No paging yet — thesis-scale list. Add `PagedQuery` filter+sort wiring later if it becomes useful.
- [x] `GET /api/companies/{id}` — `RequireAuthorization` + handler check: SuperAdmin sees any; Staff sees only their own (`user.CompanyId == id`), otherwise 403. Returns the Company fields plus the 1:1 Outlet (DisplayName, address, phone, timezone, currency, logo).
- [x] `PUT /api/companies/{id}` — `RequireSuperAdmin`; edits `LegalName`, `TaxId`, `InvoicingAddress`, `BillingEmail`, `BillingPhone`, `IsPublic`. Added `Company.Update(...)` method since the entity uses `private set` on most fields. TaxId uniqueness re-checked at app level (excluding self); the DB unique index would also catch duplicates. Returns 204.
- [x] `DELETE /api/companies/{id}` — `RequireSuperAdmin`. The `SoftDeletableSaveChangesInterceptor` converts `Remove(company)` into a soft-delete (sets `DeletedAt` / `DeletedBy`). The row stays; child records (Outlet, roles, users) keep their FKs intact and disappear from filtered queries once the parent is filtered out. Returns 204.
- [x] `GET /api/companies/public` — `[AllowAnonymous]`; lists `IsPublic = true` companies with the Outlet's customer-facing fields (DisplayName, StreetAddress, Phone, TimeZone, Currency, LogoUrl) for the mobile app's discover view. Auto-skips soft-deleted companies via the global filter. No paging — thesis-scale list.

### Role management endpoints

All scoped to the current company via the global `ICompanyOwned` query filter on `AppRole`. SuperAdmin operates against the `X-Company-Id`-selected tenant.

- [x] `GET /api/roles` — `Permissions.RolesManage`; lists roles in the current company (alphabetical, no paging yet) with their permission claims.
- [x] `POST /api/roles` — `Permissions.RolesManage`; creates a role with initial permission claims. Forbids creating a role named "Owner" (system-managed).
- [x] `PUT /api/roles/{id}` — `Permissions.RolesManage`; updates name + replaces claims wholesale. **Rejects** if the target is the `Owner` role or if the new name is "Owner".
- [x] `DELETE /api/roles/{id}` — `Permissions.RolesManage`; soft-delete via the interceptor. **Rejects** if target is the `Owner` role.
- [x] `POST /api/users/{userId}/roles/{roleId}` — `Permissions.RolesManage`; assigns role to user. Validates user is a Staff member of the current company. Idempotent (re-assignment is a no-op).
- [x] `DELETE /api/users/{userId}/roles/{roleId}` — `Permissions.RolesManage`; unassigns role. **Rejects** if it would leave the company with zero Owner-role holders.

### User management endpoints

- [x] `GET /api/users` — `Permissions.UsersManage`; alphabetical (LastName, FirstName) list of Staff users in the current company, with each user's role names.
- [x] `GET /api/users/{id}` — `Permissions.UsersManage`; single user. Validates user is a Staff member of the current company.
- [x] `PUT /api/users/{id}` — `Permissions.UsersManage`; edits **FirstName + LastName** only. Email change is deliberately out-of-scope for this slice (involves NormalizedEmail + uniqueness juggling); password change is via `PUT /api/auth/password` (self-only). Added `AppUser.UpdateProfile(...)` since fields use `private set`.
- [x] `DELETE /api/users/{id}` — `Permissions.UsersManage`; soft-delete via the interceptor. **Rejects** self-deletion (you can't delete your own account) and the last-Owner case (cannot delete the last Owner-role holder).

### Seeding

- [x] **Startup seed** runs when `ASPNETCORE_ENVIRONMENT != "Testing"` — `ApiFactory` (Phase 3.5) will override env to `Testing`, so tests get an empty DB automatically. In Development we also auto-`MigrateAsync()` first; in Production migrations are applied out-of-band (CI/CD) and only seeding runs.
- [x] SuperAdmin account: email + initial password from `Seed:SuperAdminEmail` / `Seed:SuperAdminPassword`. Idempotent — only created if no SuperAdmin exists. Skipped (with warning log) if config values missing.
- [x] Seed demo company + Outlet + Owner role + demo Owner Staff user for dev/demo. Demo Owner password from `Seed:DemoOwnerPassword`; if missing, the demo block is skipped. Owner-role lookup uses `IgnoreQueryFilters()` because the seeder runs without an HTTP context (so `CurrentCompanyId == Guid.Empty` and the global `ICompanyOwned` filter would hide the role).
- [x] **Where the seed values live**: dev defaults committed in `appsettings.Development.json` (only loaded when `ASPNETCORE_ENVIRONMENT=Development` — Production won't see them) so an evaluator can clone + `dotnet run` and log in immediately. Production deployments override via environment variables (`Seed__SuperAdminPassword` etc.). Never put real prod secrets in any committed file. Trade-off documented for thesis defense: clean config layering, throwaway dev creds are public-repo-safe because they protect nothing real.

### SuperAdmin company-context switching

SuperAdmin is **always seeded at startup** (idempotent in `StartupSeeder`) — no API endpoint to create one. Promotion is intentionally not exposed.

- [ ] SuperAdmin frontend lists all companies; picks one to work on (context switcher) — **frontend-only concern, deferred to the admin-panel phase.**
- [ ] Request carries `X-Company-Id` header (or company id re-baked into JWT on switch) — **frontend wiring, deferred.** Server already accepts the header (see next bullet).
- [x] Server-side resolution: `AppDbContext.CurrentCompanyId` reads `X-Company-Id` when `accountType = SuperAdmin`, otherwise falls back to the `companyId` JWT claim. Global query filter Just Works for the selected tenant.
- [x] Owner-style endpoints (products, outlets, events) work normally for the selected context — no special handling needed; the global filter does the work. Will be exercised by Phase 4 endpoints.
- [ ] **Thesis note**: in real SaaS, silent cross-tenant impersonation has GDPR / contract implications (needs audit logging, tenant consent, etc.). For this thesis it's acceptable as a scoped simplification — mention as a compliance consideration in the thesis documentation and log SuperAdmin actions for audit. **Standing reminder for the thesis writeup**, not an implementation item.

### Safeguards

- [x] Cannot delete/demote the **last user holding the `Owner` role** in a company. Multiple Owners are allowed — the rule is "at least one." Enforced inline in `DELETE /api/users/{id}` and `DELETE /api/users/{userId}/roles/{roleId}`. Covered by `DeleteUserTests.last_Owner_returns_409` and `UnassignRoleTests.last_Owner_removal_returns_409`.
- [x] **`Owner` role is system-managed** — cannot be renamed or deleted (the permission bypass keys off the role name `"Owner"`). `POST/PUT/DELETE /api/roles/{...}` reject when the target is the Owner role or when a non-Owner role is being renamed to "Owner". Covered by `CreateRoleTests.named_Owner_returns_403`, `UpdateRoleTests.targeting_Owner_returns_403`, `UpdateRoleTests.renaming_to_Owner_returns_403`, `DeleteRoleTests.targeting_Owner_returns_403`.
- [x] **`Owner` role membership** is gated by `Permissions.RolesManage` (which Owners always have via bypass). A lower-rank Staff user cannot grant themselves the Owner role unless an Owner explicitly gave them `RolesManage` first. The `RegisterStaff` endpoint additionally rejects "Owner" in the roles-list (`RegisterStaffTests.assigning_Owner_role_returns_403`) — Owner is granted only via the dedicated assign endpoint, gated by `RolesManage`.

### Other

- [x] `ClaimsPrincipal` extensions: `UserId`, `AccountType` (nullable, returns null if claim missing/invalid), `CompanyId` (nullable — Guests/SuperAdmins have none), `HasPermission(string)`. Claim-name constants colocated on `ClaimsPrincipalExtensions` (`AccountTypeClaim`, `CompanyIdClaim`, `PermissionClaim`).
- [x] **Resource-based authorization** infrastructure for Guest-owned resources (Order, Favorite, UserSettings, LoyaltyPointLog) — `Shared/ResourceOwnerAttribute.cs`. Generic `[ResourceOwner<TEntity>(routeArg, ownerProperty)]` that's an `IAsyncActionFilter`: pulls the route arg, parses it as a Guid, fetches the entity via `db.Set<TEntity>().FindAsync(...)` (filters apply normally), reads the named owner property via reflection, compares to `User.UserId`. Returns 404 on missing OR mismatched owner (intentional — avoids leaking existence via id enumeration). Phase 4's first Guest-owned use case wires it onto its action method; rolled out per-endpoint as those use cases land.

---

## Phase 3.5 — Test project setup + retroactive auth tests

Lands **after the Phase 3 auth / role / user / company endpoints** (their tests get written here, retroactively) and **before Phase 4** so every subsequent feature slice can ship with its tests in the same commit. Phase 4 features are authored test-first (or tests-alongside) using the scaffolding set up here.

**Scaffolding:**

- [x] Create `CafeRMS.Api.Tests` project (NUnit, `Microsoft.AspNetCore.Mvc.Testing`, `Microsoft.EntityFrameworkCore.InMemory`, FluentAssertions). Located at `src/CafeRMS.Api.Tests/`.
- [x] `ApiFactory : WebApplicationFactory<Program>` — overrides `AppDbContext` to use InMemory DB (unique GUID per factory). Uses an internal EF service provider to dodge the "two DB providers" guard tripping when both Npgsql and InMemory are registered. Forces `ASPNETCORE_ENVIRONMENT = "Testing"` so `StartupSeeder` and the dev-only auto-`MigrateAsync()` skip. Suppresses the InMemory `TransactionIgnoredWarning`. Wires up the production `Auditable` / `SoftDeletable` interceptors.
- [x] JWT test-token helper — `factory.CreateClientAs(accountType, userId?, companyId?, permissions[], roles[])`. Signs with the same `Jwt:Key` as `appsettings.json` so the real JwtBearer pipeline accepts the token (no bypass).
- [x] Anonymous-client helper for unauthenticated cases (`factory.CreateAnonymousClient()`).
- [x] Seed helpers: `SeedUserAsync(...)`, `SeedRoleAsync(...)`. Use `UserManager` / `RoleManager` in a fresh DI scope so audit + soft-delete interceptors fire correctly.
- [x] Common JSON + `ProblemDetails` assertion helpers — `Shared/ProblemDetailsAssertions.cs` exposes three `HttpResponseMessage` extensions: `ShouldBeProblemDetailsAsync(status)` (returns the parsed body for further assertions), `ShouldBeProblemDetailsWithDetailAsync(status, detail)` (exact match), `ShouldBeProblemDetailsContainingAsync(status, substring)`. Existing tests untouched; available for Phase 4 use as needed.
- [x] Folder layout mirrors `Features/`: `CafeRMS.Api.Tests/Features/<Feature>/<UseCase>Tests.cs`. Currently `Features/Auth/{Login,RegisterGuest,RegisterStaff,ChangePassword}Tests.cs`.
- [ ] Document philosophy in test project README (deferred).
- [x] Hook `dotnet test` into local dev loop — confirmed all 15 auth tests green via `dotnet test src/CafeRMS.Api.Tests/...`.

**Retroactive tests for the Phase 3 endpoints:**

- [x] Auth — 15 tests across the 4 endpoints, all green:
  - **Login** (4): happy path, wrong password (401), unknown email (401), invalid email format (400 from validator).
  - **RegisterGuest** (3): happy path (200 + JWT), duplicate email (409), weak password (400, Identity complexity).
  - **RegisterStaff** (4): happy path with role assignment, no-permission (403), Owner-role-in-list (403), no-company-context (403).
  - **ChangePassword** (4): happy path (204 + new password lets login + old does not), wrong current (401), weak new (400), anonymous (401 from auth fallback).
- [x] Companies — 22 tests across 6 endpoints. Includes the end-to-end "create company → Owner can log in → Owner can hit `Permissions.RolesManage` endpoints via the Owner-bypass" flow. Caught 2 production bugs: `GetCompanyById` and `ListPublicCompanies` queried `db.Outlets` (ICompanyOwned-filtered), so SuperAdmin without `X-Company-Id` and anonymous callers saw zero outlets. Fixed with `.IgnoreQueryFilters()` + explicit `DeletedAt == null` re-checks.
- [x] Roles — 23 tests across 6 endpoints (List, Create, Update, Delete, Assign, Unassign). Owner-role-system-managed enforcement (rename/delete rejection, "Owner" name on create/update rejection) + last-Owner orphan prevention covered + cross-company isolation via the global filter.
- [x] Users — 14 tests across 4 endpoints. Cross-company access → 403; self-delete → 409; last-Owner deletion → 409; permission gating.
- [x] **Production bug caught and fixed**: `JwtTokenService.GenerateAsync` was using `userManager.GetRolesAsync()`, which goes through the `AppRole` `ICompanyOwned` global filter. During login the request is anonymous → `CurrentCompanyId == Guid.Empty` → user's roles all filtered out → no `role` claim in the issued JWT → Owner-bypass never triggers after login. Fixed by querying `db.UserRoles.IgnoreQueryFilters()` joined to `db.Roles.IgnoreQueryFilters()` with explicit `DeletedAt == null`. `JwtTokenService` no longer takes `UserManager` / `RoleManager` — just `IOptions<JwtOptions>` + `AppDbContext`.

---

## Phase 4 — Implement use cases

### Cross-cutting rules (apply to every feature slice below)

- [ ] Implement every use case from Phase 1, feature by feature
- [ ] Apply filter/sort/paginate convention consistently on list endpoints
- [ ] FluentValidation on every command/request
- [ ] Domain exceptions only (no `Result<T>`)
- [ ] Each use case = one file (route + request + command + handler + validator + response)
- [ ] **Per feature**: add indexes for every sortable + filterable column; composite index where filter + sort combine (e.g., `(outlet_id, created_at DESC)`); always include `deleted_at` in soft-deletable tables (+ migration per feature)
- [ ] **Per feature**: define sort whitelist (mandatory — never pass raw user input to `OrderBy`) and filter record colocated with the list use case
- [ ] **Per feature — tests alongside endpoints**: every use case ships with API tests in the same commit (happy path + key failure cases: not-found, unauthorized, forbidden, validation, conflict). Use the shared scaffolding from Phase 3.5. No feature is "done" without its tests green.
- [ ] **Wrap multi-entity writes in transactions** — order placement, event registration, loyalty redemption, etc. Each handler that mutates multiple entities wraps the work in `db.Database.BeginTransactionAsync()` (or relies on the `SaveChanges` implicit transaction when a single call suffices). Moved here from Phase 2 — can't be done before the handlers exist.

### Session split (token-aware chunking)

One session per chunk. Each chunk = a self-contained slice that builds + tests green by end-of-session. Aim for ~500k token budget per session; stop and continue at ~70% used. Estimates assume the established Phase 3 patterns (per-endpoint controllers, Request/Command split, ApiFactory test fixtures) carry over without re-litigation.

- [x] **Session 1 — Simple CRUD warmup.** **Tags + Allergens + TaxRates + Tables.** ~4 features × ~5 endpoints = ~20 endpoints, ~40 tests. Settles the per-feature pattern. Each is a flat company-scoped CRUD + paged list with sort whitelist. No sub-entities, no transactions, no resource-authz. Estimated ~300–400k tokens. **Landed 20 endpoints + 47 tests; full suite 118 green.** Fixed the broken `PagedQuery`-derived `Query` example in `src/api/CLAUDE.md` along the way (positional params don't initialize inherited base-record properties — switched to object-init pattern).
  - **Per-feature pattern (applies to all four below):** entity gets `Update(...)`; 5 use cases (`Add` / `GetById` / `List` / `Update` / `Delete`) each in its own file with a sibling controller; `List` uses `PagedQuery`-derived filter + `SortMap` + `ApplySort` + `ToPagedResultAsync`; partial unique index `(CompanyId, Name)` filtered by `deleted_at IS NULL` (lets a soft-deleted name be reused); duplicate-name check throws `ConflictException` (409); `NotFoundException` (404) on missing id; one migration per feature; ~10 tests per feature (happy + key failures: 403/404/409/validation + cross-company isolation on `List`).
  - [x] **Tags** — `Permissions.ProductsManage` per use-cases.md (Tags live in the products concern). Filter: name contains (case-insensitive). Sort allow-list: `name`, `createdAt`. Routes under `/api/tags`. Migration `AddTagsUniqueNameIndex`. 11 tests green.
  - [x] **Allergens** — `Permissions.ProductsManage`. Filter: name contains. Sort: `name`, `createdAt`. Routes `/api/allergens`. Migration `AddAllergensUniqueNameIndex`. 12 tests green.
  - [x] **TaxRates** — `Permissions.TaxRatesManage`. Filter: name contains. Sort: `name`, `rate`, `createdAt`. Routes `/api/tax-rates`. Migration `AddTaxRatesUniqueNameIndex`. 12 tests green.
  - [x] **Tables** — `Permissions.TablesManage`. `OutletId` resolved server-side from the company's 1:1 outlet (no `outletId` in request body — single outlet per company). Filter: name contains. Sort: `name`, `createdAt`. Routes `/api/tables`. Migration `AddTablesUniqueNameIndex`. 12 tests green.
- [x] **Session 2 — Mid features.** **PriceGroups + SalesChannels + PromotionCodes + ProductLists + ModifierGroups + Modifiers.** ~6 features × ~5–7 endpoints = ~30–40 endpoints, ~60–80 tests. Introduces sub-entities (`ProductList → ProductListItem`, `ModifierGroup → Modifier`) and the `SalesChannel ↔ PriceGroup` join table. Some `ExecuteDeleteAsync` join-cleanup per the Phase 2 convention. Estimated ~500–700k tokens. Could split into two sessions if it's too dense. **Landed 35 endpoints + 67 tests; full suite 185 green.** Adapted the join-cleanup convention from `ExecuteDeleteAsync` to load-then-`RemoveRange` to keep tests on EF InMemory working.
  - **Per-feature pattern (carries over from Session 1):** entity gets `Update(...)`; 5 CRUD use cases each in its own file with sibling controller; `List` uses object-init `Query : PagedQuery` + `SortMap` + `ApplySort` + `ToPagedResultAsync`; partial unique index `(CompanyId, Name)` filtered by `deleted_at IS NULL`; duplicate-name → 409, missing id → 404; ~10 tests per feature.
  - **Session 2-specific:** sub-entity endpoints (`AddProductToList` / `RemoveProductFromList`, `LinkPriceGroup` / `UnlinkPriceGroup`); join-table cleanup on parent-side delete via load-then-`RemoveRange` (Phase 2 convention initially specified `ExecuteDeleteAsync`, but EF InMemory used by the test project doesn't support it; load-then-RemoveRange has identical semantics under both providers, with negligible cost since join tables are per-tenant tiny); schema additions to `PromotionCode` (ValidFrom/Until + MaxUses + UsesCount per use-cases.md open-question recommendation) and `Modifier` (PriceDelta).
  - **Retro-fit deferred:** `Tag` → `ProductTag` and `Allergen` → `ProductAllergen` join cleanup belongs to the Phase 2 convention but the join rows can't yet be created (Products endpoints land in Session 3). Add the `ExecuteDeleteAsync` lines to `DeleteTag` / `DeleteAllergen` in Session 3 alongside Products work.
  - [x] **PriceGroups** — `Permissions.PricingManage`. Pure CRUD. Migration `AddPriceGroupsUniqueNameIndex`. 12 tests green.
  - [x] **SalesChannels** — `Permissions.SalesChannelsManage`. CRUD + Link/Unlink PriceGroup. Migration `AddSalesChannelsUniqueNameIndex`. 12 tests green.
  - [x] **PromotionCodes** — `Permissions.PromotionsManage` for CRUD; `Authorize` on Validate. Added ValidFrom/Until + MaxUses + UsesCount fields. Migration `AddPromotionCodeFieldsAndPartialIndex`. 12 tests green.
  - [x] **ProductLists** — `Permissions.MenusManage`. CRUD + AddProductToList/RemoveProductFromList sub-resources. Migration `AddProductListsUniqueNameIndex`. 11 tests green.
  - [x] **ModifierGroups** — `Permissions.ModifiersManage`. Pure CRUD. Delete-with-children blocked (409). Migration `AddModifierGroupsUniqueNameIndex`. 9 tests green.
  - [x] **Modifiers** — `Permissions.ModifiersManage`. Added `PriceDelta` (decimal, default 0). Group-scoped name uniqueness `(CompanyId, ModifierGroupId, Name)`. Migration `AddModifiersPriceDeltaAndUniqueNameIndex`. 11 tests green.
- [x] **Session 3 — Products (+ optionally Outlets + PrintoutTemplates if room).** Product as the central catalog entity with all its sub-entities and joins: `ProductImage`, `ProductPrice` (per price group), `ProductTag`, `ProductAllergen`, `ProductModifierGroup`. ~10–12 endpoints + ~30 tests on Products alone. Outlets has the existing `Outlet` entity (1:1 with Company already enforced) — only Update/Get use cases needed (Create/Delete handled by Company onboarding). PrintoutTemplates is small CRUD. Estimated ~500–700k tokens. **Landed 21 endpoints + 35 tests; full suite 220 green.** All three features in scope shipped (Products + Outlets + PrintoutTemplates).
  - **Per-feature pattern (carries over from Sessions 1–2):** entity gets `Update(...)`; CRUD use cases each in their own file with sibling controller; `List` uses object-init `Query : PagedQuery` + `SortMap` + `ApplySort` + `ToPagedResultAsync`; partial unique index `(CompanyId, Name)` filtered by `deleted_at IS NULL`; duplicate-name → 409, missing id → 404.
  - [x] **Products** — `Permissions.ProductsManage`. CRUD on Name/Description/Barcode/TaxRateId. Partial unique `(CompanyId, Name)` and `(CompanyId, Barcode)` filtered by `deleted_at IS NULL AND barcode IS NOT NULL`. `DeleteProduct` cleans up all six sub-entity / join row sets in same UoW. 12 tests green.
  - [x] **Products sub-resources** — 9 endpoints (attach/detach for tags / allergens / modifier-groups, image add/remove, price upsert). `SetProductPrice` upsert via remove-then-add to avoid `init`-only Net hassle. 11 tests green including a `DeleteProduct_cleans_up_all_join_rows` end-to-end.
  - [x] **Outlets** — `Permissions.OutletManage`. `Get` + `Update` only. 4 tests green including cross-company id → 404 via global filter.
  - [x] **PrintoutTemplates** — `Permissions.PrintoutTemplatesManage`. Standard CRUD. 8 tests green.
  - [x] **Retro-fit Session 2 join cleanup** — `DeleteTag` / `DeleteAllergen` / `DeleteModifierGroup` (after block-on-children) / `DeletePriceGroup` all now drop the matching join / orphaned-price rows in the same UoW. Convention is now uniform across every parent-side delete in the codebase.
- [x] **Session 4 — Orders.** Full order lifecycle (closed business process #1): place → prepare → mark ready → close → optional cancel. Multi-entity transactions across `Order + OrderLine` (+ `LoyaltyPointLog` on close + `PromotionCode` validation). **First adoption of `[ResourceOwner<Order>]`** on guest-facing endpoints — settles the wiring pattern for the rest. ~10–12 endpoints + ~30 tests including domain edge cases (closed-already, cancelled-already, stock validation, promo expiry). Estimated ~600–800k tokens — the heaviest single-feature session. **Landed 12 endpoints + 22 tests; full suite 242 green.** Closed-process #1 (Order lifecycle, including loyalty earn on Close) is now end-to-end functional.
  - **Schema deltas to `Order`:** add `AcceptedAt` / `InProgressAt` / `ReadyAt` (DateTimeOffset?), `PromotionCodeId` (Guid?, nullable FK to `PromotionCode`). Computed `OrderStatus` enum derived from timestamps (`Placed` if no Accepted/Cancelled; then `Accepted`, `InProgress`, `Ready`, `Closed`, `Cancelled`). One migration `AddOrderLifecycleTimestampsAndPromoFK`.
  - **PlaceOrder use case (transactional, shared):** one use case backs both `POST /api/my/orders` (Guest, UserId from JWT) and `POST /api/orders` (Staff walk-in, optional UserId). Steps in `BeginTransactionAsync`: validate outlet/product/optional table-channel-event; for each line resolve `ProductPrice` (FIFO if no `priceGroupId` given — first matching `ProductPrice` row, else 404 with "no price set"); compute Net per line and pull VAT from `Product.TaxRate.Rate`; if `promotionCode` given, run validation logic + apply discount + increment `UsesCount`; if `loyaltyPointsUsed > 0`, validate balance via `SUM(LoyaltyPointLog.Points)` for the user + insert negative log entry; create `Order` + `OrderLine` rows; commit. Validators reject empty lines / negative qty / negative loyalty.
    - **Important wrinkle — Guest has no `companyId` in JWT.** `db.CurrentCompanyId` resolves to `Guid.Empty` for Guests, which makes the `ICompanyOwned` global filter hide every cross-referenced entity (Outlet, Product, PriceGroup, PromotionCode). `PlaceOrder` therefore uses `IgnoreQueryFilters()` everywhere and **derives the company from the target outlet** (lookup via `IgnoreQueryFilters`, then read `outlet.CompanyId`); subsequent lookups manually filter by that derived `CompanyId`. The command carries an optional `CallerCompanyId` (set for Staff, null for Guest) — when set, an outlet whose `CompanyId` doesn't match returns 404 (don't leak cross-tenant existence).
  - **Endpoints (12 total):**
    - Guest (`Policies.RequireGuest`): `POST /api/my/orders`, `GET /api/my/orders`, `GET /api/my/orders/{id}` + `[ResourceOwner<Order>(...)]`, `POST /api/my/orders/{id}/cancel` + `[ResourceOwner<Order>]` (allowed only before `AcceptedAt` — guard inside use case).
    - Staff (`Permissions.OrdersManage`): `POST /api/orders` (walk-in), `POST /api/orders/{id}/accept`, `POST /api/orders/{id}/start-preparing`, `POST /api/orders/{id}/ready`, `POST /api/orders/{id}/close`, `POST /api/orders/{id}/cancel` (with reason).
    - Staff (`Permissions.OrdersView` for read, `Permissions.OrdersManage` includes view): `GET /api/orders` (paged + filters), `GET /api/orders/{id}`.
  - **Closed-process step 3 — loyalty earn on Close:** `CloseOrder` inserts a positive `LoyaltyPointLog` entry: `Points = floor(Order.Net total - Discount)` (1 point per integer unit of currency net). No-op if `UserId` is null (walk-in). Wrapped in transaction with the `ClosedAt` timestamp set.
  - **State-transition guards:** every transition rejects (409) if the order is `Closed` or `Cancelled`; `Accept` requires no prior `AcceptedAt`; `StartPreparing` requires `AcceptedAt && !InProgressAt`; `MarkReady` requires `InProgressAt && !ReadyAt`; `Close` requires `ReadyAt && !ClosedAt`. Cancel works from any non-terminal state.
  - **First `[ResourceOwner<Order>]` adoption:** wires the attribute onto `GET /api/my/orders/{id}` and `POST /api/my/orders/{id}/cancel`. Resource owner key = `Order.UserId`. Settles the pattern for Sessions 5 + 6.
  - **Skipped for scope (out of session):** stock/inventory checks (no inventory entity), per-line modifier selection (`OrderLineModifier` open-question — defer to Session 4.5 or fold into Session 5 if needed), in-flight line edits (`POST /api/orders/{id}/lines` etc. — orders are immutable post-place; cancel + re-place to edit).
- [x] **Session 5 — Events + EventDays.** Closed business process #2. Event create / publish / cancel + EventDays add/remove + close-event-with-loyalty-payout. ~9 endpoints + ~12 tests. Estimated ~400–500k tokens. **Landed 9 endpoints + 11 tests; full suite 253 green.** Closed-process #2 functional via `Order.EventId` attendance derivation.
  - **Decision: skip `EventRegistration` entity entirely.** Attendance is derived from `Order.EventId` — i.e. "anyone who placed an order tagged to this event during one of its days". Loyalty payout on `CloseEvent` distributes a flat bonus (50 pts) to every distinct user with such an order. Drops the pre-register / RSVP / capacity-cap features (deferred indefinitely; can add the entity later without disturbing what shipped).
  - **Schema deltas to `Event`:** add `PublishedAt` and `ClosedAt` (`DateTimeOffset?`) alongside existing `CancelledAt`. Computed `Status` enum from timestamps: `Draft` → `Published` → `Closed`, with `Cancelled` as terminal sibling. `Update` method + lifecycle methods on the entity.
  - **Endpoints (9):** `POST /api/events`, `GET /api/events` (paged), `GET /api/events/{id}`, `PUT /api/events/{id}`, `POST /api/events/{id}/publish`, `POST /api/events/{id}/cancel`, `POST /api/events/{id}/close` (loyalty payout, transactional), `POST /api/events/{id}/days`, `DELETE /api/events/{id}/days/{dayId}`. All gated by `Permissions.EventsManage`.
  - **Skipped for scope:** customer-facing event browse (Guest has no companyId in JWT — same problem as PlaceOrder; defer until mobile app wiring needs it). Customers still trigger process #2 by placing orders with `EventId`.
- [x] **Session 6 — Favorites + Loyalty + UserSettings.** Three small Guest-owned features. Closed business process #3 closes here (Loyalty earn / redeem from Order + Event payout). ~9 endpoints + ~15 tests. Estimated ~300–400k tokens. **Landed 9 endpoints + 14 tests; full suite 267 green.** Closed-process #3 (Loyalty lifecycle: earn from Close Order/Event, redeem in PlaceOrder, manual adjust via staff endpoint, balance via Guest endpoint) is now end-to-end functional.
  - **Favorites (3):** `POST /api/my/favorites` body `{productId}` (idempotent), `DELETE /api/my/favorites/{productId}` (404 if not favorited), `GET /api/my/favorites` (own list with product detail). Guest-only. UserId scoping inside the use case (no `[ResourceOwner]` — composite key, no separate id route arg).
  - **Loyalty (4):** Guest — `GET /api/my/loyalty/balance` (sum), `GET /api/my/loyalty/history` (paged log). Staff (`Permissions.LoyaltyManage`) — `GET /api/loyalty/entries` (paged + filter by userId), `POST /api/loyalty/entries` (manual adjustment with required reason; signed Points).
  - **UserSettings (2):** Guest — `GET /api/my/settings` (auto-creates on first access), `PUT /api/my/settings` (Theme + UiSettingsJson).
  - **Closed-process #3:** earn happens on `CloseOrder` (Session 4) + `CloseEvent` (Session 5). Redeem happens inside `PlaceOrder` (Session 4). Manual adjust via the new staff endpoint. Balance read via Guest endpoint. Lifecycle now end-to-end.
- [x] **Session 7 — Cross-feature E2E (Phase 6).** Sanity-pass + 3 closed-process E2E flows — multi-step API calls in single tests, each touching 5+ features. Estimated ~300–400k tokens. **Landed 3 E2E tests; full suite 270 green.** Tests follow the activity diagrams (`docs/diagrams/1-online-ordering.md`, `2-product-menu-management.md`, `3-event-management.md`) — note the diagrams name the 3 closed processes Online ordering / Product-menu mgmt / Event mgmt (loyalty is folded into the Order flow), which differs slightly from CLAUDE.md's "Order/Event/Loyalty lifecycle" wording.

Phase 4 + Phase 6 together ≈ **7 sessions** — **all shipped**. Full suite 270 green.

---

## Sequencing decision (post-Session-7)

**Reorder:** do **Frontend first**, then Phases 5 / 7 / 8 last.

Why this works: Phases 5 (DB views/sprocs/functions), 7 (PDF/Excel reports), 8 (Word printouts) are independent of frontend — they're their own URLs that the admin panel just links to. Doing them after the frontend means designing reports against UIs that actually exist, and avoids retrofitting the frontend later.

**Why this is risky:** Phases 5 / 7 / 8 are **hard university requirements** (per `docs/Projekt Inżynierski - Wymagania.pdf` and the `## Hard requirements` section in `CLAUDE.md`). They cannot be cut if the frontend overruns — they have to ship before defense.

**New order:**
1. **Frontend admin panel** (Next.js, ~5–7 sessions). 50-UI-views requirement lives here.
2. **Frontend mobile app** (React Native Expo, ~3–5 sessions).
3. **Phase 5 — DB artifacts** (~1 session). Design views around what the actually-built reports/UI need.
4. **Phase 7 — Report endpoints** (~1 session). Tiny once Phase 5 is done.
5. **Phase 8 — Word printout templates** (~1 session). Also tiny.

Total trailing arc: **~3 sessions** for what was previously planned as 3 sessions of standalone reporting work — same effort, better-informed shape.

---

### Backend follow-ups deferred from earlier sessions (do during Phase 5/7/8)

- **Customer-facing `GET /api/events`** — Guest event browse. Same outletId-derives-companyId trick used in PlaceOrder. Skipped in Session 5 because Guest has no companyId in JWT. Add when mobile-app event browsing needs it.
- **`OrderLineModifier` entity** — open question from Phase 1. If/when modifier selection on order lines is needed (probably triggered by mobile-app product detail screen wanting to send modifier picks).
- **`EventRegistration` entity** — open question. If/when RSVP / capacity-cap features become real.
- **`Loyalty tier` derivation** — `fn_loyalty_tier(user_id)` planned but not implemented; lands as part of Phase 5 alongside the other loyalty SQL.
- **`PromotionCode.UsesCount` increment is non-locking** — race condition theoretically possible if the same code is redeemed in parallel. Real-world cafe scale makes this irrelevant; defer until/if it ever matters.

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
