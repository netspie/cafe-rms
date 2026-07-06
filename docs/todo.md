# CafeRMS — Feature plan: waiter ops + reporting

Three features for the next iteration. Decisions locked with the user:

- **Live order notifications:** polling (no SignalR/SSE). Simplest to build and defend; ~5–10s latency is fine for a cafe.
- **Order model:** orders stay immutable (promo/loyalty/VAT snapshot at placement). A table can hold **multiple open orders**; no "add items to an open order" endpoint. The waiter view groups a table's orders and shows a combined total.
- **Reports:** a single new **sales-per-product** report with an optional `eventId` filter. With no event = store-wide "what sells"; with an event = "which products sold at this event" (the event-success check). No change to the existing event-attendance report.

Build order: Phase A (tables on orders) first, because the notification toast and the waiter view both depend on the table being visible. Then B (notifications), then C (report).

---

## Phase A — Tables on orders (staff visibility)

Data side already works: mobile checkout picks a table and `POST /api/my/orders` persists `Order.TableId`. The gap is that staff can't see it.

**API**
- [x] Surface the table on order DTOs — added `TableId` + `TableName` (+ per-order `Total`) to `ListOrders.Item`; added `TableName` to `GetOrderById.Result`. Also added a `tableId` filter to `/api/orders`.
- [x] Validate `TableId` in `PlaceOrder` — rejects when the table doesn't exist or isn't in the order's outlet.

**Web**
- [x] Orders list (`app/admin/orders/page.tsx`) — added "Table" + "Total" columns.
- [x] Order detail (`app/admin/orders/[id]/page.tsx`) — shows a "Table X" chip.
- [x] Tables detail (`app/admin/tables/[id]/page.tsx`) — shows the table's open orders (count + combined total + links).

## Phase A½ — Order-flow fixes & rules

Noticed while reviewing Phase A. One rule still open (dine-in table); the bug fixes are done.

**Bug fixes**
- [x] **Mobile order auto-refresh** — order detail polls `/api/my/orders/{id}` every ~7s while status is `Placed`, refetches only when the status changes, so a waiter-closed/cancelled order updates the UI (hides the Cancel button). *(done — commit `488bb96`)*
- [x] Loyalty "negative total" bug — **decided: 1 pt = 1 PLN**, and points may cover **at most half** the order. `PlaceOrder` rejects `LoyaltyPointsUsed > floor(subtotal · 0.5)` (`MaxLoyaltyShareOfSubtotal`); `ListOrders` total keeps `− LoyaltyPointsUsed` (now valid & bounded ≥ half price). Seeded promo order lowered 50 → 20 pts to satisfy the cap. *(done — commit `488bb96`)*

**Order rules — server-side in `PlaceOrder`**
- [x] Dine-in requires a table; takeaway may be tableless. `PlaceOrder` loads the sales channel and rejects a dine-in (`IsTakeout == false`) order with no `TableId` ("Dine-in orders require a table."); takeaway keeps the table optional. Mobile checkout mirrors it — for a dine-in channel it hides "No Table", shows a hint, and disables Place Order until a table is picked. *(implemented, unverified/uncommitted)*

**Already shipped (for record)** — commit `ca3417f`
- [x] Demo seed accounts hardcoded (`Demo1234`; admin/manager/barista/user1/user2 `@shiba.pl`) + web & mobile login prefill.
- [x] Mobile startup 401s fixed — gate route render until auth settles.

## Phase B — Live new-order notifications (polling)

No API changes — reuses `GET /api/orders?status=Placed&sort=-createdAt`.

