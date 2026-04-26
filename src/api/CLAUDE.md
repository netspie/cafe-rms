# 🛑 ABSOLUTE RULES — READ FIRST, EVERY TURN 🛑

> **DO NOT submit this file to the university portal. Remove all CLAUDE.md files before submission.**

## 1. NEVER ACT WITHOUT AN EXPLICIT COMMAND.

**An explicit command is the user telling you to perform a specific action**: `do X`, `fix Y`, `add Z`, `commit`, `push`, `run the tests`, `refactor X to Y`, `delete Z`.

**THE FOLLOWING ARE NOT COMMANDS. ANSWER. DO NOT ACT.**

- **Questions** — "Why is this here?" / "What for?" / "How does X work?" / "Did you fuck this up?" / "Isn't this wrong?" / "Doesn't this look weird?"
- **Concerns** — "I'm worried that…" / "What if X breaks later?"
- **Observations** — "This looks heavy." / "Interesting that X." / "This could grow."
- **Musings** — "Maybe we should…" / "I wonder if…"
- **Frustrated / accusatory tone** — "Did you fucking do this?!" / "Why the fuck is X here?!" / "Isn't this fucked up?". **Tone is not a directive. Answer plainly.**

**If you cannot tell whether something is an explicit command, ASK BACK in one short sentence. DO NOT ASSUME.**

## 2. NEVER COMMIT WITHOUT AN EXPLICIT COMMAND.

`commit`, `push`, `commit and X`, `push and X` = **ONE-SHOT.** Once, then stop. `push from now on always` (or equivalent) = continuous mode until `stop pushing` / `ask before pushing`. **NO spontaneous commits.**

## 3. NEVER PUSH WITHOUT AN EXPLICIT COMMAND. SAME RULES AS COMMIT.

## 4. NEVER CHAIN `build && commit && push` IN ONE BASH CALL.

Run separately. Read each result.

## 5. NON-TRIVIAL WORK (more than 1–2 subtasks) GOES INTO `docs/todo.md` FIRST.

Draft the bullets. Reach alignment. Then write code.

---

# CLAUDE.md — CafeRMS API

API-specific conventions. The root `CLAUDE.md` has shared monorepo rules — read both. The Absolute Rules above always apply.

---

## Tech Stack

| Layer | Choice |
|---|---|
| Language | C# (.NET 10, latest) |
| Framework | ASP.NET Core Minimal API |
| ORM | EF Core |
| Database | PostgreSQL |
| Auth | ASP.NET Core Identity + JWT (self-hosted, no external provider) |
| Testing | NUnit, API/integration tests only |
| Docs | Scalar |

---

## Architecture: Vertical Slices + CQRS

**Core rule: everything that changes together lives together.**

### Folder structure

```
CafeRMS.Api/
  Features/
    Auth/
      Controllers/                 ← controllers live INSIDE the feature, next to use cases
        LoginController.cs         ← one controller per endpoint (vertical slice)
        RegisterGuestController.cs
        ...
      UseCases/
        Login.cs
        ...
      AppUser.cs
      AppRole.cs
      JwtOptions.cs
      JwtTokenService.cs
      ...
    Companies/
      Company.cs
      CompanyConfiguration.cs
      Controllers/
        CreateCompanyController.cs
        ...
      UseCases/
        CreateCompany.cs
        GetCompanyById.cs
        GetCompanies.cs
        UpdateCompany.cs
        DeleteCompany.cs
    Products/
      Product.cs
      ProductConfiguration.cs
      Controllers/
        AddProductController.cs
        ...
      UseCases/
        AddProduct.cs
        ...
    <NextFeature>/
      ...
  Persistence/
    AppDbContext.cs
    Migrations/
  Infrastructure/
    GlobalExceptionHandler.cs
  Shared/
    ClaimsPrincipalExtensions.cs
    ValidationFilter.cs
    Entities/
      Entity.cs
      ...
    Errors/
      DomainException.cs
      NotFoundException.cs
  Program.cs
CafeRMS.Api.Tests/
  ApiFactory.cs
  Features/
    Companies/
      CompanyTests.cs
CafeRMS.Api.slnx
```

### Use case file anatomy

**Two layers, deliberately split:**

