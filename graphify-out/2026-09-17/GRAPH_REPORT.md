# Graph Report - DnsZoneRecordManager  (2026-09-17)

## Corpus Check
- 104 files · ~37,914 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 741 nodes · 1181 edges · 77 communities (32 shown, 15 thin omitted)
- Extraction: 94% EXTRACTED · 6% INFERRED · 0% AMBIGUOUS · INFERRED: 67 edges (avg confidence: 0.84)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `5467d98b`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- Solution1/src/server/DnsZoneRecordManager.Tests/DnsZoneRecordManager.Tests.csproj
- opencode.json
- package.json
- Solution1/src/server/DnsZoneRecordManager.Tests/UnitTest1.cs
- DnsZoneRecordManager.Models
- compilerOptions
- http
- http
- HomeController
- RecordType
- ServiceResult
- .New
- Task
- graphify.js
- eslint.config.mjs
- postcss.config.mjs
- site.js
- What You Must Do When Invoked
- Solution2/src/server/DnsZoneRecordManager.Tests/DnsZoneRecordManager.Tests.csproj
- DNS Zone & Records Manager — Requirements Analysis (accurate, file-by-file)
- Toolchain & Packages — DnsZoneRecordManager
- Opencode Configuration for DnsZoneRecordManager
- graphify reference: extra exports and benchmark
- RecordFormViewModel
- graphify reference: query, path, explain
- DnsZoneRecordManager
- graphify reference: add a URL and watch a folder
- graphify reference: commit hook and native CLAUDE.md integration
- graphify reference: incremental update and cluster-only
- client/README.md
- graphify reference: GitHub clone and cross-repo merge
- graphify reference: transcribe video and audio
- AGENTS.md
- extraction-spec.md
- client/AGENTS.md
- DnsRecord
- IGenericRepository
- RecordDataValidators.cs
- RecordDeleteViewModel
- Solution1 — DNS Manager (ASP.NET Core MVC)
- Records/Create.cshtml
- RecordDeleteViewModel
- RecordIndexViewModel
- Zones/Create.cshtml
- Zones/Delete.cshtml
- Zones/Index.cshtml
- WeatherForecast

## God Nodes (most connected - your core abstractions)
1. `DnsRecord` - 32 edges
2. `ServiceResult` - 29 edges
3. `RecordType` - 26 edges
4. `DnsZone` - 26 edges
5. `DnsZoneRecordManager.Models` - 21 edges
6. `RecordService` - 18 edges
7. `AppDbContext` - 16 edges
8. `compilerOptions` - 16 edges
9. `IUnitOfWork` - 15 edges
10. `RecordFormViewModel` - 15 edges

## Surprising Connections (you probably didn't know these)
- `ZoneInputValidator` --references--> `ZoneInput`  [EXTRACTED]
  Solution2/src/server/DnsZoneRecordManager.Tests/UnitTest1.cs → Solution1/src/server/DnsZoneRecordManager.Tests/UnitTest1.cs
- `DnsWebFactory` --references--> `Program`  [EXTRACTED]
  Solution1/src/server/DnsZoneRecordManager.Tests/IntegrationTests.cs → Solution1/src/server/DnsZoneRecordManager/Program.cs
- `RecordsController` --references--> `IZoneService`  [EXTRACTED]
  Solution1/src/server/DnsZoneRecordManager/Controllers/RecordsController.cs → Solution1/src/server/DnsZoneRecordManager/Services/ZoneService.cs
- `IUnitOfWork` --references--> `IGenericRepository`  [EXTRACTED]
  Solution1/src/server/DnsZoneRecordManager/Data/UnitOfWork.cs → Solution1/src/server/DnsZoneRecordManager/Data/GenericRepository.cs
- `UnitOfWork` --references--> `IGenericRepository`  [EXTRACTED]
  Solution1/src/server/DnsZoneRecordManager/Data/UnitOfWork.cs → Solution1/src/server/DnsZoneRecordManager/Data/GenericRepository.cs

## Import Cycles
- None detected.

## Communities (77 total, 15 thin omitted)

### Community 0 - "Solution1/src/server/DnsZoneRecordManager.Tests/DnsZoneRecordManager.Tests.csproj"
Cohesion: 0.11
Nodes (16): Microsoft.AspNetCore.Mvc.Testing (10.0.12), net10.0, FluentValidation (12.1.1), Microsoft.EntityFrameworkCore.InMemory (10.0.12), Microsoft.NET.Sdk.Web, net10.0, Bogus (35.6.5), coverlet.collector (6.0.4) (+8 more)

### Community 1 - "opencode.json"
Cohesion: 0.04
Nodes (47): agents, default, list, chmod 777 *, rm -rf *, sudo *, commands, custom (+39 more)

