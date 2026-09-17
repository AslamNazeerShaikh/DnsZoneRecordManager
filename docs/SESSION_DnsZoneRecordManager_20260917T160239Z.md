# Session Memory — DnsZoneRecordManager — 20260917T160239Z

- Repo: `DnsZoneRecordManager` (local `/Users/aslamshaikh/Projects/DnsZoneRecordManager`, GitHub `AslamNazeerShaikh/DnsZoneRecordManager`, private, `main` tracks `origin/main`)
- Saved (UTC): 20260917T160239Z. Model this session: `opencode/muse-spark-1.3-contributor-free` via OpenCode Zen.
- Purpose: full read-through of requirements + assessment bundle + toolchain, saved so a later chat can resume with one file.
- Resume instruction for next chat: read this file + `docs/REQUIREMENTS_ANALYSIS.md` + `docs/TOOLCHAIN_AND_PACKAGES.md`, then continue at "Next steps".

## What was read (every line, every file — independently verified this session)

1. `docs/REQUIREMENTS_ANALYSIS.md` (174 lines) — normative analysis. Assignment verbatim (§2), 8 PDF links (§3), seed data (§4), build plan (§5), data model (§6), validation (§7), scope checklist (§8), build order (§9), open questions (§10).
2. `docs/TOOLCHAIN_AND_PACKAGES.md` (122 lines) — pinned toolchain (§1–§5), not-yet-installed (§6), formatters (§7).
3. `/Users/aslamshaikh/Downloads/Other/REQUIREMENTS_ANALYSIS.md` — `diff` vs repo copy: IDENTICAL.
4. `#take-home/WebApp-TakeHome_2025.pdf` — read full text; matches §2 verbatim (DevSelect Confidential, ASP.NET 6.x/stable, In-Memory, EF preferred, Bootstrap/jQuery-or-vanilla-JS, 8h/5d box, clean/extensible/secure/maintainable grading, zip + readme.md delivery).
5. `#take-home/zone_sample/nahuexolab.com.dns` (23 lines) — matches §4a exactly (Azure export, `$TTL 300`, SOA + 4× NS + 1× TXT `_dmarc`).
6. `#take-home/zone_sample/Dns_Records_05_24_2024 20_06_01.xlsx` — decoded via stdlib zip/xml (sharedStrings + sheet1); 8 cols × 5 rows, matches §4b exactly.
7. `#take-home/zone_sample/DM_Zone_snapshot.png` — viewed; matches §4c (zone dropdown, Export, Search, FQDN/Name/Type/TTL/Data/Modified/Action grid, 5 rows, `1 – 5 of 5 items`).
8. `#take-home/zone_sample.zip` (3 files) and `DS-ENG_take-home.zip` (7 entries) — `unzip -l` matches §1 rows 5–6.
9. `#take-home/zone_sample 2/` — `diff -r` vs `zone_sample/`: IDENTICAL (safe to delete one copy).
10. `.DS_Store` files — Finder metadata, ignored.
- Link liveness NOT re-checked today; §3 table (5 OK, 3 redirect/dead as of 2026-09-17) stands.

## Goal (from §2 + §5 + §8)

Simple web app for Query/Create/Modify/Delete of DNS Zones + Records for non-expert customers. Grading: clean/extensible/secure/maintainable, guided UX, comments noting trade-offs, readme.md. Scope: zones list/search/create/rename/delete; records list-by-zone/search/filter/create/edit/delete; toasts, count meter, confirm dialogs; seed `nahuexolab.com` + 5 records on first run; per-solution readme + zip delivery.

## Domain rules (§6 model, §7 validation — server authoritative, FluentValidation)

- Zone: Id PK, Name unique (case-insensitive, lowercase, trim trailing dot, ≤253 total, label ≤63). Record: Id PK, ZoneId FK cascade, Name, Type ∈ {A,AAAA,CNAME,NS,TXT}, TTL positive (keep 3600/172800), Data per-type (A=IPv4, AAAA=IPv6, CNAME/NS=hostname, TXT non-empty ≤255/string), CreatedUtc/UpdatedUtc; Unique(ZoneId,Name,Type,Data); FQDN derived, not stored.
- ≥4 NS per zone (floor), ≤10 records per zone (ceiling), CNAME exclusivity, no duplicate zones/records.
- SOA/`$TTL` headers not editable; Modified = UpdatedUtc; Export = records-only CSV; zone delete cascades after confirm.

## Toolchain pins (TOOLCHAIN doc — re-verify via its §5)

.NET SDK 10.0.400 (+10.0.300), net10.0, macOS osx-arm64. Server NuGet: OpenApi 10.0.11 (S2), FluentValidation 12.1.1 (both). Tests (both): xunit 2.9.3, runner 3.1.4, TestSdk 17.14.1, coverlet 6.0.4, Moq 4.20.72, Bogus 35.6.5, FluentAssertions 8.11.0. Client: Next 16.3.5, React 19.2.8, TS 5.9.3, Tailwind 4.3.3, eslint 9.39.5, prettier 3.9.7. MVC wwwroot: Bootstrap 5.3.3, jQuery 3.7.1, Validation 1.21.0, Unobtrusive 4.0.0. Formatters: CSharpier 1.3.0 (`.config/dotnet-tools.json`), Prettier defaults. Explicit `Program`+`Main` convention (no top-level statements).

## Repo state at save time

- Commits: `23f06b7` (scaffolds, tests, .opencode, formatters, docs, graph) + `1d42c99` (AGPL-3.0 LICENSE.md). Pushed to GitHub.
- Scaffolded, NOT yet implemented: EF Core stores, Generic Repo/UoW (S1), hand-rolled CQRS (S2), domain/validators, seed, UI, per-solution readmes, zips.
- `.opencode` verified clean: no College Admission leftovers, no orphaned commands/agents/skills entries.

## Open tensions for next session (do not guess — confirm or follow doc default)

1. DB for Solution1: §5 says EF Core + SQLite for BOTH builds, but README/TOOLCHAIN/scaffold say S1 = In-Memory (brief default), S2 = SQLite. Decide before implementing S1 store.
2. §10 hiring-manager questions: export format (default records-only CSV), 4-NS floor scope (default always enforced).
3. Build order when authorized: S1 MVC first, S2 API+Next.js second, then zips (§9).
