# API TODO — finish the backend

Ordered by phase. Reports & printouts intentionally last.

---

## Phase 0 — Immediate fixes & foundations

- [x] Set up Controllers infrastructure (no endpoints exist yet — this is setup, not migration)
  - [x] `AddControllers()` + `MapControllers()` in `Program.cs`, drop `MapGroup("/api")` scaffold
  - [x] Global fallback auth policy (`RequireAuthenticatedUser`) — no base class; controllers use `[ApiController]` + `[Route("api/[controller]")]`, opt out of auth with `[AllowAnonymous]`
  - [x] Convert `ValidationFilter` from `IEndpointFilter` → `IAsyncActionFilter`, register globally
  - [ ] New top-level `Controllers/` folder — thin HTTP layer, one controller per feature; actual feature controllers added per-feature during Phase 4
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
- [ ] Health check endpoint (`/health`)
- [ ] Verify `GlobalExceptionHandler` maps all domain exceptions → `ProblemDetails`
  - [ ] Brainstorm full domain exception taxonomy — candidates: `NotFoundException`, `DomainException` (base), `ValidationException`, `ConflictException`, `ForbiddenException`, `InsufficientStockException`, `EventFullException`, `OrderAlreadyCancelledException`, `PromotionExpiredException`, `LoyaltyPointsInsufficientException`, etc.
  - [ ] Decide granularity: fewer generic types (e.g., just `DomainException` w/ codes) vs. more specific subclasses
  - [ ] Ensure each mapped to correct HTTP status in handler (404 / 400 / 403 / 409 / 422)

---

## Phase 1 — Use case brainstorm (per feature)

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

- [ ] Allergens
- [ ] Auth
- [ ] Companies
- [ ] Events
- [ ] Favorites
- [ ] Loyalty
- [ ] ModifierGroups
- [ ] Modifiers
- [ ] Orders
- [ ] Outlets
- [ ] PriceGroups
- [ ] PrintoutTemplates
- [ ] ProductLists
- [ ] Products
- [ ] PromotionCodes
- [ ] SalesChannels
- [ ] Tables
- [ ] Tags
- [ ] TaxRates
- [ ] UserSettings

---

## Phase 2 — EF Core infrastructure

- [ ] `IAuditable` interface + `SaveChangesInterceptor` to auto-set `CreatedAt/CreatedBy/UpdatedAt/UpdatedBy`
- [ ] `ISoftDeletable` interface + interceptor to set `DeletedAt/DeletedBy` on delete (convert `Remove` → soft delete)
- [ ] **Global query filter** for `ISoftDeletable` (skip join tables)
- [ ] **Optimistic concurrency**: use Postgres `xmin` as shadow row-version property on entities edited by multiple users (zero app-logic overhead, native PG); map `DbUpdateConcurrencyException` → `ConflictException` in global handler
- [ ] Wrap multi-entity writes in transactions (order placement, event registration, loyalty redemption)
- [ ] Review every generated migration before applying

---

## Phase 3 — Auth, identity, permissions

Identity tables: 4 only (`asp_net_users`, `asp_net_roles`, `asp_net_user_roles`, `asp_net_role_claims`). Roles are **dynamic, per-company**; claims attached to roles. User / role / role-claim CRUD lives in the **Auth feature use cases** brainstormed in Phase 1 — this phase is cross-cutting infra + data-model decisions.

### Data model

- [ ] `AppUser.AccountType` enum on `AppUser`: `SuperAdmin` | `Staff` | `Customer` (single source of truth)
- [ ] `Customer` entity — 1:1 with `AppUser` (AccountType = Customer); holds customer profile (loyalty link, preferences, etc.)
- [ ] `StaffMember` entity — 1:1 with `AppUser` (AccountType = Staff); FK to `Company`, attached to dynamic roles
- [ ] `AppRole` scoped per-company (add `CompanyId` FK; null = global, only used by the SuperAdmin flow if at all)
- [ ] Auto-seed an **`Owner`** role with full claims when a new company is registered (creator of the company → assigned Owner)
- [ ] Company entity — brainstorm proper fields (legal name, tax id, address, billing/contact, timezone, currency, logo, etc.)

### JWT claim design

- [ ] `accountType` — `SuperAdmin` / `Staff` / `Customer`
- [ ] `companyId` — Staff only
- [ ] `permission` — Staff only, multi-valued, flattened from role-claims at login
- [ ] `sub` — user id

