# Toolchain & Packages — DnsZoneRecordManager

Snapshot date: 2026-09-17 (UTC). Everything below is read from the repo/SDKs, not guessed.
Re-verify any time with the commands in §5.

Project conventions: UTC timestamps everywhere (code: `DateTime.UtcNow`, never local time; docs/snapshots: UTC dates); XML doc comments (`///`) on all public controllers, handlers, validators, and models.

## 1. Toolchain (host)

| Tool | Version | Source / note |
|---|---|---|
| .NET SDK (active) | 10.0.400 (commit 14fbf8d527, MSBuild 18.9.6) | `dotnet --info` |
| .NET SDK (also installed) | 10.0.300 | `dotnet --list-sdks` |
| .NET workloads | macos, ios, maccatalyst (SDK-managed) | `dotnet --info` |
| Target framework (both web projects) | `net10.0` | both `.csproj` |
| OS / RID | macOS 27.0, `osx-arm64` | `dotnet --info` / `sw_vers` |
| Git | 2.54.0 (Apple Git-157) | `git --version` |
| Node.js | v26.7.0 | `node --version` |
| npm | 12.0.2 | `npm --version` |
| create-next-app (scaffolder) | 16.3.5 | `create-next-app --version` (npm cache workaround: `NPM_CONFIG_CACHE=/tmp/npm-cache-dns` — host `~/.npm/_cacache` is not writable) |
| VS Code | 1.138.0 (see build block below) | `code --version` (binary not on PATH; use `/Applications/Visual Studio Code.app/Contents/Resources/app/bin/code`) |
| .gitignore baseline | `dotnet new gitignore` template | repo root `.gitignore` |

### VS Code build (user-provided `code --version`, 2026-09-15)

```text
Version: 1.138.0
Commit: 7debcd0e2acdea1c52de81bf9ee1620444407dda
Date: 2026-09-15T07:24:32Z
Electron: 42.10.0
ElectronBuildId: 15109253
Chromium: 148.0.7778.280
Node.js: 24.18.1
V8: 14.8.178.38-electron.0
@github/copilot: 1.0.84-4
@github/copilot-sdk: 1.0.13
OS: Darwin arm64 27.0.0
```

### VS Code extensions (7 installed, `--list-extensions --show-versions`, 2026-09-17 UTC)

| Extension | Version | Purpose / relevance |
|---|---|---|
| `ms-dotnettools.csharp` | 2.140.9 | C# language support (Roslyn, debugging) — primary editor for both solutions |
| `ms-dotnettools.csdevkit` | 3.20.207 | C# Dev Kit (solution explorer, test runner for the xunit projects) |
| `ms-dotnettools.vscode-dotnet-runtime` | 3.1.0 | .NET runtime acquisition for the above |
| `csharpier.csharpier-vscode` | 11.0.0 | Format-on-save for C# — matches §7 (CSharpier 1.3.0 local tool) |
| `avaloniateam.vscode-avalonia` | 12.3.1 | Avalonia UI preview — installed, not used by this repo (no Avalonia projects) |
| `oderwat.indent-rainbow` | 8.3.1 | Indent guides (cosmetic) |
| `pkief.material-icon-theme` | 5.38.1 | File icons (cosmetic) |

## 2. Solution1 — MVC (`Solution1/src/server/DnsZoneRecordManager`)

`Microsoft.NET.Sdk.Web`, `<Nullable>enable</Nullable>`, `<ImplicitUsings>enable</ImplicitUsings>`.
Explicit `Program` class with `Main` + block-scoped namespace (no top-level statements) — project convention.

### NuGet

| Package | Version | Purpose |
|---|---|---|
| FluentValidation | 12.1.1 | Server-authoritative validation (REQUIREMENTS §7 rules) |
| Microsoft.EntityFrameworkCore.InMemory | 10.0.12 | In-Memory store behind Generic Repository + Unit of Work (brief default) |
| Microsoft.AspNetCore.OpenApi | 10.0.12 | OpenAPI document (`/openapi/v1.json`, Development) backing the Scalar reference |
| Scalar.AspNetCore | 2.17.4 | API reference UI (`/scalar`, Development) per https://scalar.com/products/api-references/integrations/aspnetcore/integration |

JSON API: `Controllers/Api` (`[ApiController]`, `api/zones` + `api/records`, enums as strings, `required` members); MVC view actions stay HTML-only and are excluded from the OpenAPI document by design.

### wwwroot client libs (shipped with the `dotnet new mvc` template, LibMan-style static files)

