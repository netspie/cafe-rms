# CafeRMS Guest Mobile App

Expo React Native app for cafe customers. Browse the menu, place orders, manage loyalty.

## Stack

- Expo SDK 54 + expo-router (file-based routing)
- TypeScript
- NativeWind (Tailwind for React Native)
- Zustand (auth + cart stores)
- React Query (server state)
- expo-secure-store (JWT persistence)

## Run

```bash
npm install
npm start              # then press i (iOS sim) / a (Android emulator) / w (web)
```

### Pointing at a local API

Default base URL is `http://localhost:5167`. Override via env:

```bash
EXPO_PUBLIC_API_BASE_URL=http://10.0.2.2:5167 npm start    # Android emulator
EXPO_PUBLIC_API_BASE_URL=http://localhost:5167 npm start   # iOS simulator
```

For physical device on the same Wi-Fi: use the host machine's LAN IP.

## File layout

```
app/
  _layout.tsx                        ← root layout: QueryClient + auth gate
  globals.css                        ← Tailwind directives
  (auth)/
    _layout.tsx
    login.tsx                        ← POST /api/auth/login
    register.tsx                     ← POST /api/auth/register/guest
  (tabs)/
    _layout.tsx                      ← Menu / Orders / Loyalty / Events / Profile
    menu/
      _layout.tsx
      index.tsx                      ← GET /api/products + /api/tags
      [id].tsx                       ← GET /api/products/{id} + favorite toggle
      favorites.tsx                  ← GET /api/my/favorites
    orders/
      _layout.tsx
      index.tsx                      ← GET /api/my/orders
      [id].tsx                       ← GET /api/my/orders/{id} + cancel
    loyalty.tsx                      ← /api/my/loyalty/balance + /history
    events/
      _layout.tsx
      index.tsx                      ← GET /api/events
      [id].tsx                       ← GET /api/events/{id}
    profile.tsx                      ← GET /api/me + sign out
  cart.tsx                           ← cart store editor
  checkout.tsx                       ← POST /api/my/orders
components/
  Button.tsx                         ← shared primary/secondary/danger button
lib/
  api.ts                             ← fetch wrapper with bearer auth
  types.ts                           ← API response shapes (mirror api naming)
stores/
  authStore.ts                       ← token + secure-store hydration
  cartStore.ts                       ← in-memory cart (cleared on order placed)
```

## Auth flow

Token stored in `expo-secure-store`. Hydrated on app start. Auth gate in `app/_layout.tsx` redirects:
- No token + not in `(auth)` → `/(auth)/login`
- Has token + in `(auth)` → `/(tabs)/menu`

401 from any API call clears the token (`api.ts` → `useAuthStore.getState().logout()`).
