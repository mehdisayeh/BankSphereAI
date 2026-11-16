Subsequent Identity work added the identity schema (16 additional tables). See [Identity](identity.md) and the current [verification](verification.md). The six-service migration report below is retained.

# SQL Server migration execution ? 2026-11-16

All six contexts were migrated successfully using their existing design-time factories and the existing SQL Server connection configuration: `Server=.\SQLEXPRESS;Database=BankSphereAI_Development;Integrated Security=true;TrustServerCertificate=true`.

The factory connection override `BANKSPHERE_SQL_CONNECTION` was absent. A sysadmin catalog query before execution returned `DB_ID(...) = NULL`: the requested database did not exist on this instance. EF provisioned that named database during the first update. No existing database or table was dropped or recreated; there was no pre-existing development data in this target. Other application databases were not modified.

## Applied status

Every context has `HasDefaultSchema()` and a separate schema-qualified `__EFMigrationsHistory`. Each history contains its original migration plus one new forward migration. The 12 original migration/designer files remain unchanged; the six snapshots were updated by EF.

| Microservice / context | Schema | Applied migrations | Model tables | History tables | Indexes and keys | Foreign keys | Check constraints |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: |
| Customers / CustomersDbContext | `customers` | `20261113155415_InitialCustomerProfiles`<br>`20261116094848_SetCustomersDefaultSchema` | 14 | 1 | 25 | 12 | 2 |
| Accounts / AccountsDbContext | `accounts` | `20261113155539_InitialAccountsAndOwnership`<br>`20261116094852_AddBalanceSnapshotsAndDefaultSchema` | 7 | 1 | 15 | 5 | 1 |
| Transactions / TransactionsDbContext | `transactions` | `20261113155543_InitialSyntheticTransactionsAndOutbox`<br>`20261116094855_SetTransactionsDefaultSchema` | 9 | 1 | 16 | 7 | 2 |
| FraudInvestigation / FraudInvestigationDbContext | `fraudinvestigation` | `20261113155547_InitialInvestigationCasesAndActiveDeduplication`<br>`20261116094859_SetFraudDefaultSchema` | 10 | 1 | 22 | 8 | 1 |
| Compliance / ComplianceDbContext | `compliance` | `20261113155551_InitialComplianceReviewCases`<br>`20261116094902_SetComplianceDefaultSchema` | 8 | 1 | 17 | 6 | 1 |
| CreditAssessment / CreditAssessmentDbContext | `credit` | `20261113155556_InitialCreditApplicationsAndAssessments`<br>`20261116094905_MoveToCreditSchema` | 13 | 1 | 26 | 11 | 1 |

Totals: **61 model tables + 6 history tables = 67 tables**, 121 model indexes/keys, 49 foreign keys, and 8 check constraints. Index/key totals exclude history-table primary keys.

## Created tables by schema

All names below are qualified by the schema in the first column. Each schema also has its own `__EFMigrationsHistory`.

