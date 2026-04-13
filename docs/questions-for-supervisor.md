# Questions for Supervisor (Opiekun)

Questions to clarify before/during implementation. Bring to the next meeting.

---

### 1. Database views, stored procedures, functions

The requirements say "Niezbędne jest użycie widoków, procedur, funkcji, indeksowania" (point 8).

- **How many** views, stored procedures, and functions are expected at minimum?
- Can indexes alone satisfy the "indeksowanie" part, or do they expect something more specific (e.g., composite indexes, full-text search)?
- Is it OK if stored procedures / functions serve the reporting module specifically (so they have a clear business purpose), rather than wrapping basic CRUD?

### 2. CRUD + filtering + sorting on ALL tables?

Point 12 says "podstawowe operacje na wszystkich tabelach bazy danych (dodawanie, edytowanie, kasowanie, wyświetlanie, filtrowanie, sortowanie)."

- Does "all tables" literally mean every table including join tables and Identity tables?
- Or is it sufficient to have CRUD on all business entities, with join tables managed through their parent endpoints?

### 3. Separate projects for DB operations and business logic (points 3b, 3c)

Point 3 says the solution should contain:
- b) "Projekt realizujący operacje na bazie danych"
- c) "Projekt zawierający klasy logiki biznesowej"

- Can this be satisfied with a single API project using a well-known architectural pattern (Vertical Slices + CQRS) where persistence and business logic are clearly separated by convention (folders, not projects)?
- Or must these be physically separate .csproj class libraries (e.g., `CafeRMS.Persistence` + `CafeRMS.Domain`)?

Context: Vertical Slices is a well-established architecture pattern in the professional .NET community (see: Jimmy Bogard, CodeOpinion, Amichai Mantinband, etc.). It co-locates use cases instead of splitting by technical layer, but the separation of concerns is still clear.

### 4. Printouts from Word templates (point 19)

"ZSI powinien umożliwiać generowanie parametryzowanych wydruków na bazie szablonów wydruków np. określonych w word. W systemie powinna być możliwość zmiany szablonu wydruku przez użytkownika."

- What kind of printouts are expected in a cafe/restaurant context? E.g., order receipts, daily summaries, event flyers?
- How many different printout types are expected?
- Is it enough to support uploading a .docx template with placeholders that get filled in, or is there a more specific expectation?

### 5. Permission groups (point 11)

"Konieczny jest moduł zarządzania użytkownikami oraz grupami uprawnień użytkownika (za pomocą interfejsów systemu)."

- Is a role-based system with manageable permissions per role sufficient? (e.g., Admin creates roles, assigns permissions like "CanManageProducts", "CanViewOrders", etc., then assigns roles to users — all through the admin panel)
- Or is something more granular expected (per-user permissions, per-entity access control, etc.)?
