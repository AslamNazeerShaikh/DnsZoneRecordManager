# Opencode Configuration for DnsZoneRecordManager

DNS Zones + DNS Records manager take-home (Query, Create, Modify, Delete) for a non-technical persona.
Sources of truth: `docs/REQUIREMENTS_ANALYSIS.md` (domain rules, scope) and `docs/TOOLCHAIN_AND_PACKAGES.md` (pinned versions).
Model: `opencode/muse-spark-1.3-contributor-free` via OpenCode Zen (reasoning effort high; ~1M context is model-side).

## Structure

```
.opencode/
├── opencode.json              # Main configuration (model, agents, skills, commands, MCP)
├── agents/                    # Agent definitions
│   ├── dns-zone-assistant.md  # Main development assistant (default)
│   ├── code-reviewer.md       # ASP.NET Core + Next.js review checklist
│   ├── test-writer.md         # xunit + Moq + Bogus + FluentAssertions specialist
│   └── documentation-writer.md# Keeps docs/ + per-solution readmes in sync
├── skills/                    # Skill definitions
│   ├── api-design.md          # ASP.NET Core controllers + OpenAPI + Next.js fetch
│   ├── testing-strategy.md    # .NET test stack (pinned versions, Bogus fixtures)
│   └── graphify/              # Knowledge-graph skill (untouched)
├── commands/                  # Custom commands
│   ├── build.md test.md run.md clean.md lint.md format.md   # Solution-aware shortcuts
│   └── dotnet-build.md dotnet-test.md dotnet-run.md dotnet-clean.md dotnet-publish.md dotnet-lint.md
├── permissions/               # Permission rules (dotnet + npm + graphify bash allows)
│   └── default.md
├── lsp/                       # csharp-ls + TypeScript + JSON
│   └── default.md
└── plugins/
    └── graphify.js            # Knowledge-graph reminder hook (untouched)
```

## Solutions

| | Solution1 | Solution2 |
|---|---|---|
| Folder | `Solution1/` | `Solution2/` |
| Backend | ASP.NET Core MVC, .NET 10 | ASP.NET Core Web API, .NET 10 |
| Data access | Generic Repository + Unit of Work, In-Memory | Hand-rolled CQRS (no MediatR), EF Core + SQLite |
| Frontend | Razor + Bootstrap 5.3.3 + jQuery 3.7.1 | Next.js 16.3.5 + React 19 + TypeScript + Tailwind 4 |
| Tests | xunit + Moq + Bogus + FluentValidation + FluentAssertions | Same |

## Agents

| Agent | Purpose |
|-------|---------|
| `dns-zone-assistant` | Main development assistant (default) |
| `code-reviewer` | Code review against §7 domain rules + toolchain |
| `test-writer` | Test engineering (validator/handler tests) |
| `documentation-writer` | Keeps `docs/` + readmes in sync |

## Skills

| Skill | Description |
|-------|-------------|
| `api-design` | Controllers, OpenAPI (`/openapi/v1.json`), FluentValidation error shape, Next.js fetch |
| `testing-strategy` | Pinned test packages, Bogus seed-shaped fixtures, coverlet runs |

## Commands

| Command | Description |
|---------|-------------|
| `build` / `dotnet-build` | `dotnet build` a solution (`Solution1/…slnx`, `Solution2/…slnx`) |
| `test` / `dotnet-test` | `dotnet test` a solution (+ `--filter`, coverlet) |
| `run` / `dotnet-run` | `dotnet run` a server project; client via `npm run dev --prefix Solution2/src/client` |
| `clean` / `dotnet-clean` | `dotnet clean` a solution |
| `lint` / `dotnet-lint` | Roslyn analysis (`TreatWarningsAsErrors`); client via `npm run lint` |
| `format` | CSharpier (`dotnet csharpier format .`) for C#; `npm run format` for the client |
| `dotnet-publish` | `dotnet publish` a server project (Release, net10.0) |

## Formatters

- C#: [CSharpier](https://csharpier.com/docs/About) 1.3.0, pinned as a local dotnet tool (`.config/dotnet-tools.json`). Run `dotnet csharpier format .` / `check .`.
- Client: [Prettier](https://prettier.io/docs/) 3.9.7 (devDependency). Run `npm run format` / `format:check --prefix Solution2/src/client`. Defaults, no config file; `.prettierignore` covers `.next`.
- Versions + commands also pinned in `docs/TOOLCHAIN_AND_PACKAGES.md` §7.

## Usage

### Switch Agent
```
> agent code-reviewer
```

### Run Command
```
> build
> test --coverage
```

### Knowledge graph
```
> /graphify
```
(`graphify update .` after code changes; see `AGENTS.md`)

## Documentation

- [Opencode Config](https://opencode.ai/docs/config/)
- [Opencode Agents](https://opencode.ai/docs/agents/)
- [Opencode Models](https://opencode.ai/docs/models/) (Zen model IDs use `opencode/<model-id>`)
- [Opencode Commands](https://opencode.ai/docs/commands/)
- [Opencode Permissions](https://opencode.ai/docs/permissions/)
- [Opencode LSP](https://opencode.ai/docs/lsp/)
