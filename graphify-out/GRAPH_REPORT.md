# Graph Report - DnsZoneRecordManager  (2026-09-17)

## Corpus Check
- 78 files · ~27,006 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 399 nodes · 361 edges · 65 communities (26 shown, 8 thin omitted)
- Extraction: 100% EXTRACTED · 0% INFERRED · 0% AMBIGUOUS · INFERRED: 1 edges (avg confidence: 0.85)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `812f0956`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- Solution1/src/server/DnsZoneRecordManager.Tests/DnsZoneRecordManager.Tests.csproj
- opencode.json
- package.json
- Solution1/src/server/DnsZoneRecordManager.Tests/UnitTest1.cs
- WeatherForecast
- compilerOptions
- http
- http
- HomeController
- github
- permission
- devDependencies
- layout.tsx
- graphify.js
- eslint.config.mjs
- postcss.config.mjs
- What You Must Do When Invoked
- Solution2/src/server/DnsZoneRecordManager.Tests/DnsZoneRecordManager.Tests.csproj
- DNS Zone & Records Manager — Requirements Analysis (accurate, file-by-file)
- Toolchain & Packages — DnsZoneRecordManager
- Opencode Configuration for DnsZoneRecordManager
- graphify reference: extra exports and benchmark
- Session Memory — DnsZoneRecordManager — 20260917T160239Z
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

## God Nodes (most connected - your core abstractions)
1. `compilerOptions` - 16 edges
2. `What You Must Do When Invoked` - 12 edges
3. `DNS Zone & Records Manager — Requirements Analysis (accurate, file-by-file)` - 11 edges
4. `/graphify` - 10 edges
5. `Opencode Configuration for DnsZoneRecordManager` - 9 edges
6. `permission` - 8 edges
7. `graphify reference: extra exports and benchmark` - 8 edges
8. `Toolchain & Packages — DnsZoneRecordManager` - 8 edges
9. `scripts` - 7 edges
10. `WeatherForecast` - 7 edges

## Surprising Connections (you probably didn't know these)
- `ZoneInputValidator` --references--> `ZoneInput`  [EXTRACTED]
  Solution2/src/server/DnsZoneRecordManager.Tests/UnitTest1.cs → Solution1/src/server/DnsZoneRecordManager.Tests/UnitTest1.cs

## Import Cycles
- None detected.

## Communities (65 total, 8 thin omitted)

### Community 0 - "Solution1/src/server/DnsZoneRecordManager.Tests/DnsZoneRecordManager.Tests.csproj"
Cohesion: 0.13
Nodes (13): net10.0, FluentValidation (12.1.1), Microsoft.NET.Sdk.Web, net10.0, Bogus (35.6.5), coverlet.collector (6.0.4), FluentAssertions (8.11.0), FluentValidation (12.1.1) (+5 more)

### Community 1 - "opencode.json"
Cohesion: 0.08
Nodes (24): agents, default, list, commands, custom, instructions, lsp, model (+16 more)

### Community 2 - "package.json"
Cohesion: 0.08
Nodes (25): eslint, eslint-config-next, prettier, react, react-dom, tailwindcss, @tailwindcss/postcss, @types/node (+17 more)

### Community 3 - "Solution1/src/server/DnsZoneRecordManager.Tests/UnitTest1.cs"
Cohesion: 0.16
Nodes (12): AbstractValidator, DnsZoneRecordManager.Tests, Fact, IZoneLookup, ToolchainSmokeTests, ZoneInput, ZoneInputValidator, Fact (+4 more)

### Community 4 - "WeatherForecast"
Cohesion: 0.10
Nodes (14): ControllerBase, DnsZoneRecordManager, DateOnly, HttpGet, IEnumerable, Program, WeatherForecastController, Program (+6 more)

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
Cohesion: 0.16
Nodes (9): Controller, DnsZoneRecordManager.Controllers, DnsZoneRecordManager.Models, IActionResult, ResponseCache, HomeController, ErrorViewModel, RequestId (+1 more)

### Community 9 - "github"
Cohesion: 0.17
Nodes (12): enabled, headers, oauth, type, url, Authorization, mcp, github (+4 more)

### Community 10 - "permission"
Cohesion: 0.18
Nodes (11): chmod 777 *, rm -rf *, sudo *, permission, bash, edit, glob, grep (+3 more)

### Community 11 - "devDependencies"
Cohesion: 0.20
Nodes (10): devDependencies, eslint, eslint-config-next, prettier, tailwindcss, @tailwindcss/postcss, @types/node, @types/react (+2 more)

### Community 12 - "layout.tsx"
Cohesion: 0.25
Nodes (5): next, nextConfig, geistMono, geistSans, metadata

