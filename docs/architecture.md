# Architecture decisions

## Solution and target framework

The inspected repository contained a seven-project `.sln`, not a `.slnx`. It was migrated
using `dotnet sln BankSphereAI.sln migrate`, then reorganized under BuildingBlocks, Services
(with bounded-context subfolders), AI, Integrations, Gateway, Orchestration, and Tests.
The obsolete `.sln` was removed to prevent opening an incomplete solution.
Every physical `.csproj` is registered in the canonical `BankSphereAI.slnx`.

All 43 projects inherit `net9.0` and C# 13 from Directory.Build.props. SDK 9.0.302 is pinned
in global.json. Nullable types, recommended analyzers, warnings as errors, deterministic
compilation, and formatting rules are retained. There are no per-project target overrides.

## Dependency direction

For each of Customers, Accounts, Transactions, FraudInvestigation, Compliance,
CreditAssessment, and AI:

```text
Api -> Application -> Domain -> SharedKernel
Api -> Infrastructure -> Application + Domain
Api -> ServiceDefaults -> Observability
```

Domain and Application have no package or framework references. Domain references only
SharedKernel. Infrastructure explicitly references Application and Domain, with no adapters
implemented. APIs own composition and host configuration; no service locator or empty DI
registration interfaces are introduced. Cross-context domain references are prohibited.

SharedKernel, EventBus, Security, and AI.Contracts reference no projects. SharedKernel's
working primitives are retained; the latter three libraries intentionally contain no source
implementations. AI.Agents, AI.Orchestration, AI.Rag, and AI.Memory each reference only
AI.Contracts. They have no runtime SDKs, agents, persistence, workflow orchestration, or
links to infrastructure. Add further dependencies only when an actual responsibility requires them.

McpServer, A2AHost, and Gateway reference only ServiceDefaults. They are health-only Web SDK
hosts; naming does not imply implemented protocols or proxying. The Aspire AppHost references
the ten executable HTTP projects for resource metadata and startup. These are orchestration
edges, not service-to-service business dependencies. No worker process exists yet.

The architecture test verifies the exact approved graph for all 42 production projects.
A separate test compares every physical project with the solution registrations, including
the retained Foundation.Tests project. Future architecture changes require deliberate review.
Do not hide new dependencies in shared MSBuild files.

## Retained DDD primitives

- Error validates immutable code and description, independent of HTTP.
- Result<T> guards value access on failure and rejects null success values.
- Entity<TId> exposes non-null identity, keeping reference equality. A future domain must
  enforce specific identity rules, including empty GUID validation where relevant.
- AggregateRoot<TId> retains a read-only view of domain events until explicitly cleared.
  There is no event dispatcher, transaction, or outbox yet. Future persistence must capture
  events durably before clearing them.
- IDomainEvent carries identity and occurrence time; implementations own UTC invariants.

No generic repository, mediator wrapper, empty handler, or business entity is introduced.
Existing narrowly scoped analyzer suppressions and their justifications are preserved.

## Host and Aspire conventions

All HTTP hosts use Web SDK, Problem Details, production exception handling, built-in DI,
JSON console logging, and the existing `/health/live` and `/health/ready` conventions.
Liveness selects `live`-tagged checks; readiness runs all registered checks. Future dependency
checks belong to readiness. Responses do not disclose dependency details.

Observability retains ASP.NET Core tracing/metrics and service resource registration.
ServiceDefaults is an Aspire shared library (`IsAspireSharedProject`) that reuses those
conventions, registers service discovery for HttpClient, and enables OTLP logs, metrics,
and traces only when an OTLP endpoint is configured. Do not log banking payloads.

AppHost uses Microsoft.NET.Sdk with Aspire.AppHost.Sdk, IsAspireHost, generated Projects
metadata, AddProject, dynamic HTTP endpoints, and HTTP readiness checks. It registers all
ten HTTP hosts. The local launch profile uses loopback HTTP ports and Aspire's dashboard
login token. No container or database resource is created. AppHost requires its bundled
Aspire orchestration/dashboard binaries, restored as SDK dependencies.

## Package availability and security

Versions were verified against nuget.org package metadata and the NuGet V3 flat-container
version indexes, then through actual restore/build. All explicit package references use
central versions; the MSBuild SDK version belongs in global.json, not PackageVersion.

- Aspire.AppHost.Sdk and Aspire.Hosting.AppHost: 13.6.5.
- OpenTelemetry hosting, ASP.NET instrumentation, OTLP exporter: 1.17.0.
- Microsoft.Extensions.ServiceDiscovery: 10.0.0.
- Existing xunit 2.9.3, runner 3.0.2, Microsoft.NET.Test.Sdk 17.12.0 retained and verified on net9.0.

Initial Aspire 13.1.0 and 13.6.4 restores were rejected by NuGet audit. Hosting and dashboard
advisories require the patched 13.6.5 release; warnings were not suppressed:

- https://github.com/advisories/GHSA-f7vh-h5g3-7jrh
- https://github.com/advisories/GHSA-4g86-54fm-h9h8
- https://www.nuget.org/packages/Aspire.Hosting.AppHost/13.6.5
- https://www.nuget.org/packages/Aspire.AppHost.Sdk/13.6.5
- https://www.nuget.org/packages/Microsoft.Extensions.ServiceDiscovery/10.0.0
- https://www.nuget.org/packages/OpenTelemetry.Extensions.Hosting/1.17.0
- https://www.nuget.org/packages/OpenTelemetry.Instrumentation.AspNetCore/1.17.0
- https://www.nuget.org/packages/OpenTelemetry.Exporter.OpenTelemetryProtocol/1.17.0

Normal restore resolves transitive dependencies; package lock files are not enabled.
Package advisories reflect the feed at verification time. No production security or
business functionality is claimed by this scaffold.
AspireUseCliBundle=false intentionally keeps direct dotnet/IDE startup with the restored
SDK tooling. Only the informational ASPIRE010 diagnostic is suppressed in AppHost with
an inline reason; NuGet audit remains enabled. AppHost runtime verification is blocked by
DCP's restricted-file policy on this machine; see verification.md.
