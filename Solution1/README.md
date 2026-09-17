# Solution1 — DNS Manager (ASP.NET Core MVC)

Take-home assessment implementation: DNS Zones + DNS Records manager (Query, Create, Modify, Delete) for a non-technical customer persona.

## Stack

- ASP.NET Core MVC, .NET 10 (`net10.0`, Nullable + ImplicitUsings, explicit `Program` + `Main`, no top-level statements)
- Razor views + Bootstrap 5.3.3 + jQuery 3.7.1 (brief default), shadcn-style CRM theme with runtime dark/light switch
- Generic Repository + Unit of Work over EF Core 10.0.12 **In-Memory** store (brief default)
- FluentValidation 12.1.1 (server-authoritative) + DataAnnotations mirrors for instant client feedback
- Tests: xunit 2.9.3 + Moq 4.20.72 + Bogus 35.6.5 + FluentAssertions 8.11.0 + Mvc.Testing 10.0.12 — **100% line/branch/method coverage** (gate below)

## Build / run / test

```bash
dotnet build Solution1/DnsZoneRecordManager.slnx
dotnet run --project Solution1/src/server/DnsZoneRecordManager
dotnet test Solution1/DnsZoneRecordManager.slnx
# coverage gate (excludes generated Razor/state-machine code; hand-written code must be 100/100/100)
dotnet test Solution1/DnsZoneRecordManager.slnx --settings Solution1/src/server/DnsZoneRecordManager.Tests/coverlet.runsettings --collect:"XPlat Code Coverage"
dotnet csharpier check .
```

Seed zone `nahuexolab.com` + 5 records (4× NS @ apex + 1× TXT `_dmarc`) is inserted automatically on first run.

## Features (every REQUIREMENTS §8 item)

- Zones: list/search, create, rename, delete with cascade preview — grid shows `n / 10` meter + NS badge per zone
- Records: zone picker, text search, type filter, sortable grid, create/edit/delete, per-row actions, CSV export of the filtered grid
- Feedback: inline field errors + validation summary, success/error toasts, delete confirmations, count meter `n / 10 records, x NS`
- Rules enforced server-side (§7): ≥4 NS per zone, ≤10 records per zone, types A/AAAA/CNAME/NS/TXT with per-type data checks, CNAME exclusivity, no duplicate zones/records (unique index), FQDN derived
- UI: mobile-friendly responsive cards/tables, dark/light theme persisted at runtime with smooth transitions

## Notes / assumptions (see REQUIREMENTS §7 resolutions)

- Store is In-Memory per the brief (process-lifetime; Solution2 uses SQLite for persistence).
- SOA/`$TTL` header lines are not editable records; Modified = `UpdatedUtc` (UTC everywhere); export is records-only CSV.
- Zone delete cascades its records after confirmation. XML doc comments on all public APIs.

## Cut for the 8h box

- No pagination (grids cap at 10 records/zone by rule), no auth (single-user LOB assumption), no zone-file import (seed covers the sample).

## Security audit (against the 25-point CRUD checklist)

Applies as-is: input validation (FluentValidation server-side + DataAnnotations mirrors), no entity binding (ViewModels only — no mass assignment), LINQ/EF only (no SQL injection), Razor auto-encoding (no XSS; no `Html.Raw` anywhere), `[ValidateAntiForgeryToken]` on every POST (CSRF), `UseHttpsRedirection` + HSTS outside Development (HTTPS), no secrets in code/config, generic error page in production (no stack leaks), `X-Content-Type-Options: nosniff` + `Referrer-Policy` + `X-Frame-Options: SAMEORIGIN` headers (no CSP: the inline pre-paint theme script requires it), mutation audit logging (create/rename/delete, no secrets in model), `dotnet list package --vulnerable` clean (2026-09-17).
Not applicable (single-user LOB, no accounts): authentication, authorization/IDOR, passwords, JWT, CORS (same-origin MVC, none configured), rate limiting, file uploads, DB permissions (InMemory).
Accepted trade-offs: duplicate-race window (pre-checked + unique index; concurrent same-name creates surface the generic prod error page), last-writer-wins edits (standard LOB behavior, no concurrency tokens).

## Design patterns (GoF, as needed — nothing speculative)

- **Strategy**: per-type RDATA validation (`IRecordDataValidator`: IPv4/IPv6/Hostname/Text + registry) — new types plug in without touching callers.
- **Factory Method**: `ServiceResult<T>.Ok/Fail` — single construction path for outcomes.
- **Repository + Unit of Work**: `IGenericRepository<T>` over EF Core, one shared context per request.
- MVC itself (controllers/views), DI container as factory, Razor layout as Template Method for pages.
