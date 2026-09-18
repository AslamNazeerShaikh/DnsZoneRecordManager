# Solution2 — DNS Manager API + Web Client

Take-home assessment, second solution: ASP.NET Core Web API + Next.js client for DNS Zones + DNS Records (Query, Create, Modify, Delete) for a non-technical customer persona. **Status: implemented** (backend + client, tested).

## Stack

- ASP.NET Core Web API, .NET 10 (`net10.0`, Nullable + ImplicitUsings, explicit `Program` + `Main`)
- Hand-rolled CQRS commands/queries + handlers over `ISender` (**no MediatR**, by design), single layer
- EF Core 10.0.12 + SQLite file (`dnsmanager.db`, created by `InitialCreate` migration on startup)
- Serilog 10.0.0 structured logging (console + rolling `logs/` files, config-driven, request logging)
- FluentValidation 12.1.1 (command validators, server authoritative); typed `ServiceResult` (`ErrorKind`: Validation/NotFound/Conflict/RuleViolation)
- Scalar 2.17.4 API reference at `/scalar` (Development) + OpenAPI at `/openapi/v1.json`
- Client: Next.js 16.3.5 + React 19.3.0 + TypeScript 5.9 + Tailwind CSS 4.3.3, **shadcn** components (button/input/label/select/card/table/dialog/badge/progress/sonner/alert/separator), `next-themes` dark mode, Inter font, warm CRM palette
- Tests: xunit + Moq + Bogus + FluentValidation + FluentAssertions + Mvc.Testing + EF SQLite — **100% line/branch/method coverage** (gate below); client: vitest 3.2.7 + Testing Library + jsdom — **100% lines/branches/functions/statements** (`npm run test:coverage`)

## Build / run / test

### Prerequisites

```bash
dotnet --info            # needs .NET SDK 10.0.400
node --version           # needs Node 26 (npm 12+)
```

### HTTPS dev certificate (trust once per machine)

The API serves HTTPS with a self-signed localhost cert; browsers and the Next.js client need it trusted:

```bash
dotnet dev-certs https --check    # verify a valid cert exists
dotnet dev-certs https --trust    # install + trust it (macOS: approves via Keychain prompt)
```

Without trust, the browser shows a warning (Advanced → Continue) and API calls from the client fail.

### Restore, clean, rebuild

```bash
dotnet restore Solution2/DnsZoneRecordManager.slnx
dotnet clean Solution2/DnsZoneRecordManager.slnx
dotnet build Solution2/DnsZoneRecordManager.slnx
dotnet build Solution2/DnsZoneRecordManager.slnx -c Release -p:TreatWarningsAsErrors=true
dotnet csharpier check .
cd Solution2/src/client && npm install   # restores node_modules (not committed)
```

### Launch backend + frontend (CLI)

`src/server/.../Properties/launchSettings.json` profiles:

| Profile | Command | URLs |
|---|---|---|
| `https` (API) | `dotnet run --project Solution2/src/server/DnsZoneRecordManager --launch-profile https` | `https://localhost:7264` (+ `http://localhost:5126` redirects) |
| `dev` (client) | `cd Solution2/src/client && npm run dev` | `http://localhost:3000` |

```bash
# terminal 1: API (migrates + seeds SQLite on first boot)
dotnet run --project Solution2/src/server/DnsZoneRecordManager --launch-profile https
# terminal 2: web client (talks to NEXT_PUBLIC_API_URL, default https://localhost:7264)
cd Solution2/src/client && npm run dev
```

Then open `http://localhost:3000`. Scalar reference: `https://localhost:7264/scalar`. Stop each with `Ctrl+C`.

Seed zones (`nahuexolab.com` + `demo.example`, 13 records) migrate + seed automatically on first API boot. The client talks to `NEXT_PUBLIC_API_URL` (defaults to `https://localhost:7264`).

### Test + coverage

