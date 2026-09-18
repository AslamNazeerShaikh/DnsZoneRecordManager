# Graph Report - DnsZoneRecordManager  (2026-09-18)

## Corpus Check
- 164 files · ~61,512 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 1372 nodes · 2754 edges · 107 communities (60 shown, 18 thin omitted)
- Extraction: 91% EXTRACTED · 9% INFERRED · 0% AMBIGUOUS · INFERRED: 247 edges (avg confidence: 0.84)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `1781b533`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- Solution1/src/server/DnsZoneRecordManager.Tests/DnsZoneRecordManager.Tests.csproj
- opencode.json
- package.json
- Solution1/src/server/DnsZoneRecordManager.Tests/UnitTest1.cs
- HomeController
- compilerOptions
- http
- http
- Fact
- ServiceResult
- .SendAsync
- .BuildApp
- Task
- graphify.js
- eslint.config.mjs
- postcss.config.mjs
- app/page.tsx
- site.js
- What You Must Do When Invoked
- Solution2/src/server/DnsZoneRecordManager.Tests/DnsZoneRecordManager.Tests.csproj
- DNS Zone & Records Manager — Requirements Analysis (accurate, file-by-file)
- Toolchain & Packages — DnsZoneRecordManager
- Opencode Configuration for DnsZoneRecordManager
- graphify reference: extra exports and benchmark
- .Create
- graphify reference: query, path, explain
- DnsRecord
- graphify reference: add a URL and watch a folder
- graphify reference: commit hook and native CLAUDE.md integration
- graphify reference: incremental update and cluster-only
- client/README.md
- graphify reference: GitHub clone and cross-repo merge
- graphify reference: transcribe video and audio
- AGENTS.md
- extraction-spec.md
- client/AGENTS.md
- RecordDeleteViewModel
- IGenericRepository
- IRecordDataValidator
- DnsZone
- Solution1 — DNS Manager (ASP.NET Core MVC)
- Records/Create.cshtml
- RecordDeleteViewModel
- RecordIndexViewModel
- Zones/Create.cshtml
- Zones/Delete.cshtml
- Zones/Index.cshtml
- ServiceResult
- DnsZoneRecordManager.Models
- components.json
- .Update
- DnsZoneRecordManager.Data
- apiErrorMessages
- UpdateRecordRequest
- InitialCreate
- RecordType
- records/page.tsx
- layout.tsx
- RecordFormViewModel
- zones/page.tsx
- ErrorKind
- Solution2/src/server/DnsZoneRecordManager.Tests/IntegrationTests.cs
- progress.tsx
- cn
- devDependencies
- api.ts
- Solution1/src/server/DnsZoneRecordManager/Validation/DnsRules.cs
- DesignTimeDbContextFactory
- records/page.test.tsx
- vitest
- dependencies
- scripts
- DnsRules
- DnsViewModels.cs
- .SeedAsync
- select.tsx
- AppDbContext
- CreateRecordRequest

## God Nodes (most connected - your core abstractions)
1. `ServiceResult` - 44 edges
2. `RecordType` - 42 edges
3. `AppDbContext` - 39 edges
4. `DnsZoneRecordManager.Models` - 38 edges
5. `DnsRecord` - 36 edges
6. `ServiceResult` - 33 edges
7. `DnsZone` - 30 edges
8. `DnsZoneRecordManager.Data` - 24 edges
9. `CreateRecordCommand` - 21 edges
10. `DnsZoneRecordManager.Validation` - 19 edges

## Surprising Connections (you probably didn't know these)
- `RecordsApiTests` --references--> `DnsWebFactory`  [EXTRACTED]
  Solution2/src/server/DnsZoneRecordManager.Tests/IntegrationTests.cs → Solution1/src/server/DnsZoneRecordManager.Tests/IntegrationTests.cs
- `SiteApiTests` --references--> `DnsWebFactory`  [EXTRACTED]
  Solution2/src/server/DnsZoneRecordManager.Tests/IntegrationTests.cs → Solution1/src/server/DnsZoneRecordManager.Tests/IntegrationTests.cs
- `ZonesApiTests` --references--> `DnsWebFactory`  [EXTRACTED]
  Solution2/src/server/DnsZoneRecordManager.Tests/IntegrationTests.cs → Solution1/src/server/DnsZoneRecordManager.Tests/IntegrationTests.cs
