# Thesis Defense Cheatsheet

> Personal study notes. Not for submission. Topics here are the "sus" / hard-to-defend places — rehearse the answers before defense so you don't fumble at the whiteboard.

---

## 1. PagedQuery + SortMap (pagination + sorting plumbing)

**Where:** `src/api/CafeRMS.Api/Shared/Paging/`

**What it is:** A small set of helpers used by every list endpoint:
- `PagedQuery` — record with `Page`, `PageSize`, `Sort`. Inherit it to add per-feature filters.
- `PagedResult<T>` — `{ Items, Page, PageSize, Total }`.
- `SortMap<T>` — allow-list of sortable columns: `.Add("name", x => x.Name)`. Maps a sort string to a strongly-typed selector.
- `IQueryable<T>.ApplySort(sortString, sortMap)` — parses `"-createdAt,name"` and applies `OrderByDescending().ThenBy()`.
- `IQueryable<T>.ToPagedResultAsync(query)` — clamps page/pageSize, runs `Count` + `Skip/Take`.

**Why it exists:**
- Same convention everywhere — every list endpoint takes `?page=&pageSize=&sort=&filter=`.
- **Security**: `ApplySort` is an allow-list. Without it, `?sort=Password` could leak ordering signals. Raw user input never reaches `OrderBy`.

**Likely question — "Why a SortMap and not just a Dictionary?"**
> Because the values are typed selectors (`Expression<Func<T, TKey>>`) — different columns have different key types. A `Dictionary<string, object>` would force boxing and lose type info at `OrderBy` time.

**Likely question — "Why inherit `PagedQuery` instead of using a positional record?"**
> CS8907: positional record params can't initialize *inherited* properties. The base `Page/PageSize/Sort` would silently stay at defaults. Init-only properties on the derived record make it work.

---

## 2. Zustand state management (mobile)

**Where:** `src/mobile/stores/{authStore,orderStore}.ts`

**What it is:** Tiny third-party state library (~1 KB). One `create()` call returns a hook + a `getState()` accessor. Components subscribe to slices via the hook, get re-rendered when their slice changes.

```ts
const useOrderStore = create((set, get) => ({
  lines: [],
  add: (line) => set((s) => ({ lines: [...s.lines, line] })),
  totalCount: () => get().lines.reduce((a, l) => a + l.quantity, 0),
}));

// in a component
const lines = useOrderStore((s) => s.lines);  // subscribes only to lines
```

**Why we use it (the real defense):**
- Order/cart state lives across 4 screens (menu list badge, menu detail "Add", order page, checkout). One source of truth, no prop drilling.
- Survives screen unmounts during navigation. `useState` would reset.
- Auth store: token persistence (read once on app start, available everywhere via `useAuthStore.getState().token` from non-React code like `lib/api.ts`).

**Likely question — "Why not React Context?"**
> Context re-renders all consumers when any slice changes. Zustand selectors only re-render the component that reads that exact slice. For something edited frequently (cart lines), context would cause unnecessary re-renders across the app.

**Likely question — "Why not just hit the backend each time?"**
> The order/cart is in-progress, not yet POSTed. We chose batch placement (one `POST /api/my/orders` with all lines) over a server-side draft model — saves a `DraftOrder` entity and 3-4 endpoints. Tradeoff: cart only lives client-side.

---

## 3. Soft-delete reflection scan

**Where:** `src/api/CafeRMS.Api/Persistence/AppDbContext.cs` → `OnModelCreating` → `ApplySoftDeleteFilter<T>`

**What it does:**
```csharp
foreach (var entityType in builder.Model.GetEntityTypes()) {
    if (!typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType)) continue;
    softDeleteFilter.MakeGenericMethod(entityType.ClrType).Invoke(null, [builder]);
}

private static void ApplySoftDeleteFilter<T>(ModelBuilder builder) where T : class, ISoftDeletable =>
    builder.Entity<T>().HasQueryFilter(e => e.DeletedAt == null);
```

It walks the EF model, finds every entity that implements `ISoftDeletable`, and calls `HasQueryFilter(e => e.DeletedAt == null)` on it via `MakeGenericMethod`. The result: every soft-deletable entity automatically excludes rows with `DeletedAt != null` from queries.

**Why reflection (the honest answer):**
- 14+ business entities all need the same filter. Listing 14 explicit `builder.Entity<X>().HasQueryFilter(...)` calls would be 14 lines of repetition that drift if you add a new entity and forget.
- `HasQueryFilter` is a generic method — needs the entity type at compile time. Reflection bridges that.

**Risk in defense:** The professor might ask why this isn't explicit. Honest answer: "tradeoff — explicitness vs forgettability. Reflection ensures no entity is missed."

**Bypass:** `db.X.IgnoreQueryFilters()` to query soft-deleted rows when needed (rare, e.g., admin "show deleted" view).

