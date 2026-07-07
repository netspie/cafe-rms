# CafeRMS — plan

Working notes. Full detail lives in git history; below is a short log of what's done and a space for what's next.

## Next

**Mobile — pricing UX**
- [ ] Update the checkout total in real time as loyalty points change (live discount preview; 1 pt = 1 PLN).

**Product images**
- [ ] Add image upload on **web admin only** (no mobile upload); store files on the API server filesystem, serve as static `/uploads/...`, persist via a Docker volume. `ProductImage.Url` holds the path.
- [ ] Get product images into the repo + seed by path. *(I can't fetch real photos — user sources free-for-commercial from Unsplash/Pexels/Pixabay and drops files in; alternatively I generate committed SVG placeholders.)*
- [ ] Show images on mobile: detail screen already renders the first image; add a thumbnail field to `/api/menu` and show it on the menu list cards.

**Products**
- [ ] Refine the default product set + rename products so each is recognizable.

**Web**
- [ ] Rename the Settings page to "Company & outlet" (fit the name properly).

**Seed / localisation**
- [ ] Use Polish names for seeded staff and customers (this is an RMS for Poland); keep the `@shiba.pl` emails + `admin/user1` handles.

**Demo**
- [ ] Plan the full demo script across features — primary the 1/2/3 business scenarios from the thesis docs, then extras (adding menu items, ordering, reports).

**UI polish (last)**
- [ ] Improve overall look — all list-item backgrounds/cards look bland (mobile + web); make them more appealing.

## Done (short log — see git history for detail)
- Events (mobile): `/api/events/mobile` returns published today-or-future only, today-first; closed/past hidden. Today's event shows as a highlighted card + a banner on the Menu tab; upcoming below. Stable per-event colour (id hash, sakura/gold palette). Seeder keeps one event always dated today (dynamic on reseed). *(uncommitted)*
- Event strikethrough price: `/api/menu` (+ detail) returns the pre-event default price for event items; mobile strikes it through above the discounted price on card + detail. *(uncommitted)*
- Tables on orders: staff order list/detail show table + total; waiter table view groups open orders.
- Order rules: dine-in requires a table; loyalty redemption capped at half the order (1 pt = 1 PLN), enforced in the Order entity.
- Live new-order notifications: web polls `/admin/live-orders/poll`, sonner toast + Web Audio ringtone + refresh.
- Sales-per-product report: `/api/reports/products` (+ event filter), PDF/Excel export, web page with chart.
- Gross (brutto) pricing: `ProductPrice.Gross` stored; line net/VAT derived at placement so reports are unchanged.
- Event-day pricing: an event has a product list + price group; its products are re-priced on the event day.
- Outlet default menu: `DefaultPriceGroupId` + `DefaultProductListId`; two switchable menus seeded (Menu standardowe = default, Menu poranne); price groups renamed Standard / Promocja. Switchable live in admin Settings.
- Server-authoritative pricing: client sends only `{productId, quantity}` — server resolves the price group (event override vs outlet default) and tags the event. New customer endpoints `/api/menu` + `/api/menu/{id}` return one resolved gross price; `/api/products` is the admin catalog.
- Web: 401 → `/login` redirect (stale session no longer crashes); settings selects keyed so saved value shows after save.
- Mobile: single price + event badge; pull-to-refresh + refetch on focus; `useFetch.refetch` stabilised.
- Demo seed: 5 accounts (all `Demo1234`), event menus, three events today.

## Discipline
- One phase at a time; pause for go/no-go.
- Build green before commit; verify commit before push. Never build+commit+push in one shell call.
- Commit only on explicit command.