```bash
dotnet test Solution2/DnsZoneRecordManager.slnx
# coverage gate (excludes generated code + ef design-time factory; hand-written code must be 100/100/100)
dotnet test Solution2/DnsZoneRecordManager.slnx --settings Solution2/src/server/DnsZoneRecordManager.Tests/coverlet.runsettings --collect:"XPlat Code Coverage"
cd Solution2/src/client && npm run test:coverage   # vitest gate: 100/100/100/100 (shadcn ui/* excluded as vendored)
cd Solution2/src/client && npm run lint && npm run format:check && npm run build
```

## Backend layout (`src/server/DnsZoneRecordManager`)

```text
Models/         DnsZone, DnsRecord, RecordType (A/AAAA/CNAME/NS/TXT)
Data/           AppDbContext, DbSeeder, Migrations/InitialCreate, DesignTimeDbContextFactory (ef tooling only)
Results/        ErrorKind, ServiceError, ServiceResult<T> (never throws for domain failures)
Validation/     DnsRules (patterns/limits/normalization), IRecordDataValidator strategies, command validators
Cqrs/           IRequest/Handler/ISender (hand-rolled Mediator) + Zones/ + Records/ commands, handlers, DTOs
Controllers/    ZonesController + RecordsController (api/zones, api/records; thin, map Result→201/200/204/400/404/409)
Program.cs      Serilog bootstrap (exit codes), SQLite, validators, handlers, CORS (ClientOrigins), security headers, Scalar
appsettings.json  ConnectionStrings, ClientOrigins (default localhost:3000), Serilog sinks
```

Conventions: async/await with `CancellationToken` everywhere, `AsNoTracking` reads, single-query projections (no N+1), UTC timestamps, `///` docs on all public APIs, structured mutation logs (no secrets in model).

## CQRS without MediatR (verdict from 5 articles, 2026-09-18)

Read: Milan Jovanović (direct handler injection), Mukesh Murugan (FrozenDictionary dispatcher + benchmarks), TheCodeMan (split command/query dispatchers), ExpertMinds (MediatR-shaped dispatcher + behaviors), Adrian Bailador (minimal interfaces, dispatcher optional).

| Approach | Fit for this CRUD API |
|---|---|
| Direct handler injection (Milan/Adrian) | Simplest, but controllers collect N dependencies as endpoints grow |
| Split dispatchers + per-call `MakeGenericType` (TheCodeMan) | Works; slowest dispatch per published benchmarks |
| Full behaviors/notifications/ValueTask (Mukesh, complete) | Built for scale we don't have; speculative here |
| **FrozenDictionary core only (adopted)** | **O(1) typed dispatch, no `dynamic`, no new deps, ~60 lines** |

Adopted: Mukesh's `FrozenDictionary<Type, Wrapper>` registry + startup assembly scan (no Scrutor), our `IRequest`/`Handler`/`ISender` shapes, plain `Task` (ValueTask single-consumption rules buy nothing against ms-scale SQLite calls), validation/logging inline in handlers (no generic behavior layer to maintain), no notifications (nothing crosses processes). Controllers keep one `ISender` dependency; unknown requests fail fast with `InvalidOperationException` (tested).

## Client layout (`src/client`)

```text
src/app/            layout (Inter, ThemeProvider, header, Toaster) · page (dashboard) · zones/ · records/
src/components/    ui/* (shadcn) · site-header · theme-provider
src/lib/           api.ts (typed fetch client, ApiError), format.ts (UTC), utils.ts
```

Pages mirror Solution1: dashboard with live counts, zones grid (search/meter/dialogs/toasts), records grid (zone picker/search/type/CSV export/meter/dialogs). Filters apply on Apply; delete resets to the full grid. shadcn + `next-themes` dark mode; warm light theme by default.

## Logic paths (backend + client flows)