**Likely question — "What if I add a new ISoftDeletable entity?"**
> Nothing else to do — the scan picks it up automatically when EF builds the model.

---

## 4. xmin concurrency tokens

**Where:** `src/api/CafeRMS.Api/Persistence/AppDbContext.cs` → `OnModelCreating` → `ApplyXminConcurrencyTokens`

**What it does:**
```csharp
foreach (var entityType in builder.Model.GetEntityTypes()) {
    if (!typeof(IAuditable).IsAssignableFrom(entityType.ClrType)) continue;
    builder.Entity(entityType.ClrType)
        .Property<uint>("xmin")
        .IsConcurrencyToken()
        .ValueGeneratedOnAddOrUpdate();
}
```

**What `xmin` is:** A PostgreSQL system column on every row holding the transaction ID that last modified it. Auto-incremented on every UPDATE. **Existed before EF — we don't add a column, just tell EF to read it.**

**Why use it as a concurrency token:**
- Optimistic concurrency: when you update a row, EF includes `WHERE id = ? AND xmin = ?` in the SQL. If someone else updated the row between your read and your write, the `xmin` won't match → 0 rows affected → EF throws `DbUpdateConcurrencyException` → 409 Conflict.
- No need for a manual `RowVersion` column or a timestamp trigger. Postgres-native.

**Likely question — "Why xmin instead of a `RowVersion` byte array?"**
> SQL Server's `RowVersion` doesn't exist in Postgres. xmin is the Postgres equivalent — system column, auto-managed, no migration needed.

**Likely question — "What does it actually defend against?"**
> Two concurrent edits on the same row. User A loads order, user B loads order, B updates first, A submits → A gets 409 instead of silently overwriting B's changes.

**Honest tradeoff to mention:** For a single-cafe single-staff workflow, conflicts are rare. Could be argued as over-engineering. Ready answer: "The cost is one EF call in `OnModelCreating` and one extra column in the WHERE clause. Compared to silently losing edits, it's cheap insurance."

---

## 5. Vertical Slices + CQRS (the architecture pattern)

**Where:** Everywhere. `Features/<Feature>/{Controllers,UseCases}/` layout.

**What it is:**
- **Vertical slice**: each feature owns its entity, configuration, controllers, use cases, request/response types. Everything that changes together lives together. No `Services/`, `Repositories/`, `DTOs/` folders.
- **CQRS** here means: each use case has a `Command` (input), `Result` (output), `Execute(...)` (the work). No MediatR — just a static method. Read paths use `Query` instead of `Command`.

**Why this and not layered (Repository / Service / Controller)?**
- Adding a feature touches one folder, not five.
- No abstraction for a single implementation. No repository interface for one EF DbContext.
- Use case is the unit of business logic, not a "service class."

**File anatomy (rehearse this — examiner will ask):**
- `Features/Products/Product.cs` — entity, factory, mutation methods
- `Features/Products/ProductConfiguration.cs` — EF config
- `Features/Products/Controllers/AddProductController.cs` — HTTP transport: Request, Response, Validator, controller
- `Features/Products/UseCases/AddProduct.cs` — Command, Result, Execute (business logic)

**Likely question — "Why is the controller a class with one method?"**
> One controller per endpoint = vertical slice. The controller is the transport layer for *that one endpoint*. No shared `[Route]` attribute, no fat controllers.

---

## Quick rapid-fire (one-liners ready)

| Question | Answer |
|---|---|
| Why no MediatR? | Static `Execute(...)` covers it. No cross-cutting behaviors needed yet. |
| Why no Result<T> / OneOf? | Throw typed domain exceptions, map to ProblemDetails in `GlobalExceptionHandler`. Simpler stack traces, no wrapping ceremony. |
| Why client-side GUIDs? | `Guid.NewGuid()` at creation time means we don't need DB round-trip to know an entity's ID. Useful for relationships set up before save. |
| Why DateTimeOffset and not DateTime? | Stores UTC offset, no ambiguity across timezones. |
| Why snake_case in DB? | `EFCore.NamingConventions` package — Postgres convention. C# stays PascalCase. |
| Why audit fields on every entity (CreatedAt/By, UpdatedAt/By)? | Cheap traceability. Set in `SaveChangesAsync` override via `IAuditable` interface. |
| Why FluentValidation and not DataAnnotations? | Better expression of cross-field rules, async rules, custom rules. Bound to `Request` via `ValidationFilter` automatically. |
| Why `TrimmingStringConverter`? | Auto-trims whitespace on every JSON string at deserialization. Use cases never see padded strings — `"  Coffee  "` arrives as `"Coffee"`. Empty/whitespace-only still rejected by `NotEmpty()`. |
| Why JWT and not cookies? | Mobile + web both consume the same API. JWT works in `Authorization` headers from any client. Cookies require same-origin. |