| Schema | Created model tables |
| --- | --- |
| `customers` | `CustomerAddresses`, `CustomerBeneficialOwners`, `CustomerConsents`, `CustomerContacts`, `CustomerCorporate`, `CustomerCountries`, `CustomerFinancial`, `CustomerIdentityDocuments`, `CustomerIndividual`, `CustomerRelationships`, `CustomerRiskFactors`, `CustomerRiskProfiles`, `Customers`, `OutboxMessages` |
| `accounts` | `Accounts`, `BalanceSnapshots`, `BankAccountMandates`, `BankAccountOwnerships`, `BankAccountRestrictions`, `BankAccountStatusHistory`, `OutboxMessages` |
| `transactions` | `BankingTransactionCounterparty`, `BankingTransactionEnrichments`, `BankingTransactionInstrument`, `BankingTransactionMerchant`, `BankingTransactionMetadata`, `BankingTransactionRiskSignals`, `BankingTransactionStatusHistory`, `OutboxMessages`, `Transactions` |
| `fraudinvestigation` | `Cases`, `FraudCaseAssignments`, `FraudCaseDecisions`, `FraudCaseEvidence`, `FraudCaseHumanReviews`, `FraudCaseIndicators`, `FraudCaseNotes`, `FraudCaseStatusHistory`, `FraudCaseTransactionLinks`, `OutboxMessages` |
| `compliance` | `Cases`, `ComplianceCaseDocuments`, `ComplianceCaseDueDiligence`, `ComplianceCaseKycReviews`, `ComplianceCaseReviewHistory`, `ComplianceCaseRiskFactors`, `ComplianceCaseScreenings`, `OutboxMessages` |
| `credit` | `Applications`, `CreditApplicationAssessments`, `CreditApplicationCollateral`, `CreditApplicationDecisionHistory`, `CreditApplicationDocuments`, `CreditApplicationExistingDebts`, `CreditApplicationFinancial`, `CreditApplicationIncomeSources`, `CreditApplicationObligations`, `CreditApplicationRecommendations`, `CreditApplicationReviewRequests`, `CreditAssessmentRiskFactors`, `OutboxMessages` |

## Migration changes

- Customers, Transactions, FraudInvestigation, and Compliance: metadata-only migrations record the new default-schema annotation; explicitly mapped physical tables already used those schemas.
- Accounts: adds the previously missing `accounts.BalanceSnapshots` table, its primary key, decimal/currency mappings, and restricted-delete foreign key to Accounts. This resolves the earlier pending-model failure.
- Credit: preserves its original migration in `creditassessment`, then moves all 13 tables to `credit` using schema transfers (`RenameTable` / `ALTER SCHEMA TRANSFER`), without copying or recreating them. Its factory, runtime registration, mappings, and test history now use `credit`. The empty legacy schema is retained.
- Existing connection strings, domain logic, API routes/contracts, repository behavior, and project dependencies were not changed. No pending-model warning was suppressed.

## Execution and verification

Executed `dotnet tool restore` using dotnet-ef 9.0.23 and `dotnet build BankSphereAI.slnx -v quiet`. For each context, generated/reviewed its migration and forward SQL, checked `dotnet ef migrations has-pending-model-changes`, and executed:

```powershell
$contexts = "Customers", "Accounts", "Transactions", "FraudInvestigation", "Compliance", "CreditAssessment"
foreach ($context in $contexts) {
    dotnet ef database update --context "${context}DbContext" `
        --project "src/Services/$context/BankSphere.$context.Infrastructure" --no-build
    if ($LASTEXITCODE -ne 0) { throw "Migration failed for $context" }
}
```

All six updates exited successfully and applied both migrations. All six pending-model checks reported no changes. Reviewed forward SQL contains no `DROP TABLE`, `DROP COLUMN`, or `TRUNCATE TABLE` statements.

The development database was then compared directly against each EF design-time relational model: exact schema/table sets (including history), column names/store types/nullability, primary and unique keys, ordered index columns and filters, foreign key columns/principals/delete actions, and check-constraint definitions. All checks passed; no indexes or constraints were disabled or untrusted. `DBCC CHECKCONSTRAINTS WITH ALL_CONSTRAINTS` reported no violations. All development model tables were empty after provisioning; integration data was confined to the test fixture?s separate temporary database.

The reusable catalog verifier is `tests/Integration/BankSphere.Persistence.Tests/SqlSchemaVerifier.cs`; the added integration test verifies all six schemas and histories on the isolated SQL test database. Execution logs, generated SQL, and the development catalog report are retained locally in `.local/migrations-20261116/`.

## Build and tests

- `dotnet build BankSphereAI.slnx -v quiet`: passed, 0 warnings, 0 errors.
- `dotnet test BankSphereAI.slnx --no-build -v minimal`: **43 passed, 0 failed, 0 skipped**.
- Foundation: 6 passed. Banking unit/API contract: 31 passed. SQL persistence: 6 passed, including all five previously failing tests and the new catalog/model regression test.
- AppHost runtime startup was not part of this database migration verification.
