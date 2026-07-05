# CafeRMS

A management system for small, themed cafes that combine food service with event hosting. Engineering thesis project (projekt inżynierski).

## System Components

| Component | Technology | Purpose |
|---|---|---|
| **API / Backend** | ASP.NET Core, PostgreSQL, EF Core | Shared backend for both clients |
| **Admin Panel** | Next.js, TypeScript | Staff: manage outlets, products, menus, events, orders |
| **Mobile App** | React Native (Expo) | Customers: browse menu, order online, loyalty, events |

Payments are **out of scope** — no transactions or payment gateway integration.

**This repository contains the API / Backend.**

## Tech Stack

- .NET 10 / C# 14 — ASP.NET Core Minimal API
- PostgreSQL via EF Core
- ASP.NET Core Identity + JWT (internal auth)
- FluentValidation
- NUnit integration tests
- Scalar for API docs

## Architecture

Vertical Slices + CQRS. Each use case is a single file containing route registration, command/query, handler, validator, and response DTO.

## Getting Started

You can run the API in two ways: **Docker Compose** (zero local setup beyond Docker) or **local .NET + Postgres**.

### Run with Docker Compose (recommended)

Brings up Postgres + API with one command. Migrations are applied and demo data is seeded on first start. The compose file lives in `src/api/`; run from the repo root with `-f`:

```bash
docker compose -f src/api/docker-compose.yml up --build -d
```

| Service | URL |
|---|---|
| API | http://localhost:5179 |
| API docs (Scalar) | http://localhost:5179/scalar |
| Postgres | localhost:5434 (`postgres` / `postgres`, db `cafe_rms`) |

Useful commands (all from the repo root):

```bash
docker compose -f src/api/docker-compose.yml up -d --build    # detached
docker compose -f src/api/docker-compose.yml logs -f api      # tail API logs
docker compose -f src/api/docker-compose.yml down             # stop, keep data
docker compose -f src/api/docker-compose.yml down -v          # stop AND wipe Postgres volume (full reset)
docker compose -f src/api/docker-compose.yml up --build api   # rebuild only the API after code changes
```

### Run locally (without Docker)

#### Prerequisites

- .NET 10 SDK
- PostgreSQL on `localhost:5434` (db `cafe_rms`, user `postgres`, password `postgres` — or override `ConnectionStrings:Default` in `appsettings.Development.json`)

#### Run

```bash
# Apply migrations
dotnet ef database update --project src/api/CafeRMS.Api

# Run the API
dotnet run --project src/api/CafeRMS.Api
```

API docs available at `/scalar` in development.

#### Test

```bash
dotnet test src/api/CafeRMS.Api.Tests
```

## Project Structure

```
src/
  CafeRMS.Api/
    Features/           # Vertical slices — one folder per domain area
      Auth/             # Register, Login (ASP.NET Core Identity + JWT)
      Companies/
      Outlets/
      Products/
      Orders/
      ...
    Persistence/        # AppDbContext + Migrations
    Infrastructure/     # Auth handlers, exception handling
    Shared/             # Cross-cutting: validation filter, extensions, errors
    Program.cs
  CafeRMS.Api.Tests/
    ApiFactory.cs
    Features/
```

## Domain Overview

| Area | Entities |
|---|---|
| Organization | Company, Outlet, OutletArea, Table |
| Auth | AppUser, AppRole (ASP.NET Core Identity) |
| Settings | UserSettings |
| Products | Product, Tag, Allergen, ProductImage, ProductPrice, ProductList |
| Modifiers | ModifierGroup, Modifier |
| Sales Config | PriceGroup, SalesChannel, TaxRate |
| Orders | Order, OrderLine, PromotionCode |
| Loyalty | LoyaltyPointLog, Favorite |
| Events | Event, EventDay |
