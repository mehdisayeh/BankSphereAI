# Banking domain design — implementation gate

This catalog defines the boundaries before implementation. The platform targets .NET 9,
Visual Studio 2022, and SQL Server. These are investigation and case-management models,
not a certified banking core. No balance mutation, settlement, disbursement, or AI is modeled.

## Consistency boundaries and types

All aggregate IDs are server-generated nonempty UUIDs. Cross-context UUIDs are references,
not EF navigations: a context cannot transact against another context's private tables.
Audited roots use CreatedAtUtc/UpdatedAtUtc (DateTimeOffset, offset zero) and a monotonically
incremented long Version concurrency token. Domain methods alone change lifecycle state.
Actor identifiers, rationale, and evidence references accompany accountable decisions.
Money is immutable decimal plus ISO currency and validates fraction digits. Currency uses
an explicit supported ISO 4217 catalog, never a permissive three-letter regex. Nonnegative
amounts are required; transaction direction represents debit/credit rather than signed amounts.
Address, EmailAddress, Iban, Percentage, and effective DateRange are immutable value objects.

## Aggregate catalog and schema

| Context/schema | Root and scalar profile | Owned relationships in separate tables | Consistency/invariants |
| --- | --- | --- | --- |
| customers | Customer: UUID, number, type/segment/status, personal OR corporate identity, onboarding/language/channel/manager, financial activity profile, KYC/risk and audit | IndividualProfile OR CorporateProfile; Addresses, Contacts, IdentityDocuments, Consents, Relationships, BeneficialOwners, RiskProfiles and RiskFactors | Individuals require adult identity; corporations require registration; identity verification needs verified unexpired document; activation requires verified KYC; closure is terminal; ownership percentages bounded |
| accounts | BankAccount: UUID/number/IBAN/BIC, product/type/currency, primary customer, branch, lifecycle dates, purpose, plan references, limits, audit | Ownerships, Mandates, Restrictions, StatusHistory, optional immutable BalanceSnapshot | Starts pending; activate requires recent customer AND compliance eligibility; freeze/unfreeze and suspend transitions explicit; close requires authoritative zero snapshot/no pending amounts; no repository operation changes balances |
| transactions | BankingTransaction: simulator record identity/external ref, customer/account, direction/type/status, Money and original Money, FX ref, dates, payment channel/method/scheme, description and provenance | Merchant, Counterparty, Instrument, Metadata, RiskSignals, Enrichments, StatusHistory | Immutable accepted simulator record; positive Money; same-currency original amount or explicit FX reference; batch/scenario/source provenance required; unique source+external ref; conflicting replay rejected |
| fraudinvestigation | FraudCase: UUID/number, customer/account/transaction links, category/type, severity/priority, source/time, score/level, description, exposure Money, reporter, due/lifecycle dates, resolution, audit | Indicators, Evidence, Notes, Assignments, StatusHistory, HumanReviews, Decisions, TransactionLinks | One active case per transaction+category; database filtered unique index enforces concurrent creation; review only from investigating/escalated; decision requires authorized reviewer; resolve requires human decision; close/reopen rules preserve histories |
| compliance | ComplianceCase: customer/type/status, jurisdiction, reason/source, officer, risk, due/open/review/close timestamps and rationale | KycReviews, DueDiligence, Screenings (sanctions/PEP/adverse media protected refs), Documents, ReviewHistory, RiskFactors | Review workflow records evidence and accountable officer; high risk requires enhanced diligence; approval never means verified regulatory screening; historical reviews retained |
| credit | CreditApplication: UUID/number/customer/product/purpose, requested Money/term, channel/officer/submission state and audit | FinancialProfile, IncomeSources, ExistingDebts, Obligations, Collateral, Documents, Assessments/RiskFactors, Recommendations, ReviewRequests, DecisionHistory | Draft→submitted→assessment→human review→approved/rejected; document request is explicit; no decision without rationale/evidence/actor; assessed income/expenses currencies agree; configurable assessment policy, no universal hardcoded lending threshold; no automatic recommendation execution |

Child collections use aggregate ownership and cascade only within the consistency boundary.
Business numbers, simulator source/external reference, and active investigation keys have unique
indexes. Every owned collection has a root foreign key and stable UUID key. Money columns use
decimal(19,4); amounts with more ISO minor digits are rejected before persistence. Currency is
char(3), timestamps datetimeoffset; indexes cover account/date transaction queries and case keys.
Explicit IEntityTypeConfiguration classes own mappings; Domain has no EF or AI dependencies.

## Application slices and boundaries

CreateCustomer/GetCustomerById, OpenAccount/GetAccountById, GetTransactionsByAccount,
OpenFraudInvestigation/GetFraudInvestigation/RequestFraudReview,
SubmitCreditApplication/GetCreditApplicationStatus have dedicated command/query handlers,
request/response projections, cancellation, domain validation and repository ports with
domain-specific semantics. Additional endpoints expose only fully implemented lifecycle actions.
Validation uses domain guards plus command-level validation (equivalent to FluentValidation),
RFC 7807 errors with stable code/correlation ID, bounded pagination and whitelisted sorting.
Writes and sensitive reads require bearer authorization. Review actor comes from authenticated
claims, never a request body; role policies are enforced at the HTTP and application boundary.

Accounts checks customer existence through an infrastructure HTTP adapter. Activation requires
fresh customer/compliance eligibility evidence; no initial activation endpoint bypasses this.
Fraud creation checks immutable simulator transaction facts through an infrastructure adapter.
No cross-context project references or distributed transaction/ledger claims are introduced.

## Persistence, events and concurrency

Six EF DbContexts own the customers, accounts, transactions, fraudinvestigation, compliance, and credit
SQL schemas in the same BankSphereAI_Development database, with separate migration histories.
See [migration execution and verification](migrations-2026-11-16.md).
They can move to distinct databases without domain changes. Migrations are explicit per context;
startup does not silently EnsureCreated/drop/reseed production databases. Integration tests use
an isolated database name and SQL Server, not EF's InMemory provider.
Root updates compare Version; one SaveChanges atomically commits state/history and outbox rows.
CustomerCreated, InvestigationOpened, HumanReviewRequested and credit lifecycle events are
domain events. TransactionCreated is a versioned integration contract with synthetic provenance.
Outbox records are durable but no dispatcher/delivery guarantee is implemented this phase.
Simulator ingestion retries use a unique database key plus immutable payload fingerprint:
same payload returns existing identity, different payload returns conflict. Active fraud case
dedup uses transaction+normalized category; resolved/closed cases release the key, reopened
cases must reacquire it through the same unique index. SQL violations translate to conflicts.

## Privacy and scope

Use synthetic identities only in examples/tests. Identity/evidence references point to protected
external storage; never accept raw documents. No PII appears in URLs/log templates or event
payloads beyond necessary UUID references. Production needs TLS, encryption at rest and key
management, access auditing, jurisdiction-specific retention/deletion policies, least-privilege
database identities, legal review and verified screening integrations. No automatic regulatory
filing, legally compliant screening claims, or real-money ledger functionality is provided.

## Identity boundary

Identity users are separate from banking Customer aggregates. The optional CustomerId is an explicit opaque reference, not aggregate inheritance or a cross-schema entity navigation. See [Identity domain and infrastructure models](identity.md).
