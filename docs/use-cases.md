# Use case brainstorm (Phase 1)

Per-feature list of use cases (CRUD + business actions) and which of the 3 closed business processes each participates in. Implementation happens in Phase 4 — this is the spec.

## Legend

- **CRUD** — standard `Add / GetById / List / Update / Delete`. `List` always supports filter/sort/paginate convention from Phase 0.
- **Business action** — non-CRUD state transition or domain operation.
- **Actor** — `Customer` / `Staff` / `Owner` / `SuperAdmin`. Defaults: Staff reads + writes feature config in their company; Customer reads public catalog data + writes their own (`/my/...`) resources; SuperAdmin cross-company.
- **Process** — one of:
  - **P1 Order** — cart → place → prepare → fulfil → close → (optional) cancel
  - **P2 Event** — create → publish → register → attend → close → loyalty payout
  - **P3 Loyalty** — enroll → earn → redeem → expire

## Cross-cutting notes

- Most config entities (Products, Modifiers, PriceGroups, etc.) are **per-company**. Entities currently lacking `CompanyId` will need it added in Phase 3 / Phase 4 when the feature is implemented. Flagged per feature as **⚠ needs CompanyId**.
- Deletes are **soft** (via `ISoftDeletable` + interceptor from Phase 2). "Delete" below means soft delete unless noted.
- Customer-facing list endpoints return a trimmed DTO (no audit fields, no cost/internal fields).
- Authorization enforcement is Phase 3 infra — referenced here as intent, not implementation.

---

## Allergens

Small reference list used to tag products with allergen info. Company-scoped.

⚠ needs CompanyId.

- **CRUD** — Staff (`ManageProducts` permission, lives in same concern area).
- **Business actions** — none.
- **Customer-visible** — Allergens appear embedded in Product detail; no separate customer endpoint.
- **Process** — supports P1 (informational; no state transitions).

---

## Auth

Authentication, user/role/permission management, self-registration.

### Use cases

- `POST /auth/login` — email + password → JWT. Single endpoint for all account types.
- `POST /auth/register/customer` — self-serve. Creates `AppUser` + `Customer` profile.
- `POST /auth/register/staff` — Owner/SuperAdmin creates staff in a company. Creates `AppUser` + `StaffMember`, assigns role(s).
- `POST /auth/change-password` — authenticated user changes own password.
- `GET /auth/me` — returns current user profile (name, email, accountType, companyId, permissions).

### Role management (dynamic, per-company)

- `POST /api/roles` — Owner creates a role in their company with chosen claims (permission list).
- `GET /api/roles` — list roles in own company (Owner / anyone with `ManageRoles`).
- `GET /api/roles/{id}` — role detail with claims.
- `PUT /api/roles/{id}` — rename + update claims.
- `DELETE /api/roles/{id}` — delete (fail if any staff still assigned).
- `POST /api/roles/{id}/users` — assign a staff member to a role.
- `DELETE /api/roles/{id}/users/{userId}` — remove assignment.
- `GET /api/permissions` — list known permission strings (static, from `Permissions.cs`).

### Staff management

- `GET /api/users` — list staff in own company.
- `GET /api/users/{id}` — detail.
- `PUT /api/users/{id}` — update name, email, assigned roles.
- `POST /api/users/{id}/deactivate` — block login without deleting.
- `POST /api/users/{id}/reactivate`.
- `DELETE /api/users/{id}` — soft delete (Identity user flag).

### SuperAdmin

- `POST /api/companies` — create company (SuperAdmin only). Auto-seeds `Owner` role + first Owner user.
- SuperAdmin context switch handled by `X-Company-Id` header (Phase 3 decision).

### Business actions

- `RegisterCustomer`, `RegisterStaff`, `Login`, `ChangePassword`, `AssignRole`, `DeactivateUser` — all non-CRUD transitions.

### Process

- Foundational for all three processes (identity is a prerequisite).

---

## Companies

Top-level tenant. SuperAdmin-only CRUD; Owner can read + update their own.

- **CRUD**
  - `POST /api/companies` — SuperAdmin only; seeds Owner role + user.
  - `GET /api/companies` — SuperAdmin only; cross-tenant list.
  - `GET /api/companies/{id}` — SuperAdmin cross-tenant; Owner for own company.
  - `PUT /api/companies/{id}` — SuperAdmin or Owner.
  - `DELETE /api/companies/{id}` — SuperAdmin only. Cascades all company-scoped data (soft).
