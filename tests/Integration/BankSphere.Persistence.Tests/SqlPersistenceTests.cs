using BankSphere.Accounts.Domain;
using BankSphere.Accounts.Infrastructure;
using BankSphere.Compliance.Domain;
using BankSphere.Compliance.Infrastructure;
using BankSphere.CreditAssessment.Domain;
using BankSphere.CreditAssessment.Infrastructure;
using BankSphere.Customers.Domain;
using BankSphere.Customers.Infrastructure;
using BankSphere.FraudInvestigation.Domain;
using BankSphere.FraudInvestigation.Infrastructure;
using BankSphere.Persistence;
using BankSphere.SharedKernel;
using BankSphere.Transactions.Application;
using BankSphere.Transactions.Domain;
using BankSphere.Transactions.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BankSphere.Persistence.Tests;

public sealed class SqlPersistenceTests(SqlDatabase database) : IClassFixture<SqlDatabase>
{
    [Fact]
    public async Task AllSchemasMatchTheirRelationalModelsAndMigrationHistories()
    {
        (DbContext Context, string Schema)[] contexts =
        [
            (database.Customers(), "customers"), (database.Accounts(), "accounts"),
            (database.Transactions(), "transactions"), (database.Fraud(), "fraudinvestigation"),
            (database.Compliance(), "compliance"), (database.Credit(), "credit")
        ];
        try
        {
            foreach (var (context, schema) in contexts) await SqlSchemaVerifier.VerifyAsync(context, schema);
        }
        finally
        {
            foreach (var (context, _) in contexts) await context.DisposeAsync();
        }
    }

    private static readonly DateTimeOffset Now = new(2026, 11, 13, 12, 0, 0, TimeSpan.Zero);
    private static Money Money(decimal amount) => new(amount, new("EUR"));
    private static Customer Customer()
    {
        var value = BankSphere.Customers.Domain.Customer.RegisterCustomer(CustomerType.Individual, CustomerSegment.Retail,
            new("Synthetic", "Customer", "Test", new(1990, 1, 1), "Paris", "FR", "FR"), null,
            new("Tester", EmploymentStatus.Employed, "Synthetic employer", null, "30000-50000", "Salary", "Savings", "Monthly salary"), "Test", "fr", "email", Now);
        value.UpdateAddress("residential", new("1 Synthetic Street", "Paris", null, "75001", "FR"), Now);
        value.UpdateContactInformation("email", "synthetic@example.invalid", Now);
        value.AddIdentityDocument("passport", "FR", new(2020, 1, 1), new(2030, 1, 1), "protected:synthetic-document", Now);
        value.RecordConsent("privacy", "v1", Now); value.AddCountry("tax", "FR", Now);
        value.UpdateRiskProfile(CustomerRiskProfile.Assess(CustomerRiskLevel.Low, "synthetic-v1", "Test risk",
            [new(Guid.NewGuid(), "residence", "Synthetic French residence")], Now, Now.AddYears(1)), Now);
        return value;
    }
    private static FraudCase Fraud(Guid transaction) => FraudCase.OpenInvestigation(Guid.NewGuid(), Guid.NewGuid(), transaction,
        "Card", "VELOCITY", FraudSeverity.High, 1, "simulator", Now, 90, "High", "Synthetic investigation", Money(20), Money(20), "operator", Now.AddDays(1), Now);

    [Fact]
    public async Task NormalizedCustomerProfileAndOutboxRoundTrip()
    {
        var value = Customer();
        await using (var context = database.Customers()) await new CustomerRepository(context).AddAsync(value, CancellationToken.None);
        Assert.Empty(value.DomainEvents);
        await using var reader = database.Customers();
        var loaded = await new CustomerRepository(reader).FindAsync(value.Id, CancellationToken.None);
        Assert.NotNull(loaded); Assert.Equal("Synthetic", loaded.Individual!.LegalFirstName);
        Assert.Single(loaded.Addresses); Assert.Single(loaded.IdentityDocuments); Assert.Single(Assert.Single(loaded.RiskProfiles).Factors);
        Assert.Contains(await reader.Set<OutboxMessage>().ToListAsync(), message => message.Payload.Contains(value.Id.ToString(), StringComparison.Ordinal));
    }

    [Fact]
    public async Task DatabaseRejectsConcurrentDuplicateActiveInvestigationsAndRollsBackOutbox()
    {
        var transaction = Guid.NewGuid();
        async Task<bool> Insert()
        {
            await using var context = database.Fraud();
            try { await new FraudCaseRepository(context).AddAsync(Fraud(transaction), CancellationToken.None); return true; }
            catch (BusinessException exception) when (exception.Code == "persistence.duplicate") { return false; }
        }
        var results = await Task.WhenAll(Insert(), Insert());
        Assert.Single(results, value => value);
        await using var reader = database.Fraud();
        var persisted = await reader.Cases.SingleAsync(value => value.TransactionId == transaction);
        Assert.Equal(1, await reader.Cases.CountAsync(value => value.TransactionId == transaction));
        var messages = await reader.Set<OutboxMessage>().ToListAsync();
        Assert.Single(messages, message => message.Payload.Contains(persisted.Id.ToString(), StringComparison.Ordinal));
    }

    [Fact]
    public async Task OptimisticConcurrencyRejectsStaleAggregateUpdates()
    {
        var value = Customer();
        await using (var writer = database.Customers()) await new CustomerRepository(writer).AddAsync(value, CancellationToken.None);
        await using var first = database.Customers(); await using var second = database.Customers();
        var a = (await new CustomerRepository(first).FindAsync(value.Id, CancellationToken.None))!;
        var b = (await new CustomerRepository(second).FindAsync(value.Id, CancellationToken.None))!;
        a.UpdateContactInformation("email", "first@example.invalid", Now); b.UpdateContactInformation("email", "second@example.invalid", Now);
        await first.SaveChangesAsync();
        var exception = await Assert.ThrowsAsync<BusinessException>(() => second.SaveChangesAsync());
        Assert.Equal("persistence.concurrency", exception.Code);
    }

