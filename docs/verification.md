# Identity implementation verification ? 2026-11-16

This section supersedes earlier solution counts and test totals. See [Identity architecture,
configuration, endpoints, and operations](identity.md) for the implementation details.

| Command/check | Actual result |
| --- | --- |
| `dotnet build BankSphereAI.slnx -v quiet` | Passed; 0 warnings, 0 errors; all 52 projects |
| `dotnet test BankSphereAI.slnx --no-build -v quiet` | **79 passed, 0 failed, 0 skipped** |
| Foundation.Tests | 6 passed; approved project graph, inner-layer isolation, .NET 9 inheritance, and exact solution registration |
| Banking.Tests | 31 passed; existing banking domain and route/registration contracts retained |
| Persistence.Tests | 6 passed against isolated SQL Server database |
| Identity.Tests | 15 passed; session invariants, validators/redaction, all six existing policies, JWT issuer/audience/expiry/signature/type/algorithm checks |
| Identity.IntegrationTests | 21 passed; real SQL stores and full HTTPS TestServer request pipelines |
| `dotnet ef database update --context IdentityDbContext --project src/Services/Identity/BankSphere.Identity.Infrastructure` | Applied `20261116101820_InitialIdentityAndOidc` to BankSphereAI_Development |
| Final update with `--no-build` | Database already up to date |
| `dotnet ef migrations has-pending-model-changes --context IdentityDbContext --project src/Services/Identity/BankSphere.Identity.Infrastructure --no-build` | No pending model changes |
| Development Identity catalog/model comparison | 15 model tables + history, 32 model indexes/keys, 12 FKs verified; all in identity schema |
| Role/permission catalog initialization | Exactly six existing roles, six matching permissions, six grants; zero seeded users/credentials |
| `DBCC CHECKCONSTRAINTS WITH ALL_CONSTRAINTS` | No violations reported in the shared development database |
| `dotnet list src/Services/Identity/BankSphere.Identity.Api package --vulnerable --include-transitive` | No known vulnerable packages reported by current NuGet sources |
| Structural checks | One matching primary public type/file, file-scoped handwritten namespaces, no committed PFX/PEM files |

The banking HTTP integration cases authenticate through Identity login + authorization-code/S256
PKCE exchange. Every banking API then uses actual discovery/JWKS validation and the shared
session checker: authorized requests return 200, anonymous/tampered requests 401, and valid
unprivileged requests 403. No Gateway participates.

Additional integration checks cover unprivileged registration and email confirmation,
lockout, CSRF, role provisioning without a public grant route, current-user ownership,
logout/revoke-all, password change/reset/token reuse, refresh rotation, concurrent redemption,
refresh/code replay, scope reduction without expansion, invalid PKCE/redirects, immediate
permission removal, Identity outage rejection, rate limiting, HTTPS enforcement, and production
startup rejection without external signing keys.

A concurrent-redemption test initially exposed two successful refreshes for one credential.
The final implementation serializes full code/refresh redemption with a SQL application lock
across Identity replicas, using a token digest as the lock name and a separate bounded pool.
The retained regression test now proves exactly one success; replay invalidates the family
and signed access tokens on subsequent API requests. No tests were skipped or weakened.

The existing six banking schemas still contain their original 67 tables including histories.
Identity adds 16 tables, for 83 user tables in the shared development database. No banking
migration, table, business operation, or API route was replaced. Existing role/policy names
are preserved; Identity tokens carry the corresponding permission and banking scope claims.
Symmetric authentication fallback was intentionally replaced with HTTPS discovery and RS256.

Test users/data reside in guarded, uniquely named temporary SQL databases. Certificates and
passwords are randomly generated for tests; email is captured in memory. Production SMTP,
external account UI, real TLS/proxy deployment, and external secret provisioning were not
exercised. AppHost includes Identity but was not launched; no Gateway process was started.
Detailed run artifacts remain locally in `.local/identity/completed-tests/` and the development
catalog report in `.local/identity/catalog.json`.