- **Business actions** — none beyond CRUD (registration of Owner happens inside `POST`).
- **Process** — prerequisite for all three.

---

## Events

Cafe events (workshops, concerts, themed nights). An event has days and optional overrides for menu (ProductList) and pricing (PriceGroup). During event days, customers at the outlet can order from either the event menu or the standard menu.

⚠ needs CompanyId (or scope via Outlet — decision needed).

### CRUD

- `POST /api/events` — Staff (`ManageEvents`). Sets name, description, image, optional ProductList + PriceGroup.
- `GET /api/events` — Staff sees all; Customer sees published + currently-or-soon-running.
- `GET /api/events/{id}` — shape depends on actor.
- `PUT /api/events/{id}` — Staff.
- `DELETE /api/events/{id}` — Staff.

### Event days (sub-resource)

- `POST /api/events/{id}/days` — add a date.
- `DELETE /api/events/{id}/days/{dayId}` — remove a date.
- `GET /api/events/{id}/days` — list dates.

### Business actions

- `PublishEvent` — `POST /api/events/{id}/publish` — makes event visible to customers. (Open question: do we need a draft vs published flag on the entity? Currently no `IsPublished` field — would need to add.)
- `CancelEvent` — `POST /api/events/{id}/cancel` with reason — cancels all days, notifies registrations (notification out of scope).
- `CloseEvent` — `POST /api/events/{id}/close` — finalizes, triggers loyalty payout for attendees.

### Open question

- **Event registration / attendance** — the 3-process candidate list mentions "registered, attended" fields in the Phase 5 view. But there's no `EventRegistration` entity yet. Needs design decision in Phase 3: add `EventRegistration { EventId, EventDayId, UserId, RegisteredAt, AttendedAt, CancelledAt }` entity, or skip registration entirely and treat an event as "walk-in with optional menu override"? **Recommendation: add the entity** — it's central to the P2 Event lifecycle and required by report 2 (attendance).

### Customer use cases (assuming registration)

- `POST /api/my/event-registrations` — register self for event on specific day.
- `DELETE /api/my/event-registrations/{id}` — cancel own registration.
- `GET /api/my/event-registrations` — list own.

### Staff use cases (assuming registration)

- `GET /api/events/{id}/registrations` — list registrations for event.
- `POST /api/events/{id}/registrations/{regId}/check-in` — mark attended (business action).

### Process

- **P2 Event** — core. Also links to P1 (event orders carry `EventId`) and P3 (attendance earns points).

---

## Favorites

Customer bookmarks a product.

- `POST /api/my/favorites` — add product to favorites.
- `DELETE /api/my/favorites/{productId}` — remove.
- `GET /api/my/favorites` — list own favorites (with product detail).
- **Authorization** — Customer only, own only.
- **Business actions** — none.
- **Process** — supports P1 (drives customer UX toward re-ordering, no state transitions).

---

## Loyalty

Event-sourced point log (no materialized balance). Balance = sum of `LoyaltyPointLog.Points` filtered by user.

### CRUD (admin-only)

- `GET /api/loyalty/entries` — Staff with `ManageLoyalty`; filter by userId, dateRange.
- `GET /api/loyalty/entries/{id}`.
- `POST /api/loyalty/entries` — manual adjustment (Staff, with required reason). Positive or negative.
- (No Update / Delete on log entries — immutable history. Corrections are reverse entries.)

### Customer use cases

- `GET /api/my/loyalty/balance` — sum of own log.
- `GET /api/my/loyalty/history` — list own entries.

### Business actions (internal / triggered)

- `EarnPointsFromOrder` — triggered when order closes (emits positive `LoyaltyPointLog`). Fires from OrderLifecycle close handler, not a direct endpoint.
- `RedeemPoints` — triggered inside `PlaceOrder` when `loyaltyPointsUsed > 0` (emits negative log entry, transactional with order creation).
- `EarnPointsFromEventAttendance` — triggered when event closes.
- `ExpireStalePoints` — background job (out of scope for thesis? **Flag for decision** — could be a scheduled SQL stored proc that writes negative entries).

### Process

