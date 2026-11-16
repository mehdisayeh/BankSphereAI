Subsequent migration work on 2026-11-16 resolved the SQL blocker recorded below: see [migration execution](migrations-2026-11-16.md). The refactoring report remains a historical record.

# Architecture refactoring — 2026-11-14

The existing .NET 9 solution and all 46 projects are retained. No service, deployment
boundary, package, banking feature, or AI capability was added. The working tree already
contained substantial uncommitted banking work; the refactoring starts from that state.

## Inspection and baseline

Inspection covered 127 C# and project files, all project references, domain declarations,
CQRS handlers, repositories, contexts, configurations, migrations, API registrations,
security/error middleware, and all three test projects. There was no repository AGENTS.md.

Project responsibilities and direct references are listed individually in
[project-inventory.md](project-inventory.md). The projects fall into these existing groups:

| Projects | Responsibility |
| --- | --- |
| Six banking contexts × Domain/Application/Infrastructure/Api (24) | Customers, Accounts, Transactions, FraudInvestigation, Compliance, CreditAssessment; invariants, use cases, persistence/gateways, HTTP composition respectively |
| SharedKernel, Persistence, Security, Observability, EventBus (5) | Shared primitives/results, EF/outbox support, authentication/authorization, telemetry, event-bus scaffold |
| AI projects (9) | Existing API, Application, Domain, Infrastructure, Contracts, Agents, Orchestration, Rag, Memory scaffolds; unchanged |
| Gateway, McpServer, A2AHost (3) | Existing gateway/integration hosts; unchanged |
| AppHost, ServiceDefaults (2) | Orchestration and shared host defaults |
| Foundation.Tests, Banking.Tests, Persistence.Tests (3) | Architecture/primitives, banking regression tests, SQL integration tests |

The dependency graph already pointed inward: Domain → SharedKernel, Application → Domain,
Infrastructure → Application/Domain/Persistence, Api → Application/Infrastructure/ServiceDefaults.
No circular references or infrastructure dependencies in Domain/Application were found.

The structural problems were six multi-type aggregate files, a large BankingPrimitives.cs,
six unrelated-operation `*Slices.cs` files, contexts mixed with design-time factories,
persistence classes at project roots, and 18 endpoint registrations embedded in Program.cs.
Two read endpoints called repositories directly. Existing validators were inline guards
and domain methods, with no separate validator framework or command/query bus.

Baseline: solution build passed with zero warnings/errors; 6 foundation and 21 banking tests
passed; all 5 SQL tests failed during fixture initialization with AccountsDbContext
`PendingModelChangesWarning`. The existing BalanceSnapshots mapping is absent from the
Accounts initial migration/snapshot. This predates the refactor.

## Final organization

The complete physical tree is in [repository-tree.txt](repository-tree.txt). Each banking
context follows this structure, with optional folders present only where used:

```text
src/Services/<Context>/
  BankSphere.<Context>.Domain/
    Aggregates/<Aggregate>/       # root, owned entities, enums, value objects
    Events/                      # existing domain/integration events
    Policies/                    # domain policy interfaces
  BankSphere.<Context>.Application/
    Features/<UseCase>/           # operation, handler, feature-only input/result types
    Abstractions/Persistence/     # repository contracts
    Abstractions/Services/        # cross-context gateway contracts
    DTOs/                        # response types reused by multiple operations
    Policies/                    # existing calculation policy
  BankSphere.<Context>.Infrastructure/
    Persistence/
      <Context>DbContext.cs
      Configurations/
      Repositories/
      Design/
      Migrations/
    Gateways/
  BankSphere.<Context>.Api/
    Endpoints/<UseCase>Endpoint.cs
    Extensions/DependencyInjection.cs
    Extensions/EndpointRegistrationExtensions.cs
    Program.cs
```

SharedKernel now has Domain, ValueObjects, Results, Validation, Pagination, and Security
folders. Shared Persistence has Outbox and Mapping folders. Banking-specific types remain
inside their owning contexts. Owned entities stay alongside their aggregate; they are not
duplicated in a second Entities folder. Owned EF mappings stay within their aggregate
configuration, preserving ownership and effective mappings.

Important splits and moves:

