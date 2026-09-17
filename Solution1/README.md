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

### Prerequisites

```bash
dotnet --info            # needs .NET SDK 10.0.400 (10.0.300 also installed here)
dotnet --list-sdks
```

### HTTPS dev certificate (trust once per machine)

Browsers reject the self-signed localhost cert until trusted:

```bash
dotnet dev-certs https --check    # verify a valid cert exists
dotnet dev-certs https --trust    # install + trust it (macOS: approves via Keychain prompt)
```

Without trust, HTTPS still works but the browser shows a warning (Advanced → Continue).

### Restore, clean, rebuild

```bash
dotnet restore Solution1/DnsZoneRecordManager.slnx
dotnet clean Solution1/DnsZoneRecordManager.slnx
dotnet build Solution1/DnsZoneRecordManager.slnx
dotnet build Solution1/DnsZoneRecordManager.slnx -c Release -p:TreatWarningsAsErrors=true
dotnet csharpier check .          # C# formatting (CSharpier 1.3.0 local tool)
```

### Launch (CLI, HTTPS config)

`Properties/launchSettings.json` defines two profiles:

| Profile | Command | URLs |
|---|---|---|
| `http` | `dotnet run --project Solution1/src/server/DnsZoneRecordManager --launch-profile http` | `http://localhost:5258` |
| `https` | `dotnet run --project Solution1/src/server/DnsZoneRecordManager --launch-profile https` | `https://localhost:7103` (+ `http://localhost:5258` redirects to HTTPS) |

```bash
# recommended: HTTPS profile (HTTP auto-redirects, HSTS active outside Development)
dotnet run --project Solution1/src/server/DnsZoneRecordManager --launch-profile https
```

Then open `https://localhost:7103` in the browser. Stop with `Ctrl+C`.

### Test + coverage

```bash
dotnet test Solution1/DnsZoneRecordManager.slnx
# coverage gate (excludes generated Razor/state-machine code; hand-written code must be 100/100/100)
dotnet test Solution1/DnsZoneRecordManager.slnx --settings Solution1/src/server/DnsZoneRecordManager.Tests/coverlet.runsettings --collect:"XPlat Code Coverage"
```

Seed zones (`nahuexolab.com` + 5 records: 4× NS @ apex + 1× TXT `_dmarc`; `demo.example` + 8 records: 4× NS plus one A/AAAA/CNAME/TXT each) are inserted automatically on first run.

## UI paths (every page and what it does)

Layout (`Views/Shared/_Layout.cshtml`): top nav (brand → dashboard, Zones, Records), dark/light toggle (sun/moon, persisted in `localStorage`, applied pre-paint to avoid flashing), toast stack (reads `TempData["Toast"]`/`["ToastType"]`, auto-dismisses in 4s), footer with the A1–A3 rule reminder. All pages render inside it; jQuery drives toasts only (no auto-submits — grids refresh on Apply).

| Route | Page | Behavior |
|---|---|---|
| `/` | Dashboard | Hero + Zones/Records cards + rule chips; static links, no data load |
| `/Zones?search=` | Zone grid | Search box + Clear; rows link to the zone's records; per-row `n / 10` meter + NS badge + Updated (UTC); per-row Records/Rename/Delete; footer totals zone count; empty-state card when no match |
| `/Zones/Create` | New zone | Name field (DataAnnotations mirror server rules) + Allowed-values guide; Save → grid + success toast, or re-render with inline errors |
| `/Zones/Edit/{id}` | Rename zone | Same form prefilled (reuses the Create view); unknown id → 404 |
| `/Zones/Delete/{id}` | Delete zone | Cascade preview (record + NS counts); Delete → grid + toast; unknown id → 404 |
| `/Records?zoneId=&search=&type=` | Record grid | Zone picker + search + type filter + Apply/Clear (Apply-only refresh; selections persist via model-bound dropdowns); Export CSV keeps current filters; zone meter card when filtered (`n / 10`, NS badge); grid FQDN/Name/Type badge/TTL/Data (wrapped, never truncated)/Modified (UTC)/Edit/Delete; footer totals shown-row count |
| `/Records/Create?zoneId=` | New record | Zone dropdown (preselects the picker zone), name/type/TTL/data + Allowed-values guide (per-type formats, lengths, TTL range); Cancel returns to the grid (filter kept only for real zone ids) |
| `/Records/Edit/{id}` | Edit record | Same form prefilled, zone locked (records never move zones); unknown id → 404 |
| `/Records/Delete/{id}` | Delete record | Shows FQDN/type/data; NS deletes below the 4-NS floor bounce back with a danger toast; success lands on the **unfiltered** grid |
| `/Records/Export?...` | CSV download | Same filters as the grid; `dns-records-{UTC stamp}.csv`; unknown zone → 404 |
| `/Home/Privacy` · `/Home/Error` | Static/error | Privacy page; generic error page (no internals) |
| `/scalar` · `/openapi/v1.json` | API reference (Development) | Scalar UI + OpenAPI document for the JSON API below |