- **P3 Loyalty** — core. Also triggered by P1 (orders) and P2 (events).

### Open question

- Tiers mentioned in the report 3 view (`tier`). No tier entity exists. Either derive tier from lifetime points via function (`fn_loyalty_tier(user_id)`), or add a `LoyaltyTier` config table. **Recommendation: derive via function** (simpler, thesis-appropriate, exercises the "DB functions" requirement).

---

## ModifierGroups

Grouping for modifiers (e.g., "Milk type", "Size"). Attached to products via `ProductModifierGroup`.

⚠ needs CompanyId.

- **CRUD** — Staff (`ManageModifiers`).
- **Business actions** — none.
- **Customer-visible** — embedded in Product detail for ordering.
- **Process** — supports P1.

---

## Modifiers

Individual modifier options within a group (e.g., "Oat milk" in "Milk type"). Each has its own price (via ProductPrice-like mechanism? **Flag**: currently no price on Modifier — need to decide).

⚠ needs CompanyId (via ModifierGroup).

- **CRUD** — Staff (`ManageModifiers`). Create always sets `ModifierGroupId`.
- **Business actions** — none.
- **Open question** — modifier pricing. Options: (a) add `Modifier.PriceDelta` as flat surcharge; (b) add a `ModifierPrice` entity like `ProductPrice` for per-PriceGroup pricing; (c) modifiers are free. **Recommendation: (a)** — flat surcharge `decimal PriceDelta`, simplest, matches typical cafe reality.
- **Also flag** — `OrderLine` currently has no link to selected modifiers. Need a `OrderLineModifier { OrderLineId, ModifierId, PriceDeltaAtOrder }` entity in Phase 4.
- **Process** — supports P1.

---

## Orders

The heart of P1. Lifecycle: create (cart) → place (submit) → prepare → fulfil → close → (optional) cancel w/ reason.

### Staff CRUD (back-office views)

- `GET /api/orders` — list (filter by outlet, sales channel, status, date range, customer, event).
- `GET /api/orders/{id}` — detail with lines + modifiers.
- (No staff `POST` / `PUT` / `DELETE` — orders come in via business actions.)

### Customer use cases

- `POST /api/my/orders` — place order (customer-facing; builds Order + OrderLines + OrderLineModifiers atomically).
- `GET /api/my/orders` — list own orders.
- `GET /api/my/orders/{id}` — own order detail.
- `POST /api/my/orders/{id}/cancel` — customer cancels own order (only if not yet `Accepted` — see state below).

### Business actions (state transitions)

Order state is derived from timestamp fields. Open question: do we need explicit status? Current model has only `ClosedAt` + `CancelledAt`. Intermediate states (Accepted, InProgress, Ready) are missing. **Recommendation: add `AcceptedAt`, `InProgressAt`, `ReadyAt`** on Order to cover prepare + fulfil phases without needing a status enum; derive via computed `OrderStatus` property.

- `PlaceOrder` — `POST /api/my/orders` — customer. Creates Order + lines; transactional; redeems loyalty points if requested; validates promo code if supplied.
- `CreateWalkInOrder` — `POST /api/orders` — Staff (POS). Creates Order for a walk-in customer (no `UserId` required), at a table.
- `AcceptOrder` — `POST /api/orders/{id}/accept` — Staff (`ManageOrders`). Sets `AcceptedAt`.
- `StartPreparing` — `POST /api/orders/{id}/start-preparing` — sets `InProgressAt`.
- `MarkReady` — `POST /api/orders/{id}/ready` — sets `ReadyAt`.
- `CloseOrder` — `POST /api/orders/{id}/close` — Staff, sets `ClosedAt`. Triggers `EarnPointsFromOrder`.
- `CancelOrder` — `POST /api/orders/{id}/cancel` — Staff or Customer (customer: only before Accepted). Requires reason.
- `AddLine` / `RemoveLine` / `UpdateLineQty` — Staff only, only while order is open (pre-Close). `POST /api/orders/{id}/lines`, etc.

### Process

- **P1 Order** — core.
- Integrates with **P2** (orders carry `EventId`, event orders may use event ProductList + PriceGroup) and **P3** (closing earns points; placing can redeem them).

### Also flag

