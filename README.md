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

### Prerequisites

- .NET 10 SDK
- PostgreSQL

### Run

```bash
# Apply migrations
dotnet ef database update --project src/CafeRMS.Api

# Run the API
dotnet run --project src/CafeRMS.Api
```

API docs available at `/scalar` in development.

### Test

```bash
dotnet test
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