### Community 2 - "package.json"
Cohesion: 0.05
Nodes (40): eslint, eslint-config-next, next, prettier, react, react-dom, tailwindcss, @tailwindcss/postcss (+32 more)

### Community 3 - "Solution1/src/server/DnsZoneRecordManager.Tests/UnitTest1.cs"
Cohesion: 0.16
Nodes (11): AbstractValidator, Fact, IZoneLookup, ToolchainSmokeTests, ZoneInput, ZoneInputValidator, Fact, IZoneLookup (+3 more)

### Community 4 - "DnsZoneRecordManager.Models"
Cohesion: 0.14
Nodes (12): DnsZoneRecordManager.Validation, DnsZoneRecordManager.Controllers, DnsZoneRecordManager.Models, DnsZoneRecordManager.Data, DnsZoneRecordManager.ViewModels, DnsZoneRecordManager.Tests, DnsZoneRecordManager.Services, ZoneDeleteViewModel (+4 more)

### Community 5 - "compilerOptions"
Cohesion: 0.11
Nodes (18): compilerOptions, allowJs, esModuleInterop, incremental, isolatedModules, jsx, lib, module (+10 more)

### Community 6 - "http"
Cohesion: 0.13
Nodes (15): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, applicationUrl, commandName (+7 more)

### Community 7 - "http"
Cohesion: 0.13
Nodes (15): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, applicationUrl, commandName (+7 more)

### Community 8 - "HomeController"
Cohesion: 0.20
Nodes (10): Controller, ResponseCache, IActionResult, HomeController, ErrorViewModel, RequestId, ShowRequestId, Fact (+2 more)

### Community 9 - "RecordType"
Cohesion: 0.08
Nodes (27): ActionName, HttpPost, IActionResult, IEnumerable, List, RecordFormViewModel, SelectListItem, Task (+19 more)

### Community 10 - "ServiceResult"
Cohesion: 0.10
Nodes (25): ActionName, HttpPost, IActionResult, IEnumerable, Task, ValidateAntiForgeryToken, ZoneFormViewModel, ZonesController (+17 more)

### Community 11 - ".New"
Cohesion: 0.19
Nodes (12): IUnitOfWork, Records, Zones, Fact, Task, RecordServiceTests, Task, TestUow (+4 more)

### Community 12 - "Task"
Cohesion: 0.23
Nodes (12): HttpClient, HttpResponseMessage, IClassFixture, Dictionary, Fact, Task, DnsWebFactory, FlowTests (+4 more)

### Community 24 - "What You Must Do When Invoked"
Cohesion: 0.08
Nodes (24): For /graphify add and --watch, For /graphify query, For the commit hook and native CLAUDE.md integration, For --update and --cluster-only, /graphify, Honesty Rules, Interpreter guard for subcommands, Part A - Structural extraction for code files (+16 more)

### Community 25 - "Solution2/src/server/DnsZoneRecordManager.Tests/DnsZoneRecordManager.Tests.csproj"
Cohesion: 0.12
Nodes (14): Microsoft.AspNetCore.OpenApi (10.0.11), net10.0, FluentValidation (12.1.1), Microsoft.NET.Sdk.Web, net10.0, Bogus (35.6.5), coverlet.collector (6.0.4), FluentAssertions (8.11.0) (+6 more)

### Community 26 - "DNS Zone & Records Manager — Requirements Analysis (accurate, file-by-file)"
Cohesion: 0.10
Nodes (20): 10. Open questions for hiring manager (optional, non-blocking; defaults above apply if unanswered), 11. Bundle provenance — people, organization, file metadata (forensics, 2026-09-17 UTC), 11a. Filesystem metadata (this machine, IST; uid 501/staff), 11b. Embedded + archive metadata (authoring side), 11c. Emails found in bundle, 11d. Explicitly absent (checked, not present), 1. File inventory (all files, no exceptions), 2. Assignment verbatim (so nothing is lost) (+12 more)

### Community 27 - "Toolchain & Packages — DnsZoneRecordManager"
Cohesion: 0.12
Nodes (16): 1. Toolchain (host), 2. Solution1 — MVC (`Solution1/src/server/DnsZoneRecordManager`), 3. Solution2 — Web API (`Solution2/src/server/DnsZoneRecordManager`), 4. Solution2 — client (`Solution2/src/client`, Next.js App Router + `src/`), 5. Re-verify commands, 6. Deliberately NOT installed yet (next implementation steps), 7. Formatters (project decision), Development dependencies (9) (+8 more)