---

# SQL migration verification ? 2026-11-16

This section supersedes earlier SQL migration blockers and test totals. Full execution details,
applied migration IDs, and created tables per service are in [migrations-2026-11-16.md](migrations-2026-11-16.md).

- All six `dotnet ef database update` commands succeeded against `BankSphereAI_Development`
  on the existing `.\SQLEXPRESS` instance, using the existing design-time factories.
- The database was absent before execution; EF provisioned the requested name. No existing
  database or tables were dropped or recreated.
- Every context now declares `HasDefaultSchema()` and owns its schema-qualified history:
  customers, accounts, transactions, fraudinvestigation, compliance, credit.
- Six forward migrations record default schemas, add the missing Accounts BalanceSnapshots,
  and transfer Credit tables from creditassessment to credit without recreating them.
- 67 tables verified: 61 model tables and six history tables, with two applied migrations per context.
- All model columns, indexes/keys, relationships/delete actions, and check constraints match
  SQL Server catalog metadata; constraints are enabled/trusted. DBCC reported no violations.
- All six contexts report no pending model changes. Original migrations were retained.
- `dotnet build BankSphereAI.slnx -v quiet`: passed, 0 warnings, 0 errors.
- `dotnet test BankSphereAI.slnx --no-build -v minimal`: **43 passed, 0 failed, 0 skipped**
  (6 foundation, 31 banking, 6 SQL persistence). All five earlier SQL failures are resolved.

---

# Architecture refactoring verification - 2026-11-14

This section supersedes earlier verification totals. The .NET 9 target remains unchanged.
See [architecture-refactoring.md](architecture-refactoring.md) for inspection findings,
folder boundaries, important moves, compatibility decisions, and remaining work.

| Command/check | Result |
| --- | --- |
| Baseline `dotnet build BankSphereAI.slnx -v quiet` | Passed; 0 warnings, 0 errors |
| Baseline `dotnet test BankSphereAI.slnx --no-build -v quiet` | 27 passed, 5 failed, 0 skipped |
| Build/test after each Domain, Application, Infrastructure, API phase | Builds passed; same 27 passing / 5 failing baseline tests |
| Final `dotnet build BankSphereAI.slnx -v quiet` | Passed; 0 warnings, 0 errors |
| Final `dotnet test BankSphereAI.slnx --no-build -v quiet` | 37 passed, 5 failed, 0 skipped |
| Foundation.Tests | 6 passed; approved dependency graph, inner-layer isolation, solution registration and net9.0 inheritance remain valid |
| Banking.Tests | 31 passed: existing 21 plus 6 route/authorization/DI cases and 4 read-handler cases |
| Persistence.Tests | Same 5 existing failures during AccountsDbContext migration initialization |
| Roslyn syntax comparison with pre-refactor working-tree snapshot | All 200 original production type declarations retain identical tokens and namespaces; no duplicates |
| Migration/designer/snapshot comparison | All 18 files unchanged; moved to Persistence/Migrations without regeneration |
| Structural audit | One matching public type per handwritten production file; 18 feature folders; no obsolete `*Slices.cs`; production project files unchanged |

The new API cases inspect all 18 exact route templates, HTTP methods, authorization
policies, and actual handler dependency resolution. They do not start a network listener or
connect to SQL. The four new read-handler cases verify nullable eligibility, transaction
responses/not-found errors, clock values, and cancellation forwarding.

The full suite still fails overall. `PendingModelChangesWarning` for AccountsDbContext is
the same baseline failure: its existing BalanceSnapshots mapping is missing from the initial
migration/snapshot. No schema repair or new migration was introduced, no warning suppressed,
and no test skipped or removed. SQL end-to-end success is not claimed. The integration fixture
still uses its existing isolated temporary SQL database lifecycle.