## Logic paths (backend flows)

Request pipeline (`Program.BuildApp`): security headers → HTTPS redirect → routing → authorization → static assets → controllers; EF InMemory + validators + services registered scoped; seed runs when the store is empty. Every layer is `///`-documented.

- **Validation pipeline**: form posts hit DataAnnotations first (instant client feedback via jQuery Unobtrusive); controllers check `ModelState`, then services normalize (lowercase, trim, drop trailing dot) and run FluentValidation (`ZoneValidator`/`RecordValidator` with the `IRecordDataValidator` Strategy per type); failures return typed `ServiceError`s, never throws.
- **Zone create**: normalize → shape-validate (`Validation`) → duplicate check (`Conflict`) → stamp UTC → save → toast + redirect. Rename adds a not-found check and self-excluding duplicate check. Delete loads, cascades records, toasts.
- **Record create**: zone must exist (`NotFound`) → shape-validate (`Validation`) → `GuardSiblings` against zone mates: exact duplicate (`Conflict`), CNAME either direction (`RuleViolation`), 10-record ceiling on creates only (`RuleViolation`) → stamp UTC → save. Update reuses the guard excluding self, plus NS→non-NS conversion blocked at the 4-NS floor. Delete blocks NS removal at the floor; everything else deletes.
- **Result mapping**: MVC re-renders forms with messages, 404s on `NotFound`; JSON API maps `NotFound` → 404, `Conflict` → 409, else 400 with `{ "errors": [...] }`. Missing JSON members are rejected with framework 400 before service code runs.
- **Export**: reuses the list flow (same filters), then renders `FQDN,Zone,Name,Type,TTL,Data` with CSV quoting.
- **Theme**: stored choice → OS preference → light; toggle flips `data-bs-theme` (Bootstrap color modes + CSS variables) with transitions.
- **Toasts**: `TempData` survives exactly one redirect; layout renders one Bootstrap toast.

## How to use (first-run walkthrough)

1. Launch the HTTPS profile and open `https://localhost:7103` — the dashboard shows both seed zones.
2. Open **Zones**: try the search box (`demo` vs `zzz`), note each row's `n / 10` meter and NS badge, and the footer total.
3. **New zone**: submit empty (inline error), then `myzone.example` (toast + row appears); try `nahuexolab.com` (duplicate message); rename it via **Rename**; delete it via **Delete** (cascade preview) — filters reset, toast confirms.
4. Open **Records**: pick `demo.example` in the zone dropdown, click **Apply** (grid filters only on Apply; selection persists); filter Type `A`; search `alias`; **Clear** resets.
5. **New record** in `demo.example`: read the Allowed-values guide; add `mail` A `192.0.2.10` TTL `300` (toast + row, FQDN derived); try `not-an-ip` (type-fit error), a duplicate (conflict error), and a `CNAME` on `www` (exclusivity error).
6. Try deleting an NS record in `nahuexolab.com` (blocked at the 4-NS floor with a danger toast).
7. **Export CSV** with a filter active — the download matches the grid.
8. Toggle the sun/moon button (persists across reloads), shrink the window (responsive tables/cards).
9. Open `/scalar` for the JSON API reference; `GET /api/zones` in the browser returns the same data as JSON.

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
