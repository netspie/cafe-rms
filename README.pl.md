# CafeRMS

System zarządzania dla małych, tematycznych kawiarni łączących gastronomię z organizacją wydarzeń. Projekt inżynierski.

🇬🇧 [English version](README.md)

## Komponenty systemu

| Komponent | Katalog | Technologia | Przeznaczenie |
|---|---|---|---|
| **API / Backend** | `src/api` | ASP.NET Core, PostgreSQL, EF Core | Wspólny backend dla obu klientów |
| **Panel administracyjny** | `src/web` | Next.js, TypeScript | Personel: produkty, menu, wydarzenia, zamówienia, raporty |
| **Aplikacja mobilna** | `src/mobile` | React Native (Expo) | Klienci: menu, zamówienia online, lojalność, wydarzenia |

Płatności są **poza zakresem** — brak transakcji i integracji z bramką płatniczą.

## Stos technologiczny

**API** — .NET 10 / C# 14, ASP.NET Core (kontrolery MVC), PostgreSQL przez EF Core, ASP.NET Core Identity + JWT, FluentValidation, QuestPDF + ClosedXML (eksport raportów), Scalar (dokumentacja API).

**Web** — Next.js 16 (App Router, komponenty serwerowe), React 19, TypeScript, Tailwind CSS 4, Recharts.

**Mobile** — Expo 54, expo-router, React Native 0.81, NativeWind, Zustand.

## Architektura

Vertical Slices + CQRS. Każdy przypadek użycia to jeden plik zawierający rejestrację trasy, komendę/zapytanie, handler, walidator i DTO odpowiedzi.

## Uruchomienie

Backend uruchamiamy jako pierwszy — obaj klienci go wymagają.

### 1. Backend

Jedna komenda uruchamia Postgresa i API. Migracje są zakładane, a dane demonstracyjne zasiewane przy pierwszym starcie.

```bash
docker compose -f src/api/docker-compose.yml up --build -d
```

| Usługa | Adres |
|---|---|
| API | http://localhost:5179 |
| Dokumentacja API (Scalar) | http://localhost:5179/scalar |
| Postgres | localhost:5434 (`postgres` / `postgres`, baza `cafe_rms`) |

Pozostałe komendy:

```bash
docker compose -f src/api/docker-compose.yml logs -f api      # podgląd logów API
docker compose -f src/api/docker-compose.yml up --build api   # przebudowanie samego API
docker compose -f src/api/docker-compose.yml down -v          # zatrzymanie i wyczyszczenie bazy
```

<details>
<summary>Uruchomienie API lokalnie, bez Dockera</summary>

Wymaga .NET 10 SDK oraz PostgreSQL na `localhost:5434` (baza `cafe_rms`, użytkownik `postgres`, hasło `postgres` — albo nadpisz `ConnectionStrings:Default` w `appsettings.Development.json`).

```bash
dotnet ef database update --project src/api/CafeRMS.Api
dotnet run --project src/api/CafeRMS.Api
```
</details>

### 2. Panel administracyjny (web)

```bash
cd src/web
npm install
npm run dev
```

Otwórz http://localhost:3000. Domyślnie oczekuje API pod `http://localhost:5179`; adres nadpisuje zmienna `API_BASE_URL`.

### 3. Aplikacja mobilna

```bash
cd src/mobile
npm install
npx expo start
```

Klawisz `i` uruchamia symulator iOS, `a` — Android, albo zeskanuj kod QR aplikacją Expo Go. Domyślnie oczekuje API pod `http://localhost:5179`; adres nadpisuje zmienna `EXPO_PUBLIC_API_BASE_URL`.

> Na fizycznym urządzeniu `localhost` wskazuje na telefon. Ustaw `EXPO_PUBLIC_API_BASE_URL` na adres LAN komputera, np. `EXPO_PUBLIC_API_BASE_URL=http://192.168.1.10:5179 npx expo start`.

## Konta demonstracyjne

Zasiewane automatycznie przy pierwszym uruchomieniu. Hasło do każdego konta: **`Demo1234`**

| E-mail | Rola | Klient | Zakres |
|---|---|---|---|
| `admin@shiba.pl` | Właściciel | Web | Wszystko |
| `manager@shiba.pl` | Kierownik | Web | Wszystko poza zarządzaniem rolami |
| `barista@shiba.pl` | Barista | Web | Zamówienia, lojalność, produkty |
| `user1@shiba.pl` | Klient | Mobile | Menu, własne zamówienia, lojalność, wydarzenia |
| `user2@shiba.pl` | Klient | Mobile | Menu, własne zamówienia, lojalność, wydarzenia |

Panel administracyjny ukrywa pozycje nawigacji niedostępne dla zalogowanej roli; API niezależnie egzekwuje te same uprawnienia.

## Struktura projektu

```
src/
  api/CafeRMS.Api/
    Features/           # Vertical slices — jeden katalog na obszar dziedziny
      Auth/             # Logowanie, role, uprawnienia (Identity + JWT)
      Products/         # Katalog, ceny, zdjęcia
      Orders/           # Składanie zamówień i ich cykl życia
      Events/           # Wydarzenia, dni wydarzeń, wydruki
      Reports/          # Sprzedaż, sprzedaż wg produktów, frekwencja (+ PDF/Excel)
      ...
    Persistence/        # AppDbContext, migracje, seedowanie
    Infrastructure/     # Handlery autoryzacji, obsługa wyjątków
    Resources/          # Szablony wydruków
    Shared/             # Elementy przekrojowe: filtr walidacji, rozszerzenia, błędy
    Program.cs
  web/                  # Panel administracyjny (Next.js)
  mobile/               # Aplikacja klienta (Expo)
```

## Model dziedziny

| Obszar | Encje |
|---|---|
| Organizacja | Outlet, Table |
| Uwierzytelnianie | AppUser, AppRole (ASP.NET Core Identity) |
| Ustawienia | UserSettings |
| Produkty | Product, Tag, Allergen, ProductImage, ProductPrice, ProductList, ProductListItem |
| Modyfikatory | ModifierGroup, Modifier |
| Konfiguracja sprzedaży | PriceGroup, SalesChannel, TaxRate |
| Zamówienia | Order, OrderLine, PromotionCode |
| Lojalność | LoyaltyPointLog, Favorite |
| Wydarzenia | Event, EventDay |
| Wydruki | PrintoutTemplate |