| Original files | Destination |
| --- | --- |
| Customer.cs, BankAccount.cs, BankingTransaction.cs, FraudCase.cs, ComplianceCase.cs, CreditApplication.cs | One declared type per file under each owning Domain aggregate, Events, or Policies folder |
| BankingPrimitives.cs and root SharedKernel types | Dedicated matching filenames in responsibility folders |
| CustomerSlices.cs | CreateCustomer, GetCustomerById, UpdateCustomerContact features; shared customer response DTOs; repository abstraction |
| AccountSlices.cs | OpenAccount and GetAccountById features, shared account response DTOs and abstractions |
| TransactionSlices.cs | IngestSimulation and GetTransactionsByAccount features, shared transaction DTO and abstractions |
| FraudSlices.cs | OpenFraudInvestigation, GetFraudInvestigation, RequestFraudReview, AttachFraudEvidence, RecordFraudDecision features |
| ComplianceSlices.cs | OpenComplianceCase and GetComplianceCase features |
| CreditSlices.cs | SubmitCreditApplication and GetCreditApplicationStatus features; FinancialCalculationPolicy in Policies |
| Six DbContext files | Separate context and matching DesignFactory files under Persistence and Persistence/Design |
| Configuration/repository/gateway files | Persistence/Configurations, Persistence/Repositories, Gateways |
| Existing Migrations directories | Persistence/Migrations; contents and namespaces retained |
| OutboxDbContext.cs | Separate OutboxMessage, OutboxDbContext, BankingMapping files |
| Six API Program.cs files | 18 individual endpoint files and explicit registration extensions |
| SQL integration fixture | SqlDatabase.cs, separate from SqlPersistenceTests.cs |

GetCustomerEligibility and GetTransactionById are now application features. Their handlers
contain the exact repository calls previously in HTTP endpoints, with the original nullable
eligibility result and transaction not-found error preserved. All 18 use cases have a
dedicated feature directory. Existing handlers accepting scalar identifiers or filters
retain those signatures; no artificial query wrappers, generic base handlers, messaging
library, or new validation framework was introduced. Existing validation order is unchanged.

## Compatibility and dependency decisions

Existing public type names and namespaces deliberately remain stable, including EF context,
entity, and event identities. Physical folders carry the new internal navigation boundaries;
renaming public namespaces to every leaf folder would break consumers and affect EF snapshot
identities. New API endpoint and composition namespaces match their folders. All handwritten
production types use file-scoped namespaces and inherit nullable enablement. Generated EF
migrations retain their tooling-generated namespace form.

No production references or package versions changed in this refactor. Banking.Tests adds
references to the six existing API projects to exercise their actual registration methods.
HTTP/DI composition remains in Api, keeping ASP.NET and DI framework dependencies out of
Application. Domain invariants, inline validation, transaction/outbox behavior, gateway
timeouts, authentication, middleware, response contracts, and health registrations are retained.

Related endpoints use resource-specific route groups with empty prefixes and full route
templates. This deliberately preserves exact route metadata: a prefixed group plus an empty
collection template introduced a trailing slash, which the contract tests detected and the
implementation corrected. Policies remain on individual endpoints as before.

## Verification and remaining issues

Builds and the complete existing test suite were run after Domain, Application,
Infrastructure, and API phases. Each phase built with zero warnings/errors and retained
the baseline 27 passing / 5 failing test result before new tests were added.

Ten regression cases were added: six exercise all 18 exact routes, HTTP methods,
authorization policies, and resolution of all registered handlers and dependencies without
opening SQL connections; four check extracted read behavior, missing results/error contracts,
clock values, and cancellation propagation.

A Roslyn comparison against the pre-refactor working-tree snapshot verifies identical C#
tokens and namespaces for all 200 original production type declarations, including handler
bodies, model conversions, constraints, repositories, and event contracts. No duplicate
production declarations exist. All 18 migration/designer/snapshot files retain their text;
they were moved without regeneration. No effective schema change was introduced.

Final results and commands are recorded at the top of [verification.md](verification.md).
The build passes with zero warnings/errors. Tests: 37 passed, 5 failed, 0 skipped. The only
failures are the original five SQL tests at Accounts migration initialization; no failures
were suppressed, skipped, or deleted. Full SQL end-to-end behavior therefore remains unverified.

The Accounts model/migration gap requires a separate reviewed schema change. The previously
documented AppHost DCP runtime blocker and production deployment configuration remain separate
work; they were not retested by this structural refactoring.