- `PromotionCode` needs linking into Order — currently no FK. Need a `PromoCodeId` on Order (nullable) + the discount is already captured in `Order.Discount`. Decide whether to store the code reference or just the calculated discount. **Recommendation: store both** (`PromotionCodeId` FK + computed `Discount`) for reporting.

---

## Outlets

Physical cafe locations. Company-scoped (has `CompanyId`).

- **CRUD** — Staff (`ManageOutlets`), scoped to own company.
- **Business actions** — none.
- **Customer-visible** — list active outlets (so they can pick one to order from).
  - `GET /api/outlets` — customer list (name, address, currency; no audit fields).
- **Process** — prerequisite for P1 (every order has an outlet).

---

## PriceGroups

Named pricing tiers (e.g., "Standard", "Member", "Event"). Products have a price per price group via `ProductPrice`.

⚠ needs CompanyId.

- **CRUD** — Staff (`ManagePricing`).
- **Business actions** — none.
- **Process** — supports P1 (determines price on order line) and P2 (event override).

---

## PrintoutTemplates

User-uploadable Word templates for printouts. Phase 8 renders them; this feature just manages metadata + file URL.

⚠ needs CompanyId.

- **CRUD** — Staff (`ManagePrintoutTemplates`).
- **Business actions**
  - `UploadTemplate` — `POST /api/printout-templates/upload` — multipart file upload; stores file, returns URL; metadata record follows.
  - `RenderPrintout` — `POST /api/printouts/render` with `{ templateId, entityType, entityId }` — lives in Phase 8, references this feature.
- **Process** — supports P1 (order receipts), P2 (event confirmations). Cross-cutting.

---

## ProductLists

Named collections of products (menus). Referenced by Events as the override menu. Also used generally as the "standard menu" in an outlet (TBD how outlet-to-list is linked).

⚠ needs CompanyId. Also **flag**: no `Outlet → ProductList` link exists yet — standard menu per outlet is undefined. **Recommendation**: add `Outlet.DefaultProductListId` (nullable, falls back to "all active products").

- **CRUD** — Staff (`ManageMenus`).
- **Business actions**
  - `AddProductToList` — `POST /api/product-lists/{id}/items` with `{ productId }`.
  - `RemoveProductFromList` — `DELETE /api/product-lists/{id}/items/{productId}`.
- **Customer-visible** — customer browsing menu at outlet reads the relevant list.
  - `GET /api/product-lists/{id}` — returns products with pricing resolved for the current context.
- **Process** — supports P1 (menu source) and P2 (event menu override).

---

## Products

Items sold. Central catalog entity. Each product has: Name, Description, Barcode, TaxRate, plus relationships to Allergens, Tags, ModifierGroups, Images, Prices per PriceGroup.

⚠ needs CompanyId.

### CRUD

- **Staff CRUD** — `ManageProducts`.
- **Customer read** — `GET /api/products`, `GET /api/products/{id}` — returns public fields + resolved price for customer's context.

### Sub-resource management (Staff)

- Allergens — `POST/DELETE /api/products/{id}/allergens/{allergenId}`.
- Tags — `POST/DELETE /api/products/{id}/tags/{tagId}`.
- Modifier groups — `POST/DELETE /api/products/{id}/modifier-groups/{groupId}`.
- Images — `POST /api/products/{id}/images` (upload), `DELETE /api/products/{id}/images/{imageId}`, `PUT /api/products/{id}/images/{imageId}` (reorder? rename? — probably just delete + re-add).
- Prices — `PUT /api/products/{id}/prices/{priceGroupId}` with `{ net }` (upsert).

### Business actions

- None (pure config).

### Process

- Supports P1 (product catalog + pricing).

---

## PromotionCodes

Discount codes customers enter at checkout.

⚠ needs CompanyId.

- **CRUD** — Staff (`ManagePromotions`).
- **Business actions**
  - `ValidatePromotionCode` — `POST /api/promotion-codes/validate` with `{ code }` — returns `{ valid, discountPercentage }`. Used by customer client pre-submit.
  - Redemption itself happens inside `PlaceOrder` (applies percentage to order subtotal; stores on `Order.Discount` + `Order.PromotionCodeId`).
- **Open question** — usage limits (max uses, per-user uses, expiry). Currently the entity has only `Code` + `DiscountPercentage`. **Recommendation**: add `ValidFrom`, `ValidUntil` (nullable), `MaxUses` (nullable), `UsesCount`. Flag for Phase 4 decision.
- **Process** — supports P1.