- `DesignTimeDbContextFactory` --references--> `AppDbContext`  [EXTRACTED]
  Solution2/src/server/DnsZoneRecordManager/Data/DesignTimeDbContextFactory.cs → Solution1/src/server/DnsZoneRecordManager/Data/AppDbContext.cs
- `RecordDto` --references--> `RecordType`  [EXTRACTED]
  Solution2/src/server/DnsZoneRecordManager/Cqrs/Dtos.cs → Solution1/src/server/DnsZoneRecordManager/Models/DnsRecord.cs

## Import Cycles
- None detected.

## Communities (107 total, 18 thin omitted)

### Community 0 - "Solution1/src/server/DnsZoneRecordManager.Tests/DnsZoneRecordManager.Tests.csproj"
Cohesion: 0.10
Nodes (18): net10.0, FluentValidation (12.1.1), Microsoft.AspNetCore.OpenApi (10.0.12), Microsoft.EntityFrameworkCore.InMemory (10.0.12), Scalar.AspNetCore (2.17.4), Microsoft.NET.Sdk.Web, net10.0, Bogus (35.6.5) (+10 more)

### Community 1 - "opencode.json"
Cohesion: 0.04
Nodes (47): agents, default, list, chmod 777 *, rm -rf *, sudo *, commands, custom (+39 more)

### Community 2 - "package.json"
Cohesion: 0.08
Nodes (21): @base-ui/react, eslint, eslint-config-next, jsdom, next, prettier, react-dom, tailwindcss (+13 more)

### Community 3 - "Solution1/src/server/DnsZoneRecordManager.Tests/UnitTest1.cs"
Cohesion: 0.17
Nodes (10): Fact, IZoneLookup, ToolchainSmokeTests, ZoneInput, ZoneInputValidator, Fact, IZoneLookup, ToolchainSmokeTests (+2 more)

### Community 4 - "HomeController"
Cohesion: 0.14
Nodes (13): Controller, ResponseCache, HttpGet, IActionResult, HomeController, ErrorViewModel, RequestId, ShowRequestId (+5 more)

### Community 5 - "compilerOptions"
Cohesion: 0.11
Nodes (18): compilerOptions, allowJs, esModuleInterop, incremental, isolatedModules, jsx, lib, module (+10 more)

### Community 6 - "http"
Cohesion: 0.13
Nodes (15): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, applicationUrl, commandName (+7 more)

### Community 7 - "http"
Cohesion: 0.13
Nodes (15): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, applicationUrl, commandName (+7 more)

### Community 8 - "Fact"
Cohesion: 0.29
Nodes (9): Fact, JsonSerializerOptions, List, RecordDto, Task, ZoneDto, ApiJson, RecordsApiTests (+1 more)

### Community 9 - "ServiceResult"
Cohesion: 0.15
Nodes (15): DateTime, DnsRecord, ILogger, IValidator, List, ServiceError, Task, IRecordService (+7 more)

### Community 10 - ".SendAsync"
Cohesion: 0.06
Nodes (56): ArgumentNullException, Assembly, FrozenDictionary, IClassFixture, InvalidOperationException, IServiceCollection, IServiceProvider, ActionResult (+48 more)

### Community 11 - ".BuildApp"
Cohesion: 0.09
Nodes (25): IUnitOfWork, Records, Zones, UnitOfWork, Records, Zones, ExcludeFromCodeCoverage, IValidator (+17 more)

### Community 12 - "Task"
Cohesion: 0.18
Nodes (14): HttpClient, HttpResponseMessage, Dictionary, Fact, IWebHostBuilder, Task, DnsWebFactory, FlowTests (+6 more)

### Community 16 - "app/page.tsx"
Cohesion: 0.33
Nodes (6): react, Card(), CardContent(), CardDescription(), CardHeader(), CardTitle()

### Community 24 - "What You Must Do When Invoked"
Cohesion: 0.08
Nodes (24): For /graphify add and --watch, For /graphify query, For the commit hook and native CLAUDE.md integration, For --update and --cluster-only, /graphify, Honesty Rules, Interpreter guard for subcommands, Part A - Structural extraction for code files (+16 more)

### Community 25 - "Solution2/src/server/DnsZoneRecordManager.Tests/DnsZoneRecordManager.Tests.csproj"
Cohesion: 0.08
Nodes (22): Microsoft.EntityFrameworkCore.Design (10.0.12), Serilog.AspNetCore (10.0.0), Serilog.Sinks.Console (6.1.1), Serilog.Sinks.File (7.0.0), net10.0, FluentValidation (12.1.1), Microsoft.AspNetCore.OpenApi (10.0.11), Microsoft.EntityFrameworkCore.Sqlite (10.0.12) (+14 more)

