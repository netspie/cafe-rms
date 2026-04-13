# CLAUDE.md — CafeRMS API

> **DO NOT submit this file to the university portal. Remove all CLAUDE.md files before submission.**

API-specific conventions. The root `CLAUDE.md` has shared monorepo rules — read both.

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
    Companies/
      Company.cs
      CompanyConfiguration.cs
      UseCases/
        AddCompany.cs
        GetCompanyById.cs
        GetCompanies.cs
        UpdateCompany.cs
        DeleteCompany.cs
    Products/
      Product.cs
      ProductConfiguration.cs
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

Each file in `UseCases/` contains everything for that one use case:

```csharp
// AddProduct.cs
// 1. Minimal API route registration
// 2. Request record (route binding)
// 3. Command record (includes UserId etc.)
// 4. Static Execute() handler
// 5. FluentValidation validator
// 6. Response DTO
```

File names = the action. No `Command`, `Query`, `Handler` suffixes.

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
- FluentValidation for input validation.
- `ProblemDetails` for all error responses.

---

## Testing

- **NUnit only**, integration/API tests — no unit tests for now
- `WebApplicationFactory` with InMemory DB (unique GUID name per test)
- Authenticate via JWT in tests (helper to generate test tokens)
- Philosophy: "add thing → get thing" — happy paths first, then key failure cases
- No mocking unless absolutely unavoidable

---

## Common Commands

```bash
dotnet run --project src/CafeRMS.Api
dotnet test
dotnet build
dotnet ef migrations add <Name> --project src/CafeRMS.Api
dotnet ef database update --project src/CafeRMS.Api
```

---

## Rules — What Claude must never do without asking

- Do not add NuGet packages without mentioning it first
- Do not create abstractions (interfaces, base classes) for things with only one implementation
- Do not split a use case across multiple files — keep the slice whole
- Do not add error handling for things that cannot happen
- Do not scaffold entities or DB tables that have no feature driving them yet