- **Controller layer (transport)** — `Controllers/<Feature>Controller.cs` holds the HTTP-facing types: `Request`, `Response`, `Validator`. These are API/wire-shape concerns.
- **Use case layer (domain)** — `Features/<Feature>/UseCases/<UseCase>.cs` holds `Command`, `Result`, `Execute`. These are business-logic concerns. Command can include context fields not on the Request (e.g., `CreatedByUserId` from claims, `CompanyId` from the SuperAdmin header).

The controller's job: take the Request, assemble the Command from Request + context, call `Execute`, map the Result onto the Response.

```csharp
// Features/Auth/UseCases/Login.cs
public static class Login
{
    public sealed record Command(string Email, string Password);
    public sealed record Result(string AccessToken, DateTimeOffset ExpiresAt, string AccountType);

    public static async Task<Result> Execute(
        Command command,
        UserManager<AppUser> userManager,
        JwtTokenService tokenService) { ... }
}

// Features/Auth/Controllers/LoginController.cs
[ApiController]
public sealed class LoginController : ControllerBase
{
    [HttpPost("/api/auth/login")]
    [AllowAnonymous]
    public async Task<LoginResponse> Handle(
        [FromBody] LoginRequest request,
        [FromServices] UserManager<AppUser> userManager,
        [FromServices] JwtTokenService tokenService)
    {
        var command = new Login.Command(request.Email, request.Password);
        var result = await Login.Execute(command, userManager, tokenService);
        return new LoginResponse(result.AccessToken, result.ExpiresAt, result.AccountType);
    }
}

public sealed record LoginRequest(string Email, string Password);
public sealed record LoginResponse(string AccessToken, DateTimeOffset ExpiresAt, string AccountType);

public sealed class LoginValidator : AbstractValidator<LoginRequest> { ... }
```

Rules:
- **One controller per endpoint** (vertical slice). File name = the controller class name = `<UseCase>Controller`. Each controller has a single `Handle` action with the route declared inline via `[HttpXxx("/api/...")]`. No shared `[Route(...)]` at class level — each endpoint declares its own full path.
- **Controllers live inside their feature folder** at `Features/<Feature>/Controllers/`. Sit next to `UseCases/` and the entity files — everything that changes together lives together.
- Controllers carry **no business logic** — they only validate (via `ValidationFilter` + `AbstractValidator<Request>`), route, assemble the Command, and dispatch.
- **Validator binds to `Request`, not `Command`** — `ValidationFilter` triggers on the action parameter type. Command-level invariants are rare since context fields come from the already-validated JWT.
- For trivial cases (Login), `Command`/`Result` look identical to `Request`/`Response`. Keep them separate anyway — the distinction is the convention.
- Auth-flow failures (e.g. wrong password) throw built-in `System.UnauthorizedAccessException` — mapped to 401 in `GlobalExceptionHandler`. Don't invent a custom `UnauthorizedException`; it's a transport concern, not a business-domain rule.
- File names = the action. No `Command`, `Query`, `Handler` suffixes.
- No MediatR / `IRequest<T>` machinery — static `Execute(...)` covers it for now. Revisit only if a cross-cutting behavior (transaction wrapping, logging) needs to wrap every handler.

---

## Database — PostgreSQL via EF Core

- EF Core entity configurations next to the entity in the feature folder (`CompanyConfiguration.cs`)
- Migrations via `dotnet ef migrations add` — always check generated migration before applying
- Stored procedures, views, and functions are REQUIRED (university requirement) — use for reporting and complex queries
- Prefer EF Core for standard CRUD, raw SQL / stored procs for reporting and complex aggregations
- snake_case for table and column names (EFCore.NamingConventions)
- `HasMaxLength` and `IsRequired` on all strings
- `HasPrecision(18, 2)` on all decimals
- Cascade delete on all FKs
- `ApplyConfigurationsFromAssembly` in DbContext
- All real entities have audit fields: `CreatedAt` + `CreatedBy`, `UpdatedAt` + `UpdatedBy`, `DeletedAt` + `DeletedBy`
- Soft delete via `DeletedAt != null` with EF global query filter (not on join tables)

---

## Entity Conventions

- All entities are classes with private parameterless constructor (for EF)
- `private init` for: Id, FKs, nav properties, timestamps, immutable fields
- `private set` for: mutable fields (Name, Description, Price, etc.)
- Nullable nav properties (`Company? Company`) — never use `= null!`
- Static `Create(...)` factory method on each entity
- All IDs are client-side `Guid.NewGuid()`
- All timestamps are `DateTimeOffset.UtcNow`
- String fields get `= ""` default for EF