---

## SalesChannels

Where an order comes from (Dine-in, Takeout, Delivery, Online). Has `IsTakeout` flag. May be linked to one or more PriceGroups via `SalesChannelPriceGroup` (channel-specific pricing).

⚠ needs CompanyId.

- **CRUD** — Staff (`ManageSalesChannels`).
- **Business actions**
  - `LinkPriceGroup` — `POST /api/sales-channels/{id}/price-groups` with `{ priceGroupId }`.
  - `UnlinkPriceGroup` — `DELETE /api/sales-channels/{id}/price-groups/{priceGroupId}`.
- **Process** — supports P1 (determines price group resolution + order metadata).

---

## Tables

Physical tables within an outlet. Outlet-scoped.

- **CRUD** — Staff (`ManageTables`).
- **Business actions** — none.
- **Customer-visible** — probably not directly; staff assigns table on walk-in order.
- **Process** — supports P1 (walk-in order → table).

---

## Tags

Product labels (e.g., "Vegan", "Spicy", "New"). Used for filtering + UI badges. Each tag has optional image URL.

⚠ needs CompanyId.

- **CRUD** — Staff (`ManageProducts` — lives in same concern area).
- **Business actions** — none.
- **Customer-visible** — embedded in Product detail; filter by tag in `GET /api/products?tag=...`.
- **Process** — supports P1.

---

## TaxRates

VAT rates applied to products. Each Product has a `TaxRateId`.

⚠ needs CompanyId.

- **CRUD** — Staff (`ManageTaxRates`).
- **Business actions** — none.
- **Process** — supports P1 (tax calculation on order lines).

---

## UserSettings

Per-user UI preferences (theme, UI JSON). 1:1 with AppUser.

- `GET /api/my/settings` — own only.
- `PUT /api/my/settings` — own only.
- (No List / Create / Delete — auto-created on first access or at user registration.)
- **Business actions** — none.
- **Process** — cross-cutting; no process participation.

---

## Summary — open questions for decision before Phase 4

1. **Event registration entity** — add `EventRegistration` for P2 attendance tracking? (Recommendation: yes.)
2. **Order status fields** — add `AcceptedAt` / `InProgressAt` / `ReadyAt` to `Order`? (Recommendation: yes.)
3. **Modifier pricing** — flat `PriceDelta` on Modifier? (Recommendation: yes.)
4. **Order-line modifiers** — add `OrderLineModifier` entity? (Recommendation: yes — otherwise modifiers on orders aren't captured.)
5. **Promotion code limits** — add `ValidFrom`, `ValidUntil`, `MaxUses`, `UsesCount`? (Recommendation: yes.)
6. **Outlet default menu** — `Outlet.DefaultProductListId`? (Recommendation: yes.)
7. **Loyalty tiers** — derive via DB function vs explicit entity? (Recommendation: function.)
8. **`IsPublished` on Event** — or is publication just "has any future EventDay"? (Needs decision.)
9. **CompanyId on config entities** — every `⚠ needs CompanyId` above; add in Phase 3 via a single cross-feature migration.
10. **Loyalty point expiry** — background job in thesis scope or out of scope? (Recommendation: scheduled stored proc as the DB-function showcase for the thesis.)

---

## Process coverage matrix

| Feature | P1 Order | P2 Event | P3 Loyalty |
|---|---|---|---|
| Allergens | read |  |  |
| Auth | prereq | prereq | prereq |
| Companies | prereq | prereq | prereq |
| Events |  | ✓ core |  |
| Favorites | support |  |  |
| Loyalty | earn/redeem | earn |  ✓ core |
| ModifierGroups | read |  |  |
| Modifiers | read |  |  |
| Orders | ✓ core | registration orders | trigger |
| Outlets | prereq | prereq |  |
| PriceGroups | support | override |  |
| PrintoutTemplates | receipts | confirmations |  |
| ProductLists | support | override |  |
| Products | ✓ core |  |  |
| PromotionCodes | support |  |  |
| SalesChannels | support |  |  |
| Tables | support |  |  |
| Tags | support |  |  |
| TaxRates | support |  |  |
| UserSettings |  |  |  |

✓ core = the feature *is* that process or a central part of it.
