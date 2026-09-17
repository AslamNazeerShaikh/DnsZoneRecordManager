# Solution1 — DNS Manager (ASP.NET Core MVC)

Take-home assessment implementation: DNS Zones + DNS Records manager (Query, Create, Modify, Delete) for a non-technical customer persona.

## Stack

- ASP.NET Core MVC, .NET 10 (`net10.0`, Nullable + ImplicitUsings, explicit `Program` + `Main`, no top-level statements)
- Razor views + Bootstrap 5.3.3 + jQuery 3.7.1 (brief default), Inter variable font (self-hosted latin woff2), shadcn-style CRM theme with runtime dark/light switch
- Generic Repository + Unit of Work over EF Core 10.0.12 **In-Memory** store (brief default)
- FluentValidation 12.1.1 (server-authoritative) + DataAnnotations mirrors for instant client feedback
- OpenAPI 10.0.12 + Scalar 2.17.4 API reference at `/scalar` (Development only); lists JSON API operations (HTML view actions are not API operations)
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

Seed zones (`nahuexolab.com` + 5 records: 4× NS @ apex + 1× TXT `_dmarc`; `demo.example` + 8 records: 4× NS plus one A/AAAA/CNAME/TXT each) are inserted automatically on first run.

## Features (every REQUIREMENTS §8 item)

- Zones: list/search, create, rename, delete with cascade preview — grid shows `n / 10` meter + NS badge per zone
- Records: zone picker, text search, type filter, sortable grid, create/edit/delete, per-row actions, CSV export of the filtered grid
- Feedback: inline field errors + validation summary, success/error toasts, delete confirmations, count meter `n / 10 records, x NS`
- Rules enforced server-side (§7): ≥4 NS per zone, ≤10 records per zone, types A/AAAA/CNAME/NS/TXT with per-type data checks, CNAME exclusivity, no duplicate zones/records (unique index), FQDN derived
- UI: mobile-friendly responsive cards/tables, dark/light theme persisted at runtime with smooth transitions

## JSON API (`Controllers/Api`, Scalar reference + client apps)

MVC view actions stay HTML-only; the JSON API is separate (`[ApiController]`, lowercase `api/zones` + `api/records` routes, enums as strings, `required` members → framework 400 on missing fields, no cookie auth by design):

| Method + route | Success | Failures |
|---|---|---|
| `GET /api/zones?search=` | 200 list | — |
| `GET /api/zones/{id}` · `POST /api/zones` | 200 / 201 + Location | 404 / 400 / 409 |
| `PUT /api/zones/{id}` · `DELETE /api/zones/{id}` | 200 / 204 | 400 / 404 / 409 |
| `GET /api/records?zoneId=&search=&type=` | 200 list | 404 (unknown zone) |
| `GET /api/records/{id}` · `POST /api/records` | 200 / 201 + Location | 400 / 404 / 409 |
| `PUT /api/records/{id}` · `DELETE /api/records/{id}` | 200 / 204 | 400 (incl. NS floor) / 404 / 409 |

Failure bodies are `{ "errors": [...] }` mapped from the error catalog (`NotFound` → 404, `Conflict` → 409, else 400). Scalar UI: `/scalar` (Development).

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

## Error catalog (Result pattern — documented exceptions and errors)

Services never throw for domain failures; every failure is a typed `ServiceError` (`ErrorKind` + guided message). Controllers branch on `HasError(kind)` — never on message text — so endpoint behavior is deterministic and no internals leak.

| `ErrorKind` | Meaning | Example messages | Endpoint mapping |
|---|---|---|---|
| `Validation` | Shape rule breached (FluentValidation) | "TTL must be a positive number of seconds." | Re-render form with inline errors |
| `NotFound` | Zone/record id unknown | "Zone not found." / "Record not found." | 404 |
| `Conflict` | Duplicate zone/record (A6) | "Zone 'x' already exists." / "This exact record already exists in the zone." | Re-render form with message |
| `RuleViolation` | A1/A2/CNAME rule breached | "A zone must keep at least 4 NS records." / "A zone cannot hold more than 10 records." / CNAME messages | Re-render form (or danger toast on confirmed delete) |

Unexpected exceptions (out-of-memory, I/O, duplicate-race `DbUpdateException`) propagate to the production exception handler → generic error page, details in server logs only.
