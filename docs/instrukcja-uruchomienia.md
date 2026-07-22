# CafeRMS — instrukcja uruchomienia

Dokument opisuje, jak po rozpakowaniu plików źródłowych uruchomić kompletny system: backend (API + baza danych), panel konfiguracyjny (web) oraz aplikację mobilną. Bazy danych nie trzeba przywracać z kopii — przy pierwszym uruchomieniu jest ona automatycznie tworzona, migrowana i wypełniana danymi demonstracyjnymi.

## Wymagania wstępne

- **Docker Desktop** — do uruchomienia backendu (API + baza danych).
- **Node.js 20+ oraz npm** — do panelu konfiguracyjnego (web) i aplikacji mobilnej.
- Do uruchomienia aplikacji mobilnej: symulator iOS (Xcode) lub emulator Android (Android Studio), albo aplikacja **Expo Go** na telefonie.

## Rozpakowanie

Rozpakuj archiwum ze źródłami. Wszystkie polecenia poniżej wykonuj z **głównego katalogu projektu** — tego, w którym znajduje się folder `src/` oraz pliki `README`.

## Krok 1 — Backend (API + baza danych)

Backend należy uruchomić jako pierwszy — panel web i aplikacja mobilna z niego korzystają. Jedno polecenie uruchamia bazę PostgreSQL i API. Przy pierwszym starcie migracje i dane demonstracyjne zakładane są automatycznie.

```bash
docker compose -f src/api/docker-compose.yml up --build -d
```

Po chwili dostępne są:

| Usługa | Adres |
|---|---|
| API | http://localhost:5179 |
| Baza PostgreSQL | localhost:5434 (użytkownik `postgres`, hasło `postgres`, baza `cafe_rms`) |

Sprawdzenie: otwórz http://localhost:5179/health — powinien pojawić się napis `Healthy`.

Pozostałe polecenia:

```bash
docker compose -f src/api/docker-compose.yml logs -f api      # podgląd logów API
docker compose -f src/api/docker-compose.yml down             # zatrzymanie, dane zostają
docker compose -f src/api/docker-compose.yml down -v          # zatrzymanie i wyczyszczenie bazy (kolejny start zasieje ją od nowa)
```

## Krok 2 — Panel konfiguracyjny (web)

```bash
cd src/web
npm install
npm run dev
```

Otwórz http://localhost:3000 i zaloguj się kontem pracownika (patrz niżej). Domyślnie panel łączy się z API pod `http://localhost:5179`; adres można nadpisać zmienną `API_BASE_URL`.

## Krok 3 — Aplikacja mobilna

```bash
cd src/mobile
npm install
```

Sposób uruchomienia zależy od celu, ponieważ każde środowisko inaczej widzi API działające na komputerze. Aplikacja domyślnie łączy się z API pod `http://localhost:5179`, a adres nadpisuje zmienna `EXPO_PUBLIC_API_BASE_URL`.

**Symulator iOS** — `localhost` działa bez zmian:

```bash
npm run start
```

następnie naciśnij `i`.

**Emulator Androida** — w emulatorze `localhost` wskazuje sam emulator, dlatego host trzeba podać jako `10.0.2.2`:

```bash
EXPO_PUBLIC_API_BASE_URL=http://10.0.2.2:5179 npm run start
```

następnie naciśnij `a`.

**Telefon fizyczny (Expo Go)** — podaj adres sieci LAN komputera i zeskanuj kod QR:

```bash
EXPO_PUBLIC_API_BASE_URL=http://192.168.1.10:5179 npm run start
```

(zamień `192.168.1.10` na adres IP swojego komputera w sieci lokalnej).

> Uwaga: jeżeli aplikacja pokaże „Logowanie nieudane — TypeError: Network request failed", oznacza to, że nie widzi API. Prawie zawsze przyczyną jest zły adres hosta — użyj `10.0.2.2` (emulator Androida) lub adresu LAN (telefon), jak wyżej. Po zmianie przeładuj aplikację klawiszem `r` w terminalu Expo.

## Konta demonstracyjne

Zakładane automatycznie przy pierwszym uruchomieniu. Hasło do każdego konta: **`Demo1234`**

| E-mail | Rola | Klient |
|---|---|---|
| `admin@shiba.pl` | Właściciel (pełne uprawnienia) | web |
| `manager@shiba.pl` | Kierownik | web |
| `barista@shiba.pl` | Barista | web |
| `user1@shiba.pl` | Klient | mobile |
| `user2@shiba.pl` | Klient | mobile |

## Dane konfiguracyjne

Do standardowego uruchomienia nie trzeba niczego uzupełniać — domyślne wartości działają od razu. Adres API, z którym łączą się klienci, można w razie potrzeby nadpisać zmiennymi `API_BASE_URL` (panel web) oraz `EXPO_PUBLIC_API_BASE_URL` (aplikacja mobilna).

## Weryfikacja

1. Backend działa: http://localhost:5179/health zwraca `Healthy`.
2. Panel web: logowanie `admin@shiba.pl` / `Demo1234` wchodzi do panelu konfiguracyjnego.
3. Aplikacja mobilna: logowanie `user1@shiba.pl` / `Demo1234` pokazuje menu z produktami.

## Najczęstsze problemy

- **Docker nie działa** — upewnij się, że Docker Desktop jest uruchomiony przed poleceniem `up`.
- **Port zajęty** (5179, 3000 lub 5434) — zwolnij port lub zmień mapowanie w `src/api/docker-compose.yml`.
- **Reset danych do stanu początkowego** — `docker compose -f src/api/docker-compose.yml down -v`, a następnie ponownie `up --build`.
- **Aplikacja mobilna na telefonie nie łączy się z API** — ustaw `EXPO_PUBLIC_API_BASE_URL` na adres LAN komputera (patrz Krok 3).
