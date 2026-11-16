Current source organization and refactoring verification: [architecture-refactoring.md](architecture-refactoring.md).

# Architecture decisions

## Current toolchain and solution

BankSphereAI.slnx is the canonical solution with BuildingBlocks, Services (context subfolders),
AI, Integrations, Gateway, Orchestration, and Tests solution folders. All 52 physical projects
are registered exactly once. Directory.Build.props supplies net9.0, C# 13, nullable types,
implicit usings, analyzer level 9.0, deterministic builds, and warnings as errors.
Global.json selects SDK 9.0.302. Package references use central version management.

The requested scaffolding already existed when this continuation began. Additional uncommitted
banking implementations, Persistence, and two test projects were also present. They are retained;
the existing banking work remains intact. The original six banking contexts are retained; Identity adds four service projects and two test projects, for 52 total.

## Dependency direction

For each of the six banking contexts, Identity, and AI:

```text
Api -> Application -> Domain -> SharedKernel
Api -> Infrastructure -> Application + Domain
Api -> ServiceDefaults -> Observability + Security
```

Banking Infrastructure also references the existing Persistence library, which references
SharedKernel. Security references SharedKernel. Domain and Application have no package or
framework references. No context references another context's Domain or Infrastructure.
SharedKernel and AI.Contracts have no project, package, or framework dependencies.
EventBus remains an empty library without invented abstractions.

AI.Agents, AI.Orchestration, AI.Rag, and AI.Memory each reference only AI.Contracts.
McpServer, A2AHost, and Gateway reference ServiceDefaults. AppHost references eleven Web SDK
projects for Aspire metadata and lifecycle management. These orchestration references do
not introduce business service dependencies. There are no circular references.

Identity Infrastructure additionally references Security for the existing role/permission vocabulary.
ASP.NET Core Identity framework models live in Infrastructure, keeping Identity Domain/Application
free of framework dependencies. All banking token validation remains in Security and has no direct
Identity Infrastructure dependency. See [Identity architecture and configuration](identity.md).

Foundation.Tests verifies the approved direct production dependency graph, inner-layer
isolation, exact solution registration, and inheritance of the shared net9.0 target.
The generated [inventory](project-inventory.md) records every actual edge, including tests.

## Host conventions

Observability retains JSON logs, ASP.NET Core traces/metrics, /health/live, and /health/ready.
ServiceDefaults adds service discovery and conditional OTLP export. Banking API composition
roots additionally use their existing security and SQL connectivity checks. AI.Api retains
its original root response and health-only readiness. No banking dependencies are added there.

AppHost uses Aspire.AppHost.Sdk 13.6.5, IsAspireHost, generated Projects metadata, and
AddProject registrations. AspireUseCliBundle=false retains direct SDK/IDE startup using
restored tooling; its existing narrow ASPIRE010 suppression is retained. A previous local
DCP restricted-file-policy runtime failure remains documented in verification.md.

## Package and migration decisions

Existing Aspire 13.6.5, OpenTelemetry 1.17.0, service discovery 10.0.0, xUnit 2.9.3,
runner 3.0.2, and Test SDK 17.12.0 versions are preserved. EF Core SQL Server/Design,
JWT bearer authentication, MVC testing, and the EF CLI tool use stable 9.0.23, verified
against NuGet V3 version indexes and actual restore. No audit warnings are suppressed.

The original EF 9 migrations are retained. On 2026-11-16, forward migrations added the
missing Accounts BalanceSnapshots table, recorded explicit default schemas, and transferred
Credit tables to the requested credit schema. All six contexts were migrated into the same
BankSphereAI_Development database with separate schema-qualified migration histories. SQL
catalog verification and all 43 tests pass; see [migration execution](migrations-2026-11-16.md).

No new business entities, handlers, agents, vector indexing, consumers, MCP tools, or A2A
workflows are introduced. Existing uncommitted implementations remain the user's work.
