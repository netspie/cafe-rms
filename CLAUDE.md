# 🛑 ABSOLUTE RULES — READ FIRST, EVERY TURN 🛑

> **DO NOT submit this file to the university portal. Remove all CLAUDE.md files before submission.**

## 1. NEVER ACT WITHOUT AN EXPLICIT COMMAND.

**An explicit command is the user telling you to perform a specific action**: `do X`, `fix Y`, `add Z`, `commit`, `push`, `run the tests`, `refactor X to Y`, `delete Z`.

**THE FOLLOWING ARE NOT COMMANDS. ANSWER. DO NOT ACT.**

- **Questions** — "Why is this here?" / "What for?" / "How does X work?" / "Did you fuck this up?" / "Isn't this wrong?" / "Doesn't this look weird?"
- **Concerns** — "I'm worried that…" / "What if X breaks later?"
- **Observations** — "This looks heavy." / "Interesting that X." / "This could grow."
- **Musings** — "Maybe we should…" / "I wonder if…"
- **Frustrated / accusatory tone** — "Did you fucking do this?!" / "Why the fuck is X here?!" / "Isn't this fucked up?". **Tone is not a directive. Answer plainly.**

**If you cannot tell whether something is an explicit command, ASK BACK in one short sentence. DO NOT ASSUME.**

The cost of asking is negligible. The cost of an unwanted commit / refactor / change is high.

## 2. NEVER COMMIT WITHOUT AN EXPLICIT COMMAND.

The user must say `commit`, `push`, `commit and X`, `push and X`, or a clear equivalent.

- `commit` / `push` / `push and continue` / `push and move on` = **ONE-SHOT.** Commit/push exactly once, then stop.
- `push from now on always` (or a clear equivalent) = continuous mode until the user says `stop pushing` / `ask before pushing` / similar.
- **NO spontaneous commits.** Not at "natural checkpoints." Not "while we're here." Not "the build is green so let's just commit."

## 3. NEVER PUSH WITHOUT AN EXPLICIT COMMAND. SAME RULES AS COMMIT.

## 4. NEVER CHAIN `build && commit && push` IN ONE BASH CALL.

Run separately. Read each result. **Build → inspect → commit → inspect → push.** Broken code on `main` is a high-cost mistake.

## 5. NON-TRIVIAL WORK (more than 1–2 subtasks) GOES INTO `docs/todo.md` FIRST.

Draft the bullets under the right phase/section. Reach alignment with the user. THEN write code.

---

# CLAUDE.md — CafeRMS (monorepo root)

Instructions for Claude Code. The Absolute Rules above override everything below.

## Project overview

Engineering thesis (projekt inżynierski) — a management system for small, themed cafes that combine food service with event hosting. Supports online ordering, loyalty, events, and product management. **Payments are out of scope.**

## System components

| Component | Path | Technology |
|---|---|---|
| API / Backend | `src/api/CafeRMS.Api/` | ASP.NET Core, PostgreSQL, EF Core |
| API tests | `src/api/CafeRMS.Api.Tests/` | NUnit, WebApplicationFactory, EF InMemory |
| Admin Panel | `src/web/` | Next.js, TypeScript (later) |
| Mobile App | `src/mobile/` | React Native Expo (later) |

The API folder (`src/api/`) has its own `CLAUDE.md` with API-specific conventions and a single `CafeRMS.slnx` covering both API + Tests projects.

## Monorepo structure

```
cafe-rms/
  CLAUDE.md                       ← you are here (Absolute Rules + monorepo-shared)
  README.md
  docs/
    todo.md                         ← living, phased plan; check + update as work progresses
    todo-api-generation.md
  src/
    api/                            ← .NET API server
      CLAUDE.md                     ← API-specific conventions
      CafeRMS.slnx                  ← solution covering both projects below
      CafeRMS.Api/                    ← the API project
      CafeRMS.Api.Tests/              ← integration test project
    web/                            ← Next.js admin panel (later)
    mobile/                         ← React Native Expo (later)
```

---

## Hard requirements (from university "Wymagania" PDF)

These are non-negotiable. Every item must be visibly covered in the final project.

- **Min 30 DB tables** (Identity tables may count — pending supervisor confirmation)
- **DB views, stored procedures, functions, indexing** — required, likely for reporting
- **CRUD + filtering + sorting** on all business entity tables
- **Auth + permission group management** via system UI (not just static roles)
- **2–3 business reports** with PDF/Excel export
- **Parameterized printouts** from Word templates (user-changeable)
- **Min 50 professional UI views** (frontend concern, but API must support them all)
- **All visible content** (texts, images) managed from admin panel — no hardcoded content
- **3 fully closed business processes** (e.g., full order lifecycle)
- **Well-known architecture pattern** — Vertical Slices + CQRS

---

## Project-specific rules

- **Never** add co-author attributions to commit messages.
- **One commit per `docs/todo.md` item** when committing completed work. Bundle only when items are genuinely inseparable (a shared refactor touched by multiple items).
- **Meta changes** (convention rules, rename sweeps, CLAUDE.md edits) get their own commits, separate from feature work.
- **`docs/todo.md` checkbox edits** go with the commit that completes the item they reference, not in a trailing "tick boxes" commit.

---

## What's coming later (don't build yet)

- Reporting / analytics dashboard