---

## Error Handling

**Domain exceptions, not result types.**

- Throw typed domain exceptions: `NotFoundException`, `DomainException`, etc.
- Catch and map to `ProblemDetails` in global exception middleware
- No `Result<T>`, no `OneOf`, no `Either`

---

## Authentication & Authorization

- **Internal identity** — ASP.NET Core Identity with JWT tokens (no external auth provider)
- Register/Login endpoints issue JWTs
- Route group: `app.MapGroup("/api").RequireAuthorization().AddEndpointFilter<ValidationFilter>()`
- Access user ID via `ClaimsPrincipal` extension: `user.UserId`
- Role-based authorization where needed

---

## Pagination + sort + filter on list endpoints

Every list endpoint follows the same convention: `?page=1&pageSize=20&sort=name,-createdAt&...filterField=value`. Plumbing lives in `Shared/Paging/`:

- **`PagedQuery`** — record with `Page` (default 1), `PageSize` (default 20, max 100), `Sort` (nullable string). Inherit it to add per-feature filter fields.
- **`PagedResult<T>`** — `{ Items, Page, PageSize, Total }`.
- **`IQueryable<T>.ApplySort(string sortExpression, SortMap<T>)`** — parses the `?sort=` expression and applies `OrderBy` / `ThenBy` typed via `SortMap.Add<TKey>(name, selector)`. Mandatory allow-list — unknown fields are rejected, raw user input never reaches `OrderBy`.
- **`IQueryable<T>.ToPagedResultAsync(PagedQuery)`** — clamps page/pageSize, runs `Count` + `Skip/Take`, builds `PagedResult<T>`.

### Worked example

```csharp
// Features/Products/UseCases/ListProducts.cs
public static class ListProducts
{
    // Inherit PagedQuery for the Page/PageSize/Sort init properties; add feature-specific
    // filters in the body. Don't use a positional record for the derived Query — positional
    // params can't initialize *inherited* properties (CS8907 "parameter is unread"), so the
    // base Page/PageSize/Sort would silently stay at their defaults.
    public sealed record Query : PagedQuery
    {
        public string? Name { get; init; }
    }

    public sealed record Item(Guid Id, string Name, string? Barcode, decimal Vat);

    public static async Task<PagedResult<Item>> Execute(Query query, AppDbContext db)
    {
        var sortable = new SortMap<Product>()
            .Add("name", x => x.Name)
            .Add("createdAt", x => x.CreatedAt)
            .Add("barcode", x => x.Barcode);

        var queryable = db.Products.AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Name))
            queryable = queryable.Where(x => x.Name.Contains(query.Name));

        return await queryable
            .ApplySort(query.Sort, sortable)
            .Select(x => new Item(x.Id, x.Name, x.Barcode, x.TaxRate!.Rate))
            .ToPagedResultAsync(query);
    }
}

// Features/Products/Controllers/ListProductsController.cs
[ApiController]
public sealed class ListProductsController : ControllerBase
{
    [HttpGet("/api/products")]
    [Authorize(Policy = Permissions.ProductsManage)]
    public Task<PagedResult<ListProducts.Item>> Handle(
        [FromQuery] ListProductsRequest request,
        [FromServices] AppDbContext db) =>
        ListProducts.Execute(
            new ListProducts.Query
            {
                Page = request.Page,
                PageSize = request.PageSize,
                Sort = request.Sort,
                Name = request.Name
            },
            db);
}

public sealed record ListProductsRequest(int Page = 1, int PageSize = 20, string? Sort = null, string? Name = null);
```

Per-feature checklist for every list endpoint:

- Define a `Query` record inheriting `PagedQuery` with feature-specific filter fields.
- Build a `SortMap<TEntity>` allow-listing every sortable column (typed selector — no `object` boxing).
- Apply filters → `ApplySort` → projection → `ToPagedResultAsync`.
- Add DB indexes for every filterable + sortable column; composite index where filter and sort combine (e.g. `(outlet_id, created_at DESC)` on Order).

---

## Code Conventions

