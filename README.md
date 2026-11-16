# BankSphere AI

The existing repository contains 52 registered .NET projects. All inherit .NET 9,
C# 13, nullable reference types, analyzers, and warnings as errors.
SDK 9.0.302 (or a later 9.0.3xx patch) is selected by global.json.
Use an IDE supporting the .NET 9 SDK and .slnx format, or the CLI.

## Verify

```powershell
dotnet --list-sdks
dotnet sln BankSphereAI.slnx list
dotnet restore BankSphereAI.slnx
dotnet build BankSphereAI.slnx
dotnet test BankSphereAI.slnx
dotnet tool restore
```

NuGet versions are centralized in Directory.Packages.props. The existing EF tool
manifest is aligned with EF Core 9.0.23. Prior EF 9 migration history is preserved.
The SQL integration tests use SQL Express by default, or BANKSPHERE_TEST_SQL_CONNECTION.
They create and delete uniquely named BankSphereAI_Tests_* or BankSphereAI_IdentityTests_* databases.
See [verification](docs/verification.md) for current build, migration, and test results.

## Identity and banking authentication

The [Identity guide](docs/identity.md) documents the four-layer service, registered OIDC
clients, role provisioning, certificate/SMTP configuration, endpoints, and token lifecycle.
The Identity migration is applied to the shared BankSphereAI_Development database.

All six banking APIs independently validate Identity access tokens through BankSphere.Security.
The existing six roles/policy names are preserved. Existing policies now also require the
matching permission and banking scope. Public registration receives no roles; privileged
roles are assigned only through the explicit deployment command documented in the guide.

Production requires external RSA signing/encryption certificates, protected persistent
Data Protection keys, an HTTPS issuer/binding, registered HTTPS callbacks, and SMTP settings.
No private keys or passwords are committed. Development alone can use OpenIddict-generated
certificates. Account UI and Gateway work remain separate from the Identity API.

## Start the AI API

```powershell
dotnet run --project src/AI/BankSphere.AI.Api --no-launch-profile -- --urls http://localhost:5080
curl.exe http://localhost:5080/
curl.exe http://localhost:5080/health/live
curl.exe http://localhost:5080/health/ready
```

The root returns {"service":"BankSphere AI","status":"ready"}; both health endpoints
return HTTP 200 Healthy. Stop with Ctrl+C. This API requires no database or AI credentials.
Gateway, McpServer, and A2AHost can also run directly with their project paths and a distinct
port. They provide diagnostic endpoints; protocol tools and proxy routes are not implemented.

The six existing banking API composition roots require ConnectionStrings__Banking.
They also contain existing authentication and downstream-service configuration requirements;
see their Extensions/DependencyInjection.cs files. Their readiness probes check SQL connectivity. Those pre-existing
implementations were preserved during the framework migration.

```powershell
dotnet run --project orchestration/BankSphere.AppHost --launch-profile http
```

AppHost registers eleven hosts, with Identity on HTTPS port 7100. Full orchestration still has a previously recorded local
DCP restricted-file-policy blocker. Banking configuration must also be supplied before the
six banking APIs can start. Successful full orchestration is not claimed.

## Structure

- src/BuildingBlocks: SharedKernel, Observability, EventBus, Security, and existing Persistence.
- src/Services: seven contexts (six banking contexts plus Identity), each with Api, Application, Domain, and Infrastructure projects.
- src/AI: nine projects; specialized AI libraries have no implemented AI features.
- src/Integrations and src/Gateway: Web SDK composition roots.
- orchestration: Aspire AppHost and ServiceDefaults.
- tests: Foundation, banking unit/API contracts, SQL persistence, Identity unit/security, and Identity SQL/HTTP integration tests.
- workers, infrastructure, pipelines: reserved directories with .gitkeep.

The [physical tree](docs/repository-tree.txt) excludes generated bin/obj, Git/IDE metadata,
and local runtime files. The [project inventory](docs/project-inventory.md) lists every
registered project, its restored target framework, and all direct project references.
See [architecture](docs/architecture.md) for dependency decisions.

Identity adds ASP.NET Core Identity, OpenIddict, asymmetric JWTs, PKCE, session revocation,
and the dedicated identity schema. Existing banking features, schemas, and API routes remain.
No Gateway proxy implementation or AI functionality was added.