The existing public APIs, banking behavior, EF mappings, event contracts, authentication,
error middleware, health checks, and production project references were preserved. Only
Banking.Tests gains six existing API project references for contract tests. No package,
framework, service, deployment, or AI capability changed. Existing uncommitted work remains.
AppHost runtime orchestration was not retested in this refactoring.

---

# Prior .NET 9 verification - 2026-11-14

The user's latest instruction restores .NET 9 for every project. This section supersedes
all framework and verification results below.

- global.json selects installed SDK 9.0.302; Directory.Build.props selects net9.0,
  C# 13, and analyzer level 9.0.
- All 46 restored project.assets.json files were inspected: all target net9.0.
- EF Core, JWT bearer authentication, MVC testing, and dotnet-ef restored to stable 9.0.23.
- dotnet restore BankSphereAI.slnx: passed.
- dotnet build BankSphereAI.slnx -v quiet: passed, zero warnings and errors.
- dotnet tool restore: passed, dotnet-ef 9.0.23.
- dotnet test BankSphereAI.slnx --no-build -v quiet: failed overall; foundation 6 passed,
  banking unit 21 passed. A separate dotnet test tests/Integration/BankSphere.Persistence.Tests
  --no-build -v minimal confirmed all 5 SQL tests still fail on the existing Accounts
  PendingModelChangesWarning. The schema gap is unchanged and was not suppressed.
- Framework regression test now enforces shared net9.0 inheritance.
- Reopened both running Visual Studio solutions via DTE: BankSphereAI.slnx and
  genai-agent-lab.sln. Both solutions reported IsOpen=true after reopening.
  Desktop-control helper was unavailable; the Visual Studio automation API was used.

The existing banking implementations and migrations are preserved. README, architecture,
and project inventory describe the restored .NET 9 target. Older .NET 10 verification is
historical only.

---

# Current .NET 10 verification - 2026-11-14

This section supersedes all historical results below. Commands were executed in
C:\Users\hi\source\repos\BankSphereAI with SDK 10.0.201.

| Command or check | Actual result |
| --- | --- |
| dotnet --list-sdks | 9.0.302 and 10.0.201 installed |
| dotnet sln BankSphereAI.slnx list | Passed: 46 registered projects |
| dotnet restore BankSphereAI.slnx | Passed: all 46 projects |
| dotnet build BankSphereAI.slnx -v minimal | Passed: 0 warnings, 0 errors |
| dotnet tool restore | Passed: dotnet-ef 10.0.15 |
| dotnet test BankSphereAI.slnx -v minimal | Failed overall: 27 passed, 5 failed, 0 skipped |
| Foundation.Tests | 6 passed, including project graph, solution registration, and net10.0 inheritance |
| Banking.Tests | 21 passed |
| Persistence.Tests | 5 failed during fixture initialization: Accounts pending model changes |
| Restored project frameworks | All 46 project.assets.json files target net10.0 |
| AI API compiled DLL startup | Passed: Production environment on http://localhost:5080 |
| GET / | HTTP 200: {"service":"BankSphere AI","status":"ready"} |
| GET /health/live | HTTP 200: Healthy |
| GET /health/ready | HTTP 200: Healthy |

The API was run in a foreground terminal and stopped after probing. An initial background
Start-Process smoke-test command was rejected by automatic approval review with 'blocked by
policy'; no more specific reason was provided. Foreground startup and separate HTTP probes
completed the verification without that action.

## Existing work preserved

All requested scaffold projects already existed at continuation time. The working tree also
contained existing banking entities, handlers, repositories, migrations, security, Persistence,
and two additional test projects. These were preserved. No new banking or AI feature was added.
Only toolchain/package configuration, a framework regression check, and documentation changed
in this continuation. Prior uncommitted changes remain uncommitted.

Package versions were verified using NuGet V3 flat-container indexes; EF SQL Server/Design,
JWT authentication, MVC testing, and dotnet-ef all provide stable 10.0.15. Restore/build verified
compatibility. Existing Aspire 13.6.5, OpenTelemetry 1.17.0, and test packages were retained.