| Library | Version | How verified | License |
|---|---|---|---|
| Bootstrap (css + js + maps) | 5.3.3 | `bootstrap.min.css` header | MIT |
| jQuery | 3.7.1 | `jquery.min.js` header (`jQuery v3.7.1`) | MIT |
| jQuery Validation | 1.21.0 (07/17/2024) | `jquery.validate.min.js` header | MIT |
| jQuery Validation Unobtrusive | 4.0.0 | `jquery.validate.unobtrusive.min.js` `@version v4.0.0` | Apache-2.0 (.NET Foundation) |

## 3. Solution2 — Web API (`Solution2/src/server/DnsZoneRecordManager`)

`Microsoft.NET.Sdk.Web`, `net10.0`, Nullable + ImplicitUsings, `--use-controllers`.
Explicit `Program` class with `Main` + block-scoped namespace (no top-level statements) — project convention.

### NuGet

| Package | Version | Purpose |
|---|---|---|
| Microsoft.AspNetCore.OpenApi | 10.0.11 | Built-in OpenAPI document (`builder.Services.AddOpenApi()` / `app.MapOpenApi()` in `Program.cs`; serves `/openapi/v1.json`) |
| FluentValidation | 12.1.1 | Server-authoritative validation (REQUIREMENTS §7 rules) |
| Microsoft.EntityFrameworkCore.Sqlite | 10.0.12 | SQLite file store (`dnsmanager.db`, created by migrations) |
| Microsoft.EntityFrameworkCore.Design | 10.0.12 | `dotnet ef` migrations tooling (+ `dotnet-ef` 10.0.12 local tool) |
| Serilog.AspNetCore | 10.0.0 | Structured logging host integration (bootstrap logger, request logging) |
| Serilog.Sinks.Console | 6.1.1 | Console sink |
| Serilog.Sinks.File | 7.0.0 | Rolling `logs/` file sink (`shared: true` for parallel test hosts) |
| Scalar.AspNetCore | 2.17.4 | API reference UI (`/scalar`, Development) |

Hand-rolled CQRS (`IRequest`/`IRequestHandler`/`ISender`, no MediatR package — by design); single layer (handlers use `AppDbContext` directly).

### Test projects (`src/server/DnsZoneRecordManager.Tests`, xunit via `dotnet new xunit`, `net10.0`)

Shared stack, each test project references its server project. Solution1 adds EF InMemory + Mvc.Testing (view/flow coverage); Solution2 adds EF SQLite + Mvc.Testing (handler + endpoint coverage over real SQLite).

| Package | Version | Purpose |
|---|---|---|
| xunit | 2.9.3 | Test framework |
| xunit.runner.visualstudio | 3.1.4 | `dotnet test` discovery/execution |
| Microsoft.NET.Test.Sdk | 17.14.1 | Test host |
| coverlet.collector | 6.0.4 | Coverage (`--collect:"XPlat Code Coverage"`, `coverlet.runsettings` gate: 100/100/100) |
| Moq | 4.20.72 | Mocking |
| Bogus | 35.6.5 | Seed-shaped fixtures (`nahuexolab.com` data) |
| FluentValidation | 12.1.1 | Validator-under-test reference |
| FluentAssertions | 8.11.0 | Assertions |
| Microsoft.EntityFrameworkCore.InMemory | 10.0.12 | Solution1 isolated stores per test |
| Microsoft.EntityFrameworkCore.Sqlite | 10.0.12 | Solution2 isolated stores per test (`:memory:` + temp files) |
| Microsoft.AspNetCore.Mvc.Testing | 10.0.12 | Integration tests (S1: all views/flows/seed/CSV/prod env; S2: all endpoints/Scalar/seed/prod env) |

## 4. Solution2 — client (`Solution2/src/client`, Next.js App Router + `src/`)

Scaffold: `create-next-app@latest client --ts --tailwind --eslint --app --src-dir --import-alias "@/*" --use-npm --disable-git --yes`, then `shadcn@4.21.0 init/add` (button/input/label/select/card/table/dialog/badge/progress/sonner/alert/separator).
`package-lock.json`: `lockfileVersion` 3, `npm audit` → 3 moderate findings, all in the vitest dev toolchain (`@vitest/mocker`; fix needs breaking vitest 5 — accepted, test-runner only). Scripts: `dev` (`next dev`), `build`, `start`, `lint` (`eslint`), `test` / `test:watch` / `test:coverage` (`vitest`, gate 100/100/100/100), `format` / `format:check` (`prettier`).

### Production dependencies

