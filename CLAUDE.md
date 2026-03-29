# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Cafe RMS is a Restaurant Management System API built with .NET 10.0 using ASP.NET Core minimal APIs. Early-stage project with domain models defined and API endpoints yet to be implemented.

## Common Commands

```bash
# Run the application
dotnet run --project src/CafeRMS.Api

# Build
dotnet build

# Run tests (once test project exists)
dotnet test
```

The API runs on `http://localhost:5179` (HTTP) or `https://localhost:7039` (HTTPS) in development.

## Architecture

**Single project:** `src/CafeRMS.Api/` — no separate layers yet.

**Feature-based organization:** `Features/` contains one file per domain entity. `Classes.cs` holds 30+ domain model definitions spanning the full domain:
- Organization: `Company`, `Outlet`, `OutletArea`, `Table`
- Users & Auth: `User`, `Role`, `RoleClaim`, `UserRole`
- Products: `Product`, `ProductPrice`, `ProductImage`, `Tag`, `Allergen`, `ModifierGroup`, `Modifier`
- Orders: `Order`, `OrderLine`
- Sales config: `SalesChannel`, `PriceGroup`, `PromotionCode`
- Events: `Event`, `EventDay`, `ProductList`, `ProductListItem`
- Loyalty: `LoyaltyPointLog`

**Model conventions:**
- `Id` uses `init` accessor (immutable after construction)
- Constructor-based initialization for required fields
- No ORM/database context exists yet — models are POCO only

**API layer:** Minimal APIs in `Program.cs`. Only a placeholder `/weatherforecast` endpoint exists — all real endpoints are yet to be added.

**OpenAPI:** `Microsoft.AspNetCore.OpenApi` is included; Swagger UI is available in development.

## Tech Stack

- .NET 10.0, ASP.NET Core minimal APIs
- Nullable reference types and implicit usings enabled
- `CafeRMS.Api.http` — VS Code REST Client file for manual endpoint testing