## Unresolved SQL integration failure

SQL Express is running and the tests can reach it. Fixture initialization fails with
PendingModelChangesWarning for AccountsDbContext. The existing context includes
AccountBalanceProjectionConfiguration (BalanceSnapshots), while the existing initial
Accounts migration and snapshot do not include that table.

A diagnostic migration was generated to inspect the difference: its only schema operation
was creation of accounts.BalanceSnapshots. It was then removed with dotnet ef migrations
remove, reverting the snapshot. No diagnostic migration remains, no application database
migration was applied, and the pending-model warning was not suppressed. The test fixture
uses its own uniquely named temporary database and deletes it during teardown.

A reviewed Accounts schema migration is still required before those five integration tests
can pass. This is an existing banking implementation/schema gap outside the requested
scaffolding phase. Overall test success is not claimed.

## Executable status and local orchestration

AI.Api startup and both health endpoints were verified in this continuation. Gateway,
McpServer, and A2AHost have diagnostic Web SDK roots and require no banking database.
The six banking APIs now contain pre-existing business code and require ConnectionStrings:Banking,
SQL schema provisioning, authentication settings, and any downstream URLs used by their roots.
Their startup was not reverified here.

AppHost builds and registers ten HTTP projects. Its previously recorded DCP restricted-file-policy
runtime blocker has not been retested or resolved in this continuation. The six banking APIs'
configuration is an additional prerequisite. Do not interpret the historical ten-host smoke test
below as verification of the current banking implementations.

See project-inventory.md for every project's actual restored framework and direct references,
and repository-tree.txt for the actual source tree (generated outputs and metadata excluded).

---

# Historical verification (superseded)

# Visual Studio 2022 / .NET 9 verification

Verified on 2026-11-13 after adapting the repository to the installed Visual Studio 2022
Professional 17.14 and .NET SDK 9.0.302. This section supersedes the framework/toolchain
results in the historical .NET 10 verification below.

- global.json selects SDK 9.0.302 with latestPatch roll-forward.
- Directory.Build.props selects net9.0, C# 13, and analyzer level 9.0.
- All 43 restored project.assets.json files were inspected: every project targets net9.0.
- `dotnet restore BankSphereAI.slnx`: passed for all projects.
- `dotnet build BankSphereAI.slnx --no-restore -v quiet`: passed, 0 warnings, 0 errors.
- Visual Studio 2022's own `MSBuild/Current/Bin/amd64/MSBuild.exe` was executed against
  `BankSphereAI.slnx` with `/restore /m /nologo /verbosity:quiet`: exit code 0.
- `dotnet test BankSphereAI.slnx --no-build --no-restore -v minimal`: 5 passed,
  0 failed, 0 skipped, using net9.0.
- The net9.0 AI API started on port 5080. `/`, `/health/live`, and `/health/ready`
  each returned HTTP 200; the health bodies were `Healthy`.

All 43 projects, architecture boundaries, and package versions are retained. Restore and
both build engines verified the existing packages on .NET 9; no speculative package
downgrades were needed. Close and reopen the solution in Visual Studio to reload the
SDK selection if it still shows cached project-load errors.

The previously documented DCP reparse-point runtime blocker is separate from SDK resolution
and has not been reclassified as resolved. AppHost builds under both the .NET 9 CLI and
Visual Studio 2022 MSBuild. No banking logic or AI features were added.

---

Historical .NET 10 verification (retained for context; not the current target):

# .NET 10 solution verification

Executed on 2026-11-13 with .NET SDK 10.0.201 on Windows.

