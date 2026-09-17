# Solution2 — DNS Manager API + Web Client (scaffold)

Take-home assessment, second solution: ASP.NET Core Web API + Next.js client for DNS Zones + DNS Records (Query, Create, Modify, Delete). **Status: empty scaffold** — template code only, no domain implementation yet (see build order in `docs/REQUIREMENTS_ANALYSIS.md` §9: Solution1 first, this one second).

## Planned stack (per `docs/REQUIREMENTS_ANALYSIS.md` §5)

- ASP.NET Core Web API, .NET 10 (`net10.0`, Nullable + ImplicitUsings, explicit `Program` + `Main`), `--use-controllers`
- Hand-rolled CQRS commands/queries + handlers (**no MediatR**, by design), EF Core + SQLite (file-backed, zero-setup)
- Client: Next.js 16.3.5 + React 19 + TypeScript 5 + Tailwind CSS 4 (`src/client`, App Router + `src/`)
- REST JSON between them; validation mirrored client + server, server authoritative (FluentValidation 12.1.1 already referenced)
- Same domain rules as Solution1 (analysis doc §7): ≥4 NS per zone, ≤10 records per zone, types A/AAAA/CNAME/NS/TXT, no duplicates, guided UX

## What exists today

- `src/server/DnsZoneRecordManager/`: template WeatherForecast controller, OpenAPI document at `/openapi/v1.json` (Development, via `Microsoft.AspNetCore.OpenApi` 10.0.11)
- `src/server/DnsZoneRecordManager.Tests/`: xunit toolchain smoke tests (3 passing)
- `src/client/`: `create-next-app` scaffold (dev/build/start/lint scripts, 0 audit vulnerabilities)

## Build / run / test

```bash
dotnet build Solution2/DnsZoneRecordManager.slnx
dotnet run --project Solution2/src/server/DnsZoneRecordManager --launch-profile https
# API: https://localhost:7264 (HTTP :5126 redirects); OpenAPI: https://localhost:7264/openapi/v1.json
cd Solution2/src/client && npm run dev     # web client on http://localhost:3000
dotnet test Solution2/DnsZoneRecordManager.slnx
```

## Next steps (when authorized)

1. EF Core SQLite + domain model/validation (reuse Solution1's §6 model + §7 rules).
2. Hand-rolled CQRS endpoints for zones/records (+ CSV export).
3. Next.js grid UI (zone picker, search, type filter, toasts, export) against the API.
4. Per-solution delivery zip + full test coverage per the Solution1 gate.
