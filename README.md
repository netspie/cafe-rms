# CafeRMS

A management system for small, themed cafes that combine food service with event hosting. Engineering thesis project (projekt inżynierski).

🇵🇱 [Wersja polska](README.pl.md)

## System Components

| Component | Directory | Technology | Purpose |
|---|---|---|---|
| **API / Backend** | `src/api` | ASP.NET Core, PostgreSQL, EF Core | Shared backend for both clients |
| **Admin Panel** | `src/web` | Next.js, TypeScript | Staff: manage products, menus, events, orders, reports |
| **Mobile App** | `src/mobile` | React Native (Expo) | Customers: browse menu, order online, loyalty, events |

Payments are **out of scope** — no transactions or payment gateway integration.

## Tech Stack

**API** — .NET 10 / C# 14, ASP.NET Core (MVC controllers), PostgreSQL via EF Core, ASP.NET Core Identity + JWT, FluentValidation, QuestPDF + ClosedXML (report exports), Scalar (API docs).

**Web** — Next.js 16 (App Router, server components), React 19, TypeScript, Tailwind CSS 4, Recharts.

**Mobile** — Expo 54, expo-router, React Native 0.81, NativeWind, Zustand.

## Architecture

Vertical Slices + CQRS. Each use case is a single file containing route registration, command/query, handler, validator, and response DTO.

## Getting Started

Run the backend first — both clients need it.

### 1. Backend

Brings up Postgres + API with one command. Migrations are applied and demo data is seeded on first start.

```bash
docker compose -f src/api/docker-compose.yml up --build -d
```

| Service | URL |
|---|---|
| API | http://localhost:5179 |
| API docs (Scalar) | http://localhost:5179/scalar |
| Postgres | localhost:5434 (`postgres` / `postgres`, db `cafe_rms`) |

Other commands:

```bash
docker compose -f src/api/docker-compose.yml logs -f api      # tail API logs
docker compose -f src/api/docker-compose.yml up --build api   # rebuild only the API
docker compose -f src/api/docker-compose.yml down -v          # stop and reset the database
```

<details>
<summary>Run the API locally instead of in Docker</summary>

Requires the .NET 10 SDK and PostgreSQL on `localhost:5434` (db `cafe_rms`, user `postgres`, password `postgres` — or override `ConnectionStrings:Default` in `appsettings.Development.json`).

```bash
dotnet ef database update --project src/api/CafeRMS.Api
dotnet run --project src/api/CafeRMS.Api
```
</details>

### 2. Admin Panel (web)

```bash
cd src/web
npm install
npm run dev
```

Open http://localhost:3000. Expects the API at `http://localhost:5179`; override with `API_BASE_URL`.

### 3. Mobile App

```bash
cd src/mobile
npm install
npx expo start
```

Press `i` for the iOS simulator, `a` for Android, or scan the QR code with Expo Go. Expects the API at `http://localhost:5179`; override with `EXPO_PUBLIC_API_BASE_URL`.

> On a physical device, `localhost` points at the phone. Set `EXPO_PUBLIC_API_BASE_URL` to your machine's LAN address, e.g. `EXPO_PUBLIC_API_BASE_URL=http://192.168.1.10:5179 npx expo start`.

## Demo Accounts

Seeded automatically on first start. Password for every account: **`Demo1234`**

| Email | Role | Client | Sees |
|---|---|---|---|
| `admin@shiba.pl` | Owner | Web | Everything |
| `manager@shiba.pl` | Kierownik | Web | Everything except role management |
| `barista@shiba.pl` | Barista | Web | Orders, loyalty, products |
| `user1@shiba.pl` | Customer | Mobile | Menu, own orders, loyalty, events |
| `user2@shiba.pl` | Customer | Mobile | Menu, own orders, loyalty, events |

The admin panel hides navigation the signed-in role cannot use; the API enforces the same permissions independently.

## Project Structure

```
src/
  api/CafeRMS.Api/
    Features/           # Vertical slices — one folder per domain area
      Auth/             # Login, roles, permissions (Identity + JWT)
      Products/         # Catalog, prices, images
      Orders/           # Order placement and lifecycle
      Events/           # Events, event days, printouts
      Reports/          # Sales, sales-per-product, attendance (+ PDF/Excel)
      ...
    Persistence/        # AppDbContext, migrations, seeding
    Infrastructure/     # Auth handlers, exception handling
    Resources/          # Printout templates
    Shared/             # Cross-cutting: validation filter, extensions, errors
    Program.cs
  web/                  # Next.js admin panel
  mobile/               # Expo customer app
```

## Domain Overview

| Area | Entities |
|---|---|
| Organization | Outlet, Table |
| Auth | AppUser, AppRole (ASP.NET Core Identity) |
| Settings | UserSettings |
| Products | Product, Tag, Allergen, ProductImage, ProductPrice, ProductList, ProductListItem |
| Modifiers | ModifierGroup, Modifier |
| Sales Config | PriceGroup, SalesChannel, TaxRate |
| Orders | Order, OrderLine, PromotionCode |
| Loyalty | LoyaltyPointLog, Favorite |
| Events | Event, EventDay |
| Printouts | PrintoutTemplate |
