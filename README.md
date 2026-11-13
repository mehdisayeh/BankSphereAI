# BankSphere AI

Enterprise banking investigation platform foundation: .NET 9, C# 13, nullable reference types,
Clean Architecture, and 43 registered projects. Open `BankSphereAI.slnx` in a Visual Studio
2022 17.14 or later, which supports .NET 9 and the XML solution format.

## Prerequisites and verification

.NET SDK 9.0.302 or a later 9.0.3xx patch is selected by `global.json`.
The Aspire SDK is separately pinned in `global.json`; no Aspire workload is required.
All NuGet package versions are managed in `Directory.Packages.props`.

```powershell
dotnet --list-sdks
dotnet sln BankSphereAI.slnx list
dotnet restore BankSphereAI.slnx
dotnet build BankSphereAI.slnx
dotnet test BankSphereAI.slnx
```

## Run the existing API

```powershell
dotnet run --project src/AI/BankSphere.AI.Api --no-launch-profile -- --urls http://localhost:5080
```

```powershell
curl.exe http://localhost:5080/
curl.exe http://localhost:5080/health/live
curl.exe http://localhost:5080/health/ready
```

The root returns `{"service":"BankSphere AI","status":"ready"}`. Both probes return
HTTP 200 and `Healthy`. Stop with Ctrl+C. Readiness currently checks the running process;
no database, messaging, vector store, or external AI service is registered.

Every banking API, integration host, and Gateway is a runnable Web SDK composition root
with the two health endpoints. Run any one with the same command and its project path,
choosing a different port. The integration hosts do not yet speak MCP or A2A; the Gateway
has no proxy routes. None implements business logic.

## Run all HTTP hosts with Aspire

Local verification found a DCP restricted-file-policy blocker on this machine: both the
system temp path and a workspace-local temp path were rejected for a detected reparse point.
The AppHost builds, but its dashboard and child-resource startup have not passed here.
See [the full blocker and diagnostic results](docs/verification.md). The ten hosts were each
verified directly and remain independently runnable.

```powershell
dotnet run --project orchestration/BankSphere.AppHost --launch-profile http
```

The local dashboard listens on `http://localhost:15888`; use the login link printed by
Aspire. Ten HTTP projects are registered with dynamic HTTP endpoints and readiness checks.
The launch profile enables HTTP transport for local development, binds OTLP to port 18889,
and the resource service to port 18890. No container resources are registered, so Docker
is not required for this scaffold. The AppHost starts managed executables, not business
workers. Use Ctrl+C to stop the AppHost and child resources.

`BankSphere.ServiceDefaults` shares the existing observability conventions and adds Aspire
service discovery and conditional OTLP export. Outside Aspire, telemetry export is disabled
unless `OTEL_EXPORTER_OTLP_ENDPOINT` is configured. JSON logs always go to stdout.

## Repository structure

- `src/BuildingBlocks`: SharedKernel, Observability, EventBus, Security.
- `src/Services`: six bounded contexts, each with Api/Application/Domain/Infrastructure projects.
- `src/AI`: existing four layers plus Agents, Orchestration, Rag, Memory, Contracts libraries.
- `src/Integrations`: MCP and A2A Web SDK hosts.
- `src/Gateway`: Web SDK Gateway host.
- `orchestration`: Aspire AppHost and shared ServiceDefaults library.
- `tests/BankSphere.Foundation.Tests`: retained primitive tests and expanded architecture checks.
- `tests/Unit`, `Integration`, `Architecture`, `Functional`: documented locations for future
  meaningful tests; no empty test assemblies.
- `workers`, `infrastructure`, `pipelines`: reserved directories.

See the [complete physical source tree](docs/repository-tree.txt),
[every project, target framework, and reference](docs/project-inventory.md),
[architecture decisions](docs/architecture.md), and [executed verification](docs/verification.md).

## Conventions and scope

`Directory.Build.props` enables analyzers, style enforcement, deterministic compilation,
nullable types, implicit usings, and warnings as errors. `.editorconfig` defines formatting.
`NuGet.Config` uses nuget.org. SharedKernel has no infrastructure or service dependencies.

The domain primitives and existing API response are preserved. New libraries contain only
project files until real responsibilities justify code. EventBus and Security contain no
fake abstractions. No entities, CQRS handlers, agents, workflows, RAG pipelines, repositories,
protocol tools, event consumers, or memory persistence have been implemented.

Database/Redis/messaging adapters, AI SDKs, authentication/authorization, deployment manifests,
and CI/CD remain future work. These diagnostic hosts must not receive banking data before
security and deployment configuration are implemented. Never log customer data, tokens, or prompts.