### Community 24 - "What You Must Do When Invoked"
Cohesion: 0.08
Nodes (24): For /graphify add and --watch, For /graphify query, For the commit hook and native CLAUDE.md integration, For --update and --cluster-only, /graphify, Honesty Rules, Interpreter guard for subcommands, Part A - Structural extraction for code files (+16 more)

### Community 25 - "Solution2/src/server/DnsZoneRecordManager.Tests/DnsZoneRecordManager.Tests.csproj"
Cohesion: 0.12
Nodes (14): Microsoft.AspNetCore.OpenApi (10.0.11), net10.0, FluentValidation (12.1.1), Microsoft.NET.Sdk.Web, net10.0, Bogus (35.6.5), coverlet.collector (6.0.4), FluentAssertions (8.11.0) (+6 more)

### Community 26 - "DNS Zone & Records Manager — Requirements Analysis (accurate, file-by-file)"
Cohesion: 0.12
Nodes (15): 10. Open questions for hiring manager (optional, non-blocking; defaults above apply if unanswered), 1. File inventory (all files, no exceptions), 2. Assignment verbatim (so nothing is lost), 3. Hyperlinks in the PDF (all 8, in document order, live-checked 2026-09-17), 4. Sample data (all three samples agree — this is the seed dataset), 4a. `nahuexolab.com.dns` (full content), 4b. `Dns_Records_05_24_2024 20_06_01.xlsx` (decoded), 4c. `DM_Zone_snapshot.png` (what the image actually shows) (+7 more)

### Community 27 - "Toolchain & Packages — DnsZoneRecordManager"
Cohesion: 0.13
Nodes (14): 1. Toolchain (host), 2. Solution1 — MVC (`Solution1/src/server/DnsZoneRecordManager`), 3. Solution2 — Web API (`Solution2/src/server/DnsZoneRecordManager`), 4. Solution2 — client (`Solution2/src/client`, Next.js App Router + `src/`), 5. Re-verify commands, 6. Deliberately NOT installed yet (next implementation steps), 7. Formatters (project decision), Development dependencies (9) (+6 more)

### Community 28 - "Opencode Configuration for DnsZoneRecordManager"
Cohesion: 0.15
Nodes (12): Agents, Commands, Documentation, Formatters, Knowledge graph, Opencode Configuration for DnsZoneRecordManager, Run Command, Skills (+4 more)

### Community 29 - "graphify reference: extra exports and benchmark"
Cohesion: 0.22
Nodes (8): graphify reference: extra exports and benchmark, Step 6b - Wiki (only if --wiki flag), Step 7 - Neo4j export (only if --neo4j or --neo4j-push flag), Step 7a - FalkorDB export (only if --falkordb or --falkordb-push flag), Step 7b - SVG export (only if --svg flag), Step 7c - GraphML export (only if --graphml flag), Step 7d - MCP server (only if --mcp flag), Step 8 - Token reduction benchmark (only if total_words > 5000)

### Community 30 - "Session Memory — DnsZoneRecordManager — 20260917T160239Z"
Cohesion: 0.25
Nodes (7): Domain rules (§6 model, §7 validation — server authoritative, FluentValidation), Goal (from §2 + §5 + §8), Open tensions for next session (do not guess — confirm or follow doc default), Repo state at save time, Session Memory — DnsZoneRecordManager — 20260917T160239Z, Toolchain pins (TOOLCHAIN doc — re-verify via its §5), What was read (every line, every file — independently verified this session)

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

## Knowledge Gaps
- **227 isolated node(s):** `$schema`, `version`, `model`, `small_model`, `reasoningEffort` (+222 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 290 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **8 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `mcp` connect `github` to `opencode.json`?**
  _High betweenness centrality (0.005) - this node is a cross-community bridge._
- **Why does `permission` connect `permission` to `opencode.json`?**
  _High betweenness centrality (0.005) - this node is a cross-community bridge._
- **Why does `devDependencies` connect `devDependencies` to `package.json`?**
  _High betweenness centrality (0.004) - this node is a cross-community bridge._
- **What connects `$schema`, `version`, `model` to the rest of the system?**
  _227 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Solution1/src/server/DnsZoneRecordManager.Tests/DnsZoneRecordManager.Tests.csproj` be split into smaller, more focused modules?**
  _Cohesion score 0.13333333333333333 - nodes in this community are weakly interconnected._
- **Should `opencode.json` be split into smaller, more focused modules?**
  _Cohesion score 0.08 - nodes in this community are weakly interconnected._
- **Should `package.json` be split into smaller, more focused modules?**
  _Cohesion score 0.07692307692307693 - nodes in this community are weakly interconnected._