- **Pragmatic over perfect.** No abstractions for hypothetical future needs.
- Records for commands, queries, and DTOs.
- `async/await` everywhere database or I/O is involved.
- No `var` when the type isn't obvious from the right-hand side.
- Lambda parameters: use `x`, `y`, `z` for standard projections/predicates, descriptive for complex lambdas.
- Minimal comments — only for non-obvious business logic.
- No XML doc comments (`///`) unless explicitly asked.
- Never use `= null!` or `= default!` in domain/application code. Exception: test fixtures.
- Use modern C# 14 / .NET 10 syntax: collection expressions, primary constructors, pattern matching, file-scoped namespaces, etc.
- FluentValidation for input validation. Validators bind to **Request** types (controller layer), not Command. `ValidationFilter` triggers automatically on the action parameter type.
- **Surrounding whitespace on JSON-string inputs is auto-trimmed** by `Shared/Json/TrimmingStringConverter` (registered globally on `AddControllers().AddJsonOptions(...)`). Use cases never see padded strings — `"  Coffee  "` arrives as `"Coffee"`. Empty / whitespace-only strings are still rejected by `NotEmpty()` in validators (FluentValidation treats whitespace as empty).
- `ProblemDetails` for all error responses.
- **Boolean names start with `Is` / `Has` / `Can` / `Should` / `Will` / `Does`** and use an affirmative phrase. Applies to properties, fields, parameters, and locals. This follows the Microsoft .NET Framework Design Guidelines (["Names of Type Members"](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/names-of-type-members#names-of-properties)).
  ```csharp
  // ✔
  public bool IsClosed => ClosedAt is not null;
  var isDescending = field.StartsWith('-');
  bool HasChildren(Node n) => n.Children.Count > 0;

  // ✘ banned — missing prefix / negative phrasing
  public bool Closed;
  var descending = field.StartsWith('-');
  public bool CantSeek;  // use !CanSeek instead
  ```
- **Never write single-line `if` / `foreach` / `while` statements.** The body always goes on its own line. For a **single** statement body, **omit the braces** — body on its own indented line. Only add braces when the body has two or more statements.
  ```csharp
  // ✔ single statement — no braces, body on its own line
  if (string.IsNullOrWhiteSpace(value))
      return query;

  // ✘ banned — single line
  if (string.IsNullOrWhiteSpace(value)) return query;

  // ✘ banned — braces around a single statement
  if (string.IsNullOrWhiteSpace(value))
  {
      return query;
  }
  ```
- **Logical operators `&&` / `||` go at the END of the previous line on multi-line expressions.** Other operators (`??`, ternary `?` / `:`, arithmetic, etc.) go at the **START** of the next line — same convention as the existing `?? throw` and conditional-expression style.
  ```csharp
  // ✔ logical && / || — at end of line
  if (user.AccountType == AccountType.SuperAdmin ||
      user.IsInRole(SystemRoles.Owner) ||
      user.HasPermission(requirement.Permission))
      context.Succeed(requirement);

  // ✔ ?? / ?: — at start of next line
  var id = user.FindFirstValue(ClaimTypes.NameIdentifier)
      ?? throw new UnauthorizedAccessException("...");

  var label = isOpen
      ? "Open"
      : "Closed";

  // ✘ banned — && / || at start
  if (user.AccountType == AccountType.SuperAdmin
      || user.IsInRole(SystemRoles.Owner))
      context.Succeed(requirement);

  // ✘ banned — ?? at end
  var id = user.FindFirstValue(ClaimTypes.NameIdentifier) ??
      throw new UnauthorizedAccessException("...");
  ```

---

## Testing

- **NUnit only**, integration/API tests — no unit tests for now
- `WebApplicationFactory` with InMemory DB (unique GUID name per test)
- Authenticate via JWT in tests (helper to generate test tokens)
- Philosophy: "add thing → get thing" — happy paths first, then key failure cases
- No mocking unless absolutely unavoidable

---

## Common Commands

Run from the repo root unless noted otherwise:

```bash
dotnet build src/api/CafeRMS.slnx                     # build both projects
dotnet run --project src/api/CafeRMS.Api               # run the API
dotnet test src/api/CafeRMS.Api.Tests                 # run the API tests
dotnet ef migrations add <Name> --project src/api/CafeRMS.Api
dotnet ef database update --project src/api/CafeRMS.Api
```

---

## Rules — What Claude must never do without asking

- Do not add NuGet packages without mentioning it first
- Do not create abstractions (interfaces, base classes) for things with only one implementation
- Do not split a use case across multiple files — keep the slice whole
- Do not add error handling for things that cannot happen
- Do not scaffold entities or DB tables that have no feature driving them yet