### Community 26 - "DNS Zone & Records Manager — Requirements Analysis (accurate, file-by-file)"
Cohesion: 0.10
Nodes (20): 10. Open questions for hiring manager (optional, non-blocking; defaults above apply if unanswered), 11. Bundle provenance — people, organization, file metadata (forensics, 2026-09-17 UTC), 11a. Filesystem metadata (this machine, IST; uid 501/staff), 11b. Embedded + archive metadata (authoring side), 11c. Emails found in bundle, 11d. Explicitly absent (checked, not present), 1. File inventory (all files, no exceptions), 2. Assignment verbatim (so nothing is lost) (+12 more)

### Community 27 - "Toolchain & Packages — DnsZoneRecordManager"
Cohesion: 0.12
Nodes (16): 1. Toolchain (host), 2. Solution1 — MVC (`Solution1/src/server/DnsZoneRecordManager`), 3. Solution2 — Web API (`Solution2/src/server/DnsZoneRecordManager`), 4. Solution2 — client (`Solution2/src/client`, Next.js App Router + `src/`), 5. Re-verify commands, 6. Deliberately NOT installed yet (next implementation steps), 7. Formatters (project decision), Development dependencies (16) (+8 more)

### Community 28 - "Opencode Configuration for DnsZoneRecordManager"
Cohesion: 0.15
Nodes (12): Agents, Commands, Documentation, Formatters, Knowledge graph, Opencode Configuration for DnsZoneRecordManager, Run Command, Skills (+4 more)

### Community 29 - "graphify reference: extra exports and benchmark"
Cohesion: 0.22
Nodes (8): graphify reference: extra exports and benchmark, Step 6b - Wiki (only if --wiki flag), Step 7 - Neo4j export (only if --neo4j or --neo4j-push flag), Step 7a - FalkorDB export (only if --falkordb or --falkordb-push flag), Step 7b - SVG export (only if --svg flag), Step 7c - GraphML export (only if --graphml flag), Step 7d - MCP server (only if --mcp flag), Step 8 - Token reduction benchmark (only if total_words > 5000)

### Community 30 - ".Create"
Cohesion: 0.25
Nodes (12): ActionName, HttpGet, HttpPost, IActionResult, IEnumerable, List, RecordFormViewModel, SelectListItem (+4 more)

### Community 31 - "graphify reference: query, path, explain"
Cohesion: 0.33
Nodes (5): For /graphify explain, For /graphify path, graphify reference: query, path, explain, Step 0 — Constrained query expansion (REQUIRED before traversal), Step 1 — Traversal

### Community 32 - "DnsRecord"
Cohesion: 0.07
Nodes (27): CancellationToken, Task, DateTime, DnsRecord, CreatedUtc, Data, Id, Name (+19 more)

### Community 33 - "graphify reference: add a URL and watch a folder"
Cohesion: 0.50
Nodes (3): For /graphify add, For --watch, graphify reference: add a URL and watch a folder

### Community 34 - "graphify reference: commit hook and native CLAUDE.md integration"
Cohesion: 0.50
Nodes (3): For git commit hook, For native CLAUDE.md integration, graphify reference: commit hook and native CLAUDE.md integration

### Community 35 - "graphify reference: incremental update and cluster-only"
Cohesion: 0.50
Nodes (3): For --cluster-only, For --update (incremental re-extraction), graphify reference: incremental update and cluster-only

### Community 36 - "client/README.md"
Cohesion: 0.50
Nodes (3): Deploy on Vercel, Getting Started, Learn More

### Community 65 - "RecordDeleteViewModel"
Cohesion: 0.22
Nodes (9): RecordDeleteViewModel, Data, Fqdn, Id, Name, Ttl, Type, ZoneId (+1 more)

### Community 66 - "IGenericRepository"
Cohesion: 0.20
Nodes (7): DbSet, Expression, Func, List, Task, GenericRepository, IGenericRepository

### Community 67 - "IRecordDataValidator"
Cohesion: 0.09
Nodes (14): Dictionary, HostnameValidator, IPv4Validator, IPv6Validator, IRecordDataValidator, RecordDataValidators, TextValidator, Dictionary (+6 more)

