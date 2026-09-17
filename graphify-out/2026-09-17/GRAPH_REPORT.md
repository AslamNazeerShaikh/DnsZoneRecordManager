# Graph Report - DnsZoneRecordManager  (2026-09-17)

## Corpus Check
- cluster-only mode — file stats not available

## Summary
- 241 nodes · 248 edges · 24 communities (13 shown, 3 thin omitted)
- Extraction: 100% EXTRACTED · 0% INFERRED · 0% AMBIGUOUS · INFERRED: 1 edges (avg confidence: 0.85)
- Token cost: 0 input · 0 output

## Community Hubs (Navigation)
- DnsZoneRecordManager.Tests
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

## God Nodes (most connected - your core abstractions)
1. `compilerOptions` - 16 edges
2. `DnsZoneRecordManager.Tests` - 12 edges
3. `DnsZoneRecordManager.Tests` - 12 edges
4. `permission` - 8 edges
5. `WeatherForecast` - 7 edges
6. `DnsZoneRecordManager` - 6 edges
7. `http` - 6 edges
8. `https` - 6 edges
9. `http` - 6 edges
10. `https` - 6 edges

## Surprising Connections (you probably didn't know these)
- `DnsZoneRecordManager` --references--> `Microsoft.NET.Sdk.Web`  [EXTRACTED]
  Solution2/src/server/DnsZoneRecordManager/DnsZoneRecordManager.csproj → Solution1/src/server/DnsZoneRecordManager/DnsZoneRecordManager.csproj
- `DnsZoneRecordManager.Tests` --references--> `Microsoft.NET.Sdk`  [EXTRACTED]
  Solution2/src/server/DnsZoneRecordManager.Tests/DnsZoneRecordManager.Tests.csproj → Solution1/src/server/DnsZoneRecordManager.Tests/DnsZoneRecordManager.Tests.csproj
- `ZoneInputValidator` --references--> `ZoneInput`  [EXTRACTED]
  Solution2/src/server/DnsZoneRecordManager.Tests/UnitTest1.cs → Solution1/src/server/DnsZoneRecordManager.Tests/UnitTest1.cs
- `DnsZoneRecordManager` --references--> `net10.0`  [EXTRACTED]
  Solution2/src/server/DnsZoneRecordManager/DnsZoneRecordManager.csproj → Solution1/src/server/DnsZoneRecordManager/DnsZoneRecordManager.csproj
- `DnsZoneRecordManager.Tests` --references--> `net10.0`  [EXTRACTED]
  Solution2/src/server/DnsZoneRecordManager.Tests/DnsZoneRecordManager.Tests.csproj → Solution1/src/server/DnsZoneRecordManager/DnsZoneRecordManager.csproj

## Import Cycles
- None detected.

## Communities (24 total, 3 thin omitted)

### Community 0 - "DnsZoneRecordManager.Tests"
Cohesion: 0.09
Nodes (26): Microsoft.AspNetCore.OpenApi (10.0.11), DnsZoneRecordManager, net10.0, FluentValidation (12.1.1), Microsoft.NET.Sdk.Web, DnsZoneRecordManager.Tests, Bogus (35.6.5), coverlet.collector (6.0.4) (+18 more)

### Community 1 - "opencode.json"
Cohesion: 0.08
Nodes (24): agents, default, list, commands, custom, instructions, lsp, model (+16 more)

### Community 2 - "package.json"
Cohesion: 0.09
Nodes (22): eslint, eslint-config-next, react, react-dom, tailwindcss, @tailwindcss/postcss, @types/node, @types/react (+14 more)

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
Cohesion: 0.22
Nodes (9): devDependencies, eslint, eslint-config-next, tailwindcss, @tailwindcss/postcss, @types/node, @types/react, @types/react-dom (+1 more)

### Community 12 - "layout.tsx"
Cohesion: 0.25
Nodes (5): next, nextConfig, geistMono, geistSans, metadata

## Knowledge Gaps
- **126 isolated node(s):** `Microsoft.AspNetCore.OpenApi (10.0.11)`, `FluentValidation (12.1.1)`, `Bogus (35.6.5)`, `coverlet.collector (6.0.4)`, `FluentAssertions (8.11.0)` (+121 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 150 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **3 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `mcp` connect `github` to `opencode.json`?**
  _High betweenness centrality (0.015) - this node is a cross-community bridge._
- **Why does `permission` connect `permission` to `opencode.json`?**
  _High betweenness centrality (0.014) - this node is a cross-community bridge._
- **What connects `Microsoft.AspNetCore.OpenApi (10.0.11)`, `FluentValidation (12.1.1)`, `Bogus (35.6.5)` to the rest of the system?**
  _126 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `DnsZoneRecordManager.Tests` be split into smaller, more focused modules?**
  _Cohesion score 0.0873015873015873 - nodes in this community are weakly interconnected._
- **Should `opencode.json` be split into smaller, more focused modules?**
  _Cohesion score 0.08 - nodes in this community are weakly interconnected._
- **Should `package.json` be split into smaller, more focused modules?**
  _Cohesion score 0.08695652173913043 - nodes in this community are weakly interconnected._
- **Should `WeatherForecast` be split into smaller, more focused modules?**
  _Cohesion score 0.09523809523809523 - nodes in this community are weakly interconnected._