| Executed command/check | Actual result |
| --- | --- |
| `dotnet --list-sdks` | 9.0.302 and 10.0.201 installed |
| `dotnet sln BankSphereAI.slnx list` | Passed; 43 registered projects |
| `dotnet restore BankSphereAI.slnx` | Passed after the package and disk-space corrections below |
| `dotnet build BankSphereAI.slnx` | Passed; 0 warnings, 0 errors |
| `dotnet test BankSphereAI.slnx` | Passed; 5 tests, 0 failed, 0 skipped, net10.0 |
| `dotnet list BankSphereAI.slnx package --vulnerable --include-transitive` | No known vulnerable packages reported for all 43 projects |
| Restored target frameworks read from each project's assets | All 43 are net10.0; see project-inventory.md |
| AI API direct Kestrel startup | Passed, Production environment, port 5080 |
| `GET /` on AI API | HTTP 200; `{"service":"BankSphere AI","status":"ready"}` |
| `GET /health/live` on AI API | HTTP 200; `Healthy` |
| `GET /health/ready` on AI API | HTTP 200; `Healthy` |
| Nine additional Web SDK hosts started directly | Each returned HTTP 200 `Healthy` from `/health/ready` |
| Aspire AppHost runtime orchestration | BLOCKED by DCP restricted-file policy; see below |

The nine additional hosts checked were Gateway, A2AHost, McpServer, and Accounts,
Compliance, CreditAssessment, Customers, FraudInvestigation, and Transactions APIs.
Each was launched as its compiled net10.0 executable on a distinct loopback port in
5091-5099, probed through Invoke-WebRequest, and stopped in a finally block.
The AI API and AppHost verification processes were also stopped.

The original primitive tests remain. The expanded architecture test verifies direct references
for all 42 production projects and inner-layer isolation. The fifth test verifies that every
physical project is registered exactly once in the solution.

## Corrections made during verification

Initial Aspire 13.1.0 and 13.6.4 package selections were rejected by NuGet security audit.
The final SDK/hosting version is patched stable 13.6.5; OpenTelemetry is aligned at 1.17.0.
No package security warnings were suppressed. A CA1861 analyzer finding in the expanded test
was fixed using a static readonly field.

Aspire's ASPIRE010 warning is narrowly suppressed in AppHost with an explanatory comment:
the deliberate `AspireUseCliBundle=false` setting supports direct dotnet/IDE startup using
restored DCP/dashboard packages, without requiring a separately installed CLI bundle.
This documented SDK mode is described at https://aspire.dev/diagnostics/aspire010/ .

The first patched Aspire restore ran out of disk space. Automatic approval review rejected
manual removal of old expanded package-cache directories with `blocked by policy`; none
were deleted. `dotnet nuget locals http-cache --clear` then succeeded, freeing download-cache
space while retaining installed packages. Restore/build/test subsequently succeeded.

## Unresolved local AppHost runtime blocker

Both the normal AppHost launch and a diagnostic retry using workspace-local TEMP/TMP
failed before the dashboard or child resources started. DCP 13.6.5 reported:

```text
could not write kubeconfig file ...:
restricted file policy: restricted file path contains a reparse point:
restricted file path traverses a reparse point: ...
The object manager encountered a reparse point while retrieving an object.;
choose an absolute path on a fixed local ACL-capable drive without junction or symlink ancestors
```

The normal temporary root was `C:\Users\hi\AppData\Local\Temp`; the second root was
`C:\Users\hi\source\repos\BankSphereAI\.local\aspire-temp`. The retry changed only
process-local TEMP/TMP, not machine settings. `.local/` is ignored by Git.
The underlying reparse-point cause was not established; ordinary directory attributes
alone did not reveal it. No filesystem security checks, ACLs, or SDK security fixes were bypassed.

AppHost compiles and its ten AddProject registrations are present, but successful local
orchestration is NOT claimed. Run it on a fixed local ACL-capable path whose entire ancestor
chain passes DCP's restricted-file policy, then recheck dashboard and resource readiness.
All ten HTTP hosts were independently verified and can be run directly meanwhile.
No build/test or SDK-availability blockers remain. No business or AI features were implemented.