### Community 68 - "DnsZone"
Cohesion: 0.06
Nodes (44): ActionResult, HttpDelete, HttpGet, HttpPost, HttpPut, IActionResult, List, ProducesResponseType (+36 more)

### Community 69 - "Solution1 — DNS Manager (ASP.NET Core MVC)"
Cohesion: 0.05
Nodes (39): DnsZoneRecordManager, Docs, Repo layout, Run, Tooling, Build / run / test, Cut for the 8h box, Design patterns (GoF, as needed — nothing speculative) (+31 more)

### Community 76 - "ServiceResult"
Cohesion: 0.05
Nodes (84): AbstractValidator, Connection, Context, IQueryable, IRequest, IRequestHandler, DbSet, AppDbContext (+76 more)

### Community 77 - "DnsZoneRecordManager.Models"
Cohesion: 0.28
Nodes (5): DnsZoneRecordManager.Models, DnsZoneRecordManager.Tests, DnsZoneRecordManager.Controllers.Api, DnsZoneRecordManager.Services, DbSeeder

### Community 78 - "components.json"
Cohesion: 0.09
Nodes (21): aliases, components, hooks, lib, ui, utils, iconLibrary, menuAccent (+13 more)

### Community 79 - ".Update"
Cohesion: 0.23
Nodes (13): ControllerBase, ActionResult, HttpDelete, HttpGet, HttpPost, HttpPut, IActionResult, List (+5 more)

### Community 80 - "DnsZoneRecordManager.Data"
Cohesion: 0.28
Nodes (7): DnsZoneRecordManager.Validation, DnsZoneRecordManager.Results, DnsZoneRecordManager.Cqrs.Handlers, DnsZoneRecordManager.Data, DnsZoneRecordManager.Cqrs.Records, DnsZoneRecordManager.Cqrs.Zones, DnsZoneRecordManager

### Community 81 - "apiErrorMessages"
Cohesion: 0.14
Nodes (11): fromSelect(), RecordDialog(), handleSubmit(), RecordsPage(), handleDelete(), ZoneDialog(), handleSubmit(), ZonesPage() (+3 more)

### Community 82 - "UpdateRecordRequest"
Cohesion: 0.15
Nodes (11): CreateZoneRequest, Name, RenameZoneRequest, Name, UpdateRecordRequest, Data, Name, Ttl (+3 more)

### Community 83 - "InitialCreate"
Cohesion: 0.12
Nodes (11): DnsZoneRecordManager.Data.Migrations, Migration, MigrationBuilder, ModelSnapshot, DateTime, DateTime, ModelBuilder, InitialCreate (+3 more)

### Community 84 - "RecordType"
Cohesion: 0.12
Nodes (17): DateTime, DnsRecord, CreatedUtc, Data, Id, Name, Ttl, Type (+9 more)

### Community 85 - "records/page.tsx"
Cohesion: 0.17
Nodes (15): EMPTY_FORM, Filters, RecordForm, RecordPayload, TYPE_STYLES, TYPES, Label(), Table() (+7 more)

### Community 86 - "layout.tsx"
Cohesion: 0.22
Nodes (9): lucide-react, next-themes, inter, metadata, links, SiteHeader(), { mockPath }, ThemeProvider() (+1 more)

### Community 87 - "RecordFormViewModel"
Cohesion: 0.09
Nodes (24): List, SelectListItem, RecordFormViewModel, Data, Id, Name, Ttl, Type (+16 more)

### Community 88 - "zones/page.tsx"
Cohesion: 0.20
Nodes (9): Button(), buttonVariants, Dialog(), DialogContent(), DialogDescription(), DialogFooter(), DialogHeader(), DialogTitle() (+1 more)

### Community 89 - "ErrorKind"
Cohesion: 0.24
Nodes (7): IEnumerable, List, ErrorKind, Conflict, RuleViolation, Validation, ServiceError

### Community 90 - "Solution2/src/server/DnsZoneRecordManager.Tests/IntegrationTests.cs"
Cohesion: 0.20
Nodes (5): DnsZoneRecordManager.Controllers, DnsZoneRecordManager.ViewModels, DnsZoneRecordManager.Cqrs, JsonSerializerOptions, ApiJson

### Community 92 - "cn"
Cohesion: 0.18
Nodes (7): class-variance-authority, cn, Alert(), AlertDescription(), alertVariants, Badge(), badgeVariants

