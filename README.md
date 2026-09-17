# DnsZoneRecordManager

Take-home assessment: DNS Zones + DNS Records manager (Query, Create, Modify, Delete) for a non-technical customer persona.
Full requirement analysis (every source file read, all 8 PDF hyperlinks checked): `docs/REQUIREMENTS_ANALYSIS.md`.

Two slim, goal-centric solutions cover every stack option the brief allows. Both follow Clean Architecture + DDD boundaries, SOLID, and YAGNI (no speculative abstractions; patterns below are the ones the brief/assessment explicitly rewards).

| | Solution1 | Solution2 |
|---|---|---|
| Folder | `Solution1/` | `Solution2/` |
| Backend | ASP.NET Core MVC, .NET 10 | ASP.NET Core Web API, .NET 10 (`src/server/DnsZoneRecordManager`) |
| Data access | Generic Repository + Unit of Work, In-Memory (brief default) | CQRS **without MediatR** (hand-rolled commands/queries + handlers), EF Core + SQLite |
| Frontend | Razor + Bootstrap + jQuery (default MVC template) | Next.js 16.3.5 + React 19 + TypeScript 5 + Tailwind CSS 4 (`src/client`) |
| Status | Empty scaffold (this step) | Empty scaffold (this step) |

Core domain rules both must enforce (see analysis doc §7): ≥4 NS records per zone, ≤10 records per zone, types A/AAAA/CNAME/NS/TXT only, no duplicate zones/records, guided validation feedback.

## Repo layout

```text
DnsZoneRecordManager/
  Solution1/DnsZoneRecordManager.slnx + src/server/DnsZoneRecordManager/   # MVC
  Solution2/DnsZoneRecordManager.slnx + src/server/DnsZoneRecordManager/   # Web API
                                          src/client/                      # Next.js app
  docs/REQUIREMENTS_ANALYSIS.md
  .opencode/  AGENTS.md  graphify-out/   # agent + knowledge-graph config (mirrors workspace standard)
```

## Run (empty scaffolds)

```bash
# Solution1 MVC
dotnet run --project Solution1/src/server/DnsZoneRecordManager
# Solution2 API
dotnet run --project Solution2/src/server/DnsZoneRecordManager
# Solution2 client
cd Solution2/src/client && npm run dev
```

## Tooling

- .NET SDK 10.0.400 (`net10.0`), `dotnet new gitignore` baseline.
- `/graphify` knowledge-graph skill available via `.opencode` (see `AGENTS.md`); run `graphify update .` after code changes.

## Docs

- `docs/REQUIREMENTS_ANALYSIS.md` — assessment requirements (all files read, all links checked).
- `docs/TOOLCHAIN_AND_PACKAGES.md` — every package + toolchain versions pinned (SDKs, NuGet, npm, wwwroot libs).
