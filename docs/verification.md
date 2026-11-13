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