### Policies (two layers)

- [ ] **Type-gate policies**: `RequireCustomer`, `RequireStaff`, `RequireSuperAdmin`, `RequireStaffOrSuperAdmin`
- [ ] **Permission policies** (Staff granularity): `products:write`, `products:delete`, `orders:refund`, `events:manage`, … — each passes if SuperAdmin, OR Staff with matching `permission` claim
- [ ] Single custom `PermissionRequirement` + handler to evaluate permission policies uniformly

### Endpoint scoping (flat routes, gated by policies)

- [ ] Customer-only endpoints → `RequireCustomer` (`POST /api/my/orders`, favorites, loyalty redeem, …)
- [ ] Staff config endpoints → permission policy (`DELETE /api/products/{id}` → `products:delete`)
- [ ] SuperAdmin-only → `RequireSuperAdmin` (`POST /api/companies`, cross-company ops)
- [ ] Shared endpoints (e.g. `GET /api/products`) — `RequireAuthorization()`; handler adapts response by `accountType`
- [ ] **Company isolation**: every Staff query must filter by `user.CompanyId` from JWT — enforce in handlers / query helper

### Auth use cases (lives in Phase 1 Auth brainstorm; cross-ref here)

- [ ] Single `/auth/login` endpoint (no `/admin` vs `/mobile` split — JWT carries `accountType`)
- [ ] `POST /auth/register/customer` — self-serve, creates `AppUser` + `Customer`
- [ ] `POST /auth/register/staff` — Owner/SuperAdmin creates staff in their company, creates `AppUser` + `StaffMember`
- [ ] No email verification, no refresh tokens (skipped for scope)

### Seeding

- [ ] **Startup seed**: SuperAdmin account (email + initial password from config / env var, never hardcoded; idempotent — only created if no SuperAdmin exists)
- [ ] Seed demo company + Owner user for dev/demo
- [ ] On `POST /api/companies` (SuperAdmin): auto-create default `Owner` role with full claims

### SuperAdmin company-context switching

- [ ] SuperAdmin frontend lists all companies; picks one to work on (context switcher)
- [ ] Request carries `X-Company-Id` header (or company id re-baked into JWT on switch)
- [ ] Server: if `accountType = SuperAdmin`, scope queries to selected context company instead of `companyId` claim
- [ ] Owner-style endpoints (products, outlets, events) work normally for the selected context
- [ ] Promotion path `POST /api/superadmins` (SuperAdmin-only) — optional, skip for thesis scope unless needed
- [ ] **Thesis note**: in real SaaS, silent cross-tenant impersonation has GDPR / contract implications (needs audit logging, tenant consent, etc.). For this thesis it's acceptable as a scoped simplification — mention as a compliance consideration in the thesis documentation and log SuperAdmin actions for audit.

### Other

- [ ] `ClaimsPrincipal` extensions: `UserId`, `AccountType`, `CompanyId`, `HasPermission(string)`
- [ ] Brainstorm **resource-based authorization** (user A can't mutate user B's order / favorite / loyalty via passed IDs) — options: `IAuthorizationService` + handlers, endpoint filters, or inline ownership check; decide per resource (Orders, Favorites, UserSettings, Loyalty, …)

---

## Phase 4 — Implement use cases

- [ ] Implement every use case from Phase 1, feature by feature
- [ ] Apply filter/sort/paginate convention consistently on list endpoints
- [ ] FluentValidation on every command/request
- [ ] Domain exceptions only (no `Result<T>`)
- [ ] Each use case = one file (route + request + command + handler + validator + response)
- [ ] **Per feature**: add indexes for every sortable + filterable column; composite index where filter + sort combine (e.g., `(outlet_id, created_at DESC)`); always include `deleted_at` in soft-deletable tables (+ migration per feature)
- [ ] **Per feature**: define sort whitelist (mandatory — never pass raw user input to `OrderBy`) and filter record colocated with the list use case

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

## Phase 6 — Tests

- [ ] NUnit + `WebApplicationFactory` + InMemory DB (unique GUID per test)
- [ ] JWT test-token helper
- [ ] **API test per endpoint** — happy path + key failure cases
- [ ] **3 end-to-end tests**, one per closed business process (order / event / loyalty)
- [ ] No mocking unless unavoidable

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