### Community 28 - "Opencode Configuration for DnsZoneRecordManager"
Cohesion: 0.15
Nodes (12): Agents, Commands, Documentation, Formatters, Knowledge graph, Opencode Configuration for DnsZoneRecordManager, Run Command, Skills (+4 more)

### Community 29 - "graphify reference: extra exports and benchmark"
Cohesion: 0.22
Nodes (8): graphify reference: extra exports and benchmark, Step 6b - Wiki (only if --wiki flag), Step 7 - Neo4j export (only if --neo4j or --neo4j-push flag), Step 7a - FalkorDB export (only if --falkordb or --falkordb-push flag), Step 7b - SVG export (only if --svg flag), Step 7c - GraphML export (only if --graphml flag), Step 7d - MCP server (only if --mcp flag), Step 8 - Token reduction benchmark (only if total_words > 5000)

### Community 30 - "RecordFormViewModel"
Cohesion: 0.09
Nodes (24): List, SelectListItem, RecordFormViewModel, Data, Id, Name, Ttl, Type (+16 more)

### Community 31 - "graphify reference: query, path, explain"
Cohesion: 0.33
Nodes (5): For /graphify explain, For /graphify path, graphify reference: query, path, explain, Step 0 — Constrained query expansion (REQUIRED before traversal), Step 1 — Traversal

### Community 32 - "DnsZoneRecordManager"
Cohesion: 0.33
Nodes (5): DnsZoneRecordManager, Docs, Repo layout, Run (empty scaffolds), Tooling

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

### Community 65 - "DnsRecord"
Cohesion: 0.06
Nodes (42): DbContext, ExcludeFromCodeCoverage, ICollection, IWebHostBuilder, ModelBuilder, DbSet, AppDbContext, Records (+34 more)

### Community 66 - "IGenericRepository"
Cohesion: 0.20
Nodes (7): Expression, Func, DbSet, List, Task, GenericRepository, IGenericRepository

### Community 67 - "RecordDataValidators.cs"
Cohesion: 0.15
Nodes (9): Regex, DnsRules, Dictionary, HostnameValidator, IPv4Validator, IPv6Validator, IRecordDataValidator, RecordDataValidators (+1 more)

### Community 68 - "RecordDeleteViewModel"
Cohesion: 0.22
Nodes (9): RecordDeleteViewModel, Data, Fqdn, Id, Name, Ttl, Type, ZoneId (+1 more)

### Community 69 - "Solution1 — DNS Manager (ASP.NET Core MVC)"
Cohesion: 0.22
Nodes (8): Build / run / test, Cut for the 8h box, Design patterns (GoF, as needed — nothing speculative), Features (every REQUIREMENTS §8 item), Notes / assumptions (see REQUIREMENTS §7 resolutions), Security audit (against the 25-point CRUD checklist), Solution1 — DNS Manager (ASP.NET Core MVC), Stack

### Community 76 - "WeatherForecast"
Cohesion: 0.11
Nodes (13): ControllerBase, DnsZoneRecordManager, DateOnly, HttpGet, IEnumerable, WeatherForecastController, Program, WeatherForecast (+5 more)

## Knowledge Gaps
- **304 isolated node(s):** `$schema`, `version`, `model`, `small_model`, `reasoningEffort` (+299 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 404 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **15 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `DnsRecord` connect `DnsRecord` to `RecordType`, `ServiceResult`, `.New`?**
  _High betweenness centrality (0.047) - this node is a cross-community bridge._
- **Why does `DnsZoneRecordManager.Models` connect `DnsZoneRecordManager.Models` to `DnsRecord`, `RecordType`, `RecordDataValidators.cs`, `HomeController`?**
  _High betweenness centrality (0.043) - this node is a cross-community bridge._
- **Why does `RecordType` connect `RecordType` to `DnsRecord`, `RecordDataValidators.cs`, `RecordDeleteViewModel`, `ServiceResult`, `RecordFormViewModel`?**
  _High betweenness centrality (0.040) - this node is a cross-community bridge._
- **Are the 4 inferred relationships involving `DnsZone` (e.g. with `.SeedAsync()` and `.SeedZoneAsync()`) actually correct?**
  _`DnsZone` has 4 INFERRED edges - model-reasoned connections that need verification._
- **What connects `$schema`, `version`, `model` to the rest of the system?**
  _304 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Solution1/src/server/DnsZoneRecordManager.Tests/DnsZoneRecordManager.Tests.csproj` be split into smaller, more focused modules?**
  _Cohesion score 0.1111111111111111 - nodes in this community are weakly interconnected._
- **Should `opencode.json` be split into smaller, more focused modules?**
  _Cohesion score 0.041666666666666664 - nodes in this community are weakly interconnected._