    [Fact]
    public async Task SimulatorReplayIsIdempotentAndConflictingReplayFails()
    {
        await using var context = database.Transactions();
        var record = new SimulatorRecord("test-simulator", Guid.NewGuid().ToString("N"), Guid.NewGuid(), Guid.NewGuid(), TransactionType.CardPayment,
            TransactionStatus.Booked, TransactionDirection.Debit, Money(20), Money(20), null, Now, Now, new(2026, 11, 13),
            new("Synthetic merchant", "5411", "FR", "Paris"), null, new("token", "protected:instrument"), "Web", "Card", "Synthetic",
            "Synthetic payment", null, null, "protected:device", null, null, "Test", "test-batch", "velocity");
        var handler = new IngestSimulationHandler(new TransactionRepository(context), new TestAccountGateway(), new FixedClock());
        var actor = new RequestActor("simulator", new HashSet<string>(StringComparer.Ordinal) { "simulator" });
        var command = new IngestSimulation(record, [], [], []);
        var first = await handler.Handle(command, actor, CancellationToken.None);
        var replay = await handler.Handle(command, actor, CancellationToken.None);
        Assert.False(first.Replayed); Assert.True(replay.Replayed); Assert.Equal(first.Transaction.Id, replay.Transaction.Id);
        var exception = await Assert.ThrowsAsync<BusinessException>(() => handler.Handle(command with { Record = record with { Description = "Different payload" } }, actor, CancellationToken.None));
        Assert.Equal("simulation.idempotency_conflict", exception.Code);
        Assert.Equal(1, await context.Transactions.CountAsync(value => value.ExternalTransactionReference == record.ExternalReference));
        Assert.Single(await context.Set<OutboxMessage>().Where(message => message.Type == "TransactionCreated.v1").ToListAsync());
        var page = await new GetTransactionsByAccountHandler(new TransactionRepository(context)).Handle(new(record.AccountId, new(1, 1, "amount")), CancellationToken.None);
        Assert.Equal(1, page.TotalCount); Assert.Single(page.Items);
    }

    [Fact]
    public async Task AccountComplianceAndCreditChildCollectionsPersist()
    {
        var account = BankAccount.OpenAccount(Guid.NewGuid(), AccountType.Current, "TEST", "Synthetic", new("EUR"), "TEST", "Testing", Money(100), Money(1000), "operator", Now);
        account.AddMandate(Guid.NewGuid(), "View", "read", Now.AddYears(1), Now); account.AddRestriction("review", "Synthetic review", null, Now);
        await using (var context = database.Accounts()) await new AccountRepository(context).AddAsync(account, CancellationToken.None);
        await using (var reader = database.Accounts()) Assert.Single((await new AccountRepository(reader).FindAsync(account.Id, CancellationToken.None))!.Mandates);

        var compliance = ComplianceCase.Open(Guid.NewGuid(), ComplianceCaseType.PeriodicKyc, "FR", "Periodic", "synthetic", Now.AddDays(30), Now);
        compliance.BeginReview("officer", Now); compliance.RecordKycReview("officer", "Synthetic findings", "protected:kyc", Now.AddYears(1), Now);
        compliance.RecordDueDiligence(true, "Review", "Synthetic funds", "officer", "protected:kyc", Now);
        compliance.RecordScreening("pep", "synthetic", "protected:pep", false, "Synthetic no-match", "officer", Now);
        compliance.AttachDocument("kyc", "protected:kyc", Now); compliance.AssessRisk(RiskClassification.Low, "source", "Reviewed", "officer", Now);
        compliance.RecordDecision(true, "officer", "Synthetic evidence reviewed", Now);
        await using (var context = database.Compliance()) await new ComplianceRepository(context).AddAsync(compliance, CancellationToken.None);
        await using (var reader = database.Compliance()) Assert.Single((await new ComplianceRepository(reader).FindAsync(compliance.Id, CancellationToken.None))!.Screenings);

        var credit = CreditApplication.CreateDraftApplication(Guid.NewGuid(), "TEST", "Synthetic", Money(1000), 12, "Test",
            new("Employed", Money(3000), Money(500), Money(0), Money(600)), Now);
        credit.AddIncomeSource("salary", Money(3000), "protected:salary", Now); credit.AttachDocument("salary", "protected:salary", Now); credit.SubmitApplication(Now);
        await using (var context = database.Credit()) await new CreditApplicationRepository(context).AddAsync(credit, CancellationToken.None);
        await using var creditReader = database.Credit();
        Assert.Equal(CreditApplicationStatus.Submitted, (await new CreditApplicationRepository(creditReader).StatusAsync(credit.Id, CancellationToken.None))!.Status);
        var loadedCredit = await creditReader.Applications.AsSplitQuery().SingleAsync(value => value.Id == credit.Id);
        Assert.Single(loadedCredit.IncomeSources); Assert.Equal(3000m, loadedCredit.Financial.DeclaredMonthlyIncome.Amount);
    }

    private sealed class FixedClock : TimeProvider { public override DateTimeOffset GetUtcNow() => Now; }
    private sealed class TestAccountGateway : ISimulatorAccountGateway
    {
        public Task<bool> IsOwnedByAsync(Guid account, Guid customer, CancellationToken cancellationToken) => Task.FromResult(true);
    }
}
