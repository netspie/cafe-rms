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

## Phase B — Live new-order notifications (polling)

No API changes — reuses `GET /api/orders?status=Placed&sort=-createdAt`.

**Web**
- [ ] Token-proxy poll route — `app/admin/live-orders/poll/route.ts` reads the httpOnly cookie and proxies the placed-orders query (same cookie→Bearer pattern as the report export routes).
- [ ] Watcher client component — polls every ~7s, tracks last-seen order id/`createdAt` in `localStorage` (so a page refresh doesn't re-toast old orders), and on a new order pops a `sonner` toast ("New order — Table 5") + `router.refresh()`.
- [ ] Mount the watcher in the admin shell so it fires on any admin page; optional dedicated "Live orders" page + nav item under **Operations**.

## Phase C — Sales-per-product report (+ per-event breakdown)

Clone the existing reports trio (`Features/Reports`, QuestPDF + ClosedXML, static `Render` classes).

**API**
- [ ] `GetSalesPerProduct` use case — `GET /api/reports/products?from&to&eventId?`, gated `ReportsView`. Group `OrderLines` by `ProductId`, join `Products` for the name; per product: quantity sold, net/VAT/gross revenue (`(NetPerOne + VatPerOne) * Quantity`, matching the other reports). Filter `ClosedAt != null && CancelledAt == null`; `eventId` filters `Order.EventId`. FluentValidation on the date range.
- [ ] `ExportSalesPerProduct` — `export.pdf` (QuestPDF, clone `SalesReportPdf`) + `export.xlsx` (ClosedXML, clone `SalesReportExcel`).

**Web**
- [ ] `app/admin/reports/products/page.tsx` — date range + optional event picker, stat cards, breakdown table, optional recharts bar chart.
- [ ] `export-pdf/route.ts` + `export-xlsx/route.ts` — clone the events-report route handlers (swap upstream path + default filename).
- [ ] Reports nav item; link from the event detail page → products report pre-filtered by that event ("products sold at this event").

---

## Discipline
- One phase at a time; pause for go/no-go between phases.
- Build green before commit; verify commit before push. No build+commit+push in one shell call.
- Commit only on explicit command.