**Web**
- [x] Token-proxy poll route — `app/admin/live-orders/poll/route.ts` reads the httpOnly cookie and proxies `GET /api/orders?status=Placed&sort=-createdAt&pageSize=20`.
- [x] Watcher client component (`components/live-orders-watcher.tsx`) — polls every 7s, tracks last-seen `createdAt` in `localStorage` (a refresh doesn't re-toast old orders), and on a new order pops a `sonner` toast ("New order — Table X" / "Walk-in"), plays a **Web Audio ringtone** (two-tone chime, no asset), and `router.refresh()`.
- [x] Mounted in the admin layout so it fires on any admin page. *(skipped the optional dedicated "Live orders" page + nav item.)*
- *(all three implemented, unverified/uncommitted)*

## Phase C — Sales-per-product report (+ per-event breakdown)

Clone the existing reports trio (`Features/Reports`, QuestPDF + ClosedXML, static `Render` classes).

**API**
- [x] `GetSalesPerProduct` use case — `GET /api/reports/products?from&to&eventId?`, gated `ReportsView`. Groups closed/non-cancelled order lines by product; per product: qty, net/VAT/gross (`(NetPerOne + VatPerOne) * Quantity`), ordered by gross desc. `eventId` filters `Order.EventId`. FluentValidation on the date range.
- [x] `ExportSalesPerProduct` — `export.pdf` (`SalesPerProductPdf`) + `export.xlsx` (`SalesPerProductExcel`).

**Web**
- [x] `app/admin/reports/products/page.tsx` — date range + optional event picker, stat cards (gross/items/products), recharts bar chart (gross per product), breakdown table.
- [x] `export-pdf/route.ts` + `export-xlsx/route.ts` — cloned the report route handlers.
- [x] Reports nav item ("Sales per product"); event detail page links to the report pre-filtered by that event ("Products sold").
- *(all implemented, unverified/uncommitted)*

## Phase D — Event menus (event-day ordering on mobile)

**Goal:** one menu, always. Prices are **stored and shown gross (brutto, VAT-in)**. On an **event day**, event products are **marked** and shown at the **event's price group** price; everything else at the default group. Each product appears once (no mode switch). Orders with event products are **tagged with the event**. Two parts: (1) fix the pricing model to store gross, (2) wire event pricing on mobile.

**Context — why it doesn't work yet** *(not objectives)*
- `ProductPrice.Net` is stored and treated **literally as net** (pre-VAT): `PlaceOrder` adds VAT on top, and the mobile detail screen shows this **net** value to the customer — wrong for a PL café (customer must see brutto).
- The `/api/products` list returns **no price at all**; event `productListId` / `priceGroupId` are never read at order time.

**Seed data** *(do first — also unblocks the Phase C reports/charts)*
- [x] Seed two product lists as event menus ("Menu Shiba Meet-up", "Menu Paw Painting"), each a subset of existing products, via `ProductList` + `ProductListItem`.
- [x] Assign each list + a price group to the seeded events, and add a "Shiba Coffee Day" event with a day == **today** (published) so the event-day switch is testable.
- [x] Seed 10 more closed orders across dates (event-tagged + general) via a `SeedClosedOrderAsync` helper — 15 orders total; reports now show 44 items / 738.90 gross. *(implemented, uncommitted)*

**Pricing model — store gross (brutto)** *(do next; needs a migration + reseed)*
- [ ] Rename `ProductPrice.Net` → `Gross` (brutto), update EF config, and add a **migration**.
- [ ] Derive line net/VAT from gross in `PlaceOrder`: `net = round(gross / (1 + rate/100), 2)`, `vat = gross − net`. Order lines still store `NetPerOne` / `VatPerOne`, so reports are unchanged.
- [ ] Update `SetProductPrice` (API) + the admin web price field to set the **gross** price ("Cena brutto").
- [ ] Return `gross` from `GetProductById` `prices`; seed products with round gross prices; reseed with `down -v`.

**API**
- [ ] Add `prices` (`priceGroupId`, `gross`) to the `GET /api/products` list response (today it returns no price).
- [ ] Add `GET /api/events/active-today` — today's published event(s): `name`, `priceGroupId`, and the **product ids** in the event's list.

**Mobile**
- [ ] Resolve each product's shown gross price: if the product id is in today's event list, use `prices.find(p => p.priceGroupId === event.priceGroupId).gross`; otherwise `prices[0].gross`. Fall back to `prices[0]`. Apply on the menu card and detail screen (replacing bare `prices[0]`).
- [ ] Show the gross price on each menu card, and mark event products with an event badge.
- [ ] Show gross (not net) in the cart and checkout too.
- [ ] Tag the order with the active event when the cart includes event products.

---

## Discipline
- One phase at a time; pause for go/no-go between phases.
- Build green before commit; verify commit before push. No build+commit+push in one shell call.
- Commit only on explicit command.