### Community 93 - "devDependencies"
Cohesion: 0.12
Nodes (17): devDependencies, eslint, eslint-config-next, jsdom, prettier, tailwindcss, @tailwindcss/postcss, @testing-library/jest-dom (+9 more)

### Community 94 - "api.ts"
Cohesion: 0.27
Nodes (5): ApiError, baseUrl(), recordsApi, request(), zonesApi

### Community 96 - "DesignTimeDbContextFactory"
Cohesion: 0.40
Nodes (3): IDesignTimeDbContextFactory, AppDbContext, DesignTimeDbContextFactory

### Community 97 - "records/page.test.tsx"
Cohesion: 0.16
Nodes (7): sonner, @testing-library/user-event, RECORDS, ZONES, ZONES, RecordDto, ZoneDto

### Community 98 - "vitest"
Cohesion: 0.16
Nodes (7): @testing-library/react, vitest, RootLayout(), DashboardPage(), RECORDS, ZONES, ResizeObserverStub

### Community 99 - "dependencies"
Cohesion: 0.18
Nodes (11): dependencies, @base-ui/react, class-variance-authority, cn, lucide-react, next, next-themes, react (+3 more)

### Community 100 - "scripts"
Cohesion: 0.20
Nodes (10): scripts, build, dev, format, format:check, lint, start, test (+2 more)

### Community 102 - "DnsViewModels.cs"
Cohesion: 0.33
Nodes (5): ZoneDeleteViewModel, Id, Name, NsCount, RecordCount

### Community 103 - ".SeedAsync"
Cohesion: 0.18
Nodes (7): IMigrator, Task, DbSeeder, Fact, Task, MigrationTests, SqliteConnection

### Community 104 - "select.tsx"
Cohesion: 0.20
Nodes (3): SelectContent(), SelectItem(), SelectTrigger()

### Community 105 - "AppDbContext"
Cohesion: 0.25
Nodes (6): DbContext, DbSet, ModelBuilder, AppDbContext, Records, Zones

### Community 106 - "CreateRecordRequest"
Cohesion: 0.33
Nodes (6): CreateRecordRequest, Data, Name, Ttl, Type, ZoneId

## Knowledge Gaps
- **424 isolated node(s):** `$schema`, `version`, `model`, `small_model`, `reasoningEffort` (+419 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 626 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **18 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `RecordType` connect `RecordType` to `DnsRecord`, `RecordDeleteViewModel`, `IRecordDataValidator`, `ServiceResult`, `CreateRecordRequest`, `.BuildApp`, `.SendAsync`, `ServiceResult`, `.Update`, `UpdateRecordRequest`, `RecordFormViewModel`, `.Create`?**
  _High betweenness centrality (0.064) - this node is a cross-community bridge._
- **Why does `AppDbContext` connect `ServiceResult` to `DnsRecord`, `DesignTimeDbContextFactory`, `DnsZone`, `HomeController`, `.SeedAsync`, `AppDbContext`, `.BuildApp`, `Task`, `DnsZoneRecordManager.Data`, `RecordType`?**
  _High betweenness centrality (0.048) - this node is a cross-community bridge._
- **Why does `DnsZoneRecordManager.Models` connect `DnsZoneRecordManager.Models` to `DnsRecord`, `IRecordDataValidator`, `HomeController`, `DnsZone`, `DnsViewModels.cs`, `.SeedAsync`, `ServiceResult`, `AppDbContext`, `.BuildApp`, `ServiceResult`, `DnsZoneRecordManager.Data`, `RecordType`, `Solution2/src/server/DnsZoneRecordManager.Tests/IntegrationTests.cs`, `Solution1/src/server/DnsZoneRecordManager/Validation/DnsRules.cs`?**
  _High betweenness centrality (0.041) - this node is a cross-community bridge._
- **Are the 2 inferred relationships involving `AppDbContext` (e.g. with `.should_seed_sample_zone_once()` and `.New()`) actually correct?**
  _`AppDbContext` has 2 INFERRED edges - model-reasoned connections that need verification._
- **What connects `$schema`, `version`, `model` to the rest of the system?**
  _424 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Solution1/src/server/DnsZoneRecordManager.Tests/DnsZoneRecordManager.Tests.csproj` be split into smaller, more focused modules?**
  _Cohesion score 0.1 - nodes in this community are weakly interconnected._
- **Should `opencode.json` be split into smaller, more focused modules?**
  _Cohesion score 0.041666666666666664 - nodes in this community are weakly interconnected._