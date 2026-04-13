# CLAUDE.md — CafeRMS (monorepo root)

> **DO NOT submit this file to the university portal. Remove all CLAUDE.md files before submission.**

Instructions for Claude Code. Read this before touching any code.

---

## Project overview

Engineering thesis (projekt inżynierski) — a management system for small, themed cafes that combine food service with event hosting. Supports online ordering, loyalty, events, and product management. **Payments are out of scope.**

## System components

| Component | Path | Technology |
|---|---|---|
| API / Backend | `src/CafeRMS.Api/` | ASP.NET Core, PostgreSQL, EF Core |
| Admin Panel | `src/cafe-rms-admin/` | Next.js, TypeScript |
| Mobile App | `src/cafe-rms-mobile/` | React Native (Expo) |

Each component has its own `CLAUDE.md` with specific conventions. This root file covers shared rules.

## Monorepo structure

```
cafe-rms/
  CLAUDE.md                      ← you are here (shared rules)
  README.md
  docs/
    todo-api-generation.md
  src/
    CafeRMS.Api/                 ← .NET API + tests
      CLAUDE.md                  ← API-specific conventions
    cafe-rms-admin/              ← Next.js admin panel (later)
      CLAUDE.md
    cafe-rms-mobile/             ← React Native Expo (later)
      CLAUDE.md
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

## Shared rules — What Claude must never do without asking

- Do not push to remote or create PRs without explicit confirmation
- Do not add co-author attributions to commit messages
- Do not add features or refactors beyond what was asked
- Do not rename or restructure files/folders without being asked

---

## What's coming later (don't build yet)

- Reporting / analytics dashboard