| Package | Installed | Declared | Purpose |
|---|---|---|---|
| next | 16.3.5 | `16.3.5` | App Router, SSR/RSC, routing, build |
| react | 19.3.0 | `^19.3.0` | UI runtime |
| react-dom | 19.3.0 | `^19.3.0` | DOM renderer (RSC/server components) |
| next-themes | 0.4.6 | `^0.4.6` | Dark mode (`ThemeProvider`, class strategy) |
| class-variance-authority | 0.7.1 | `^0.7.1` | shadcn variant styles |
| lucide-react | 1.47.0 | `^1.47.0` | Icons (shadcn) |
| sonner | 2.0.8 | `^2.0.8` | Toasts (`Toaster`) |
| tw-animate-css | 1.4.0 | `^1.4.0` | shadcn animations (Tailwind v4) |
| @base-ui/react | 1.8.0 | `^1.8.0` | Headless primitives behind shadcn components |

### Development dependencies (16)

| Package | Installed | Declared | Purpose |
|---|---|---|---|
| typescript | 5.9.3 | `^5` | Type-checking (kept major 5: TS 7 would risk the Next 16 build) |
| tailwindcss | 4.3.3 | `^4` | Utility CSS (v4, `@import "tailwindcss"` in `globals.css`) |
| @tailwindcss/postcss | 4.3.3 | `^4` | Tailwind v4 PostCSS bridge (`postcss.config.mjs`) |
| postcss (transitive peer) | 8.5.28 | — | CSS pipeline |
| eslint | 9.39.5 | `^9` | Linting (flat config `eslint.config.mjs`) |
| eslint-config-next | 16.3.5 | `16.3.5` | Next.js lint rules |
| prettier | 3.9.8 | `^3.9.8` | Formatting (`npm run format` / `format:check`; defaults, no config file; `.prettierignore` covers `.next`) |
| @types/node | 20.19.43 | `^20` | Node typings |
| @types/react | 19.3.0 | `^19` | React typings |
| @types/react-dom | 19.3.0 | `^19` | React-DOM typings |
| vitest | 3.2.7 | `^3` | Test runner (pinned v3: v5 needs newer `@types/node` than `^20`) |
| @vitest/coverage-v8 | 3.2.7 | `^3` | Coverage gate 100/100/100/100 (`vitest.config.ts`, shadcn `ui/*` excluded as vendored) |
| @vitejs/plugin-react | 4.7.0 | `^4` | React transform for vitest |
| jsdom | 30.1.0 | `^30` | DOM for component tests |
| @testing-library/react | 16.3.3 | `^16` | Component rendering |
| @testing-library/jest-dom | 7.0.1 | `^7` | DOM matchers |
| @testing-library/user-event | 14.6.7 | `^14` | Interaction simulation |

Also scaffolded by the template (not packages): `AGENTS.md` + `CLAUDE.md` agent guides, `next.config.ts`, `postcss.config.mjs`, `public/` SVGs.

## 5. Re-verify commands

```bash
dotnet --info | head -12
grep -A3 PackageReference Solution1/src/server/DnsZoneRecordManager/DnsZoneRecordManager.csproj Solution2/src/server/DnsZoneRecordManager/DnsZoneRecordManager.csproj
head -3 Solution1/src/server/DnsZoneRecordManager/wwwroot/lib/bootstrap/dist/css/bootstrap.min.css
cd Solution2/src/client && node -e "for (const p of ['next','react','react-dom','tailwindcss','typescript','eslint','eslint-config-next','prettier','@types/node','@types/react','@types/react-dom']) console.log(p, require('./node_modules/'+p+'/package.json').version)"
dotnet csharpier check .
npm run format:check --prefix Solution2/src/client
"/Applications/Visual Studio Code.app/Contents/Resources/app/bin/code" --list-extensions --show-versions
```

## 6. Deliberately NOT installed yet (next implementation steps)

- Solution1: done (EF Core InMemory wired + seeded). Remaining: per-solution zip for delivery.
- Solution2: done (SQLite + migrations, CQRS, Serilog, Scalar, shadcn client). Remaining: per-solution zip for delivery.
- Client: API-client is hand-rolled `lib/api.ts`; state/query libs only if justified later (YAGNI).

## 7. Formatters (project decision)

| Scope | Tool | Version | How pinned | Commands |
|---|---|---|---|---|
| C# (both solutions, incl. `.csproj`) | [CSharpier](https://csharpier.com/docs/About) | 1.3.0 | Local dotnet tool, `.config/dotnet-tools.json` (restore via `dotnet tool restore`) | `dotnet csharpier format .` / `dotnet csharpier check .` |
| Next.js client | [Prettier](https://prettier.io/docs/) | 3.9.8 | devDependency (`^3.9.8`); defaults, no config file; `.prettierignore` covers `.next` | `npm run format` / `npm run format:check --prefix Solution2/src/client` |