Backend request path: Serilog request logging → exception handler (ProblemDetails) → security headers → HTTPS redirect → routing → CORS (`ClientOrigins`) → controllers → `ISender` → handler → EF SQLite. Controllers are thin: validate route/body shape via `required` members (framework 400s), send one command/query, map the typed result (201+Location / 200 / 204, or `{errors[]}` as 404/409/400).

- **Validation pipeline**: `required` JSON members (missing → framework 400) → FluentValidation command validators (shape + per-type Strategy checks) → handler DB rules (normalize → duplicates `Conflict`, CNAME/ceiling/floor `RuleViolation`, missing `NotFound`). Client validation is cosmetic only.
- **Zone flows**: list/search projection (single query with counts) → get → create/rename (validate → duplicate-check → stamp UTC → audit log) → delete (cascade).
- **Record flows**: filtered list (one joined query, FQDN derived in SQL) → get → create/update (validate → guard siblings → stamp UTC → audit log) → delete (NS floor re-checked) → CSV export reuses the list filter with quoting.
- **Client flows** (`lib/api.ts` typed client; `ApiError` carries status + messages): pages fetch on filter Apply, mutate then refresh, toasts on every outcome, delete resets filters, export is a plain download link with the active filter params.
- **Theme**: `next-themes` class strategy (light warm default, system fallback, dark variant); Inter font; CSS variables only, no hard-coded colors.

## How to use (first-run walkthrough)

1. Trust the dev cert, launch API + client, open `http://localhost:3000` — dashboard shows 2 zones / 13 records.
2. Open **Zones**: search `demo`, clear; note meters and footer total.
3. **New zone**: save empty (dialog stays clean — no phantom errors), save `myzone.example` (toast + row), reopen (form is fresh), try `nahuexolab.com` (conflict error), rename, delete (counts in confirm, toast, row gone).
4. Open **Records**: pick `demo.example` + Apply (trigger keeps showing "demo.example", meter card appears); Type `A`; search `alias`; **Clear**.
5. **New record** with no zone filter: dialog asks to pick a zone first (button always enabled); pick `demo.example`, add `mail` A `192.0.2.10`; try bad IP / duplicate / CNAME-on-`www` (each error explained).
6. Delete an NS in `nahuexolab.com` (blocked at the 4-NS floor); delete a TXT (grid resets to all records).
7. **Export CSV** filtered and unfiltered — download matches the grid; toggle theme sun/moon (persists).
8. Open `/scalar` on the API for the same operations as documented JSON.

## Logic paths (backend + client flows)

## Notes / assumptions

- No AuthN/AuthZ by requirement (single-user LOB); JSON API carries no cookies, so CSRF N/A; CORS locked to configured `ClientOrigins`.
- Same domain resolutions as Solution1 (REQUIREMENTS §7): SOA headers not editable, Modified = `UpdatedUtc`, records-only CSV export, cascading zone delete.
- Result idioms mirror Solution1 (`ErrorKind` set identical); Serilog program shape adapted from the InventoryManagement reference.
- Migrations: `dotnet ef migrations add <Name> --project Solution2/src/server/DnsZoneRecordManager` (design-time factory included).

## Security audit (final, 2026-09-18)

Verified live: `nosniff` + `Referrer-Policy` + `SAMEORIGIN` headers on API responses; CORS preflight from unlisted origins returns no ACAO header (browser-blocked); allowed origin scoped to `ClientOrigins`. No secrets in code/config/client (only `NEXT_PUBLIC_API_URL`); `dotnet list package --vulnerable` clean; `npm audit` shows 3 moderate findings, all inside the vitest dev toolchain (`@vitest/mocker`, test-runner only, fix requires breaking vitest 5 upgrade — accepted). All mutating endpoints validate server-side (missing JSON members → framework 400, bad enums → 400, all §7 rules → typed 400/404/409); client validation is cosmetic only. No raw SQL in production code (one `sqlite_master` probe lives in tests). No auth/session/logs of secrets.
