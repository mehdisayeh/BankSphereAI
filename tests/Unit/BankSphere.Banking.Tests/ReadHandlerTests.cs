using BankSphere.Customers.Application;
using BankSphere.Customers.Domain;
using BankSphere.SharedKernel;
using BankSphere.Transactions.Application;
using BankSphere.Transactions.Domain;
using Xunit;

namespace BankSphere.Banking.Tests;

public sealed class ReadHandlerTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task EligibilityPreservesMissingResultsAndForwardsTheClockAndCancellation(bool exists)
    {
        var id = Guid.NewGuid();
        var now = new DateTimeOffset(2026, 11, 14, 10, 0, 0, TimeSpan.Zero);
        using var cancellation = new CancellationTokenSource();
        var expected = exists ? new CustomerEligibility(id, true, false, 7) : null;
        var repository = new CustomerReader(expected);

        var actual = await new GetCustomerEligibilityHandler(repository, new FixedClock(now)).Handle(id, cancellation.Token);

        Assert.Same(expected, actual);
        Assert.Equal((id, now, cancellation.Token), repository.Received);
    }

    [Fact]
    public async Task TransactionReadPreservesTheExistingResponse()
    {
        var expected = new TransactionDetails(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "external", TransactionType.Deposit,
            TransactionStatus.Pending, TransactionDirection.Credit, 20, "EUR", DateTimeOffset.UtcNow, "simulator", "batch", "scenario", true);
        using var cancellation = new CancellationTokenSource();
        var repository = new TransactionReader(expected);

        Assert.Same(expected, await new GetTransactionByIdHandler(repository).Handle(expected.Id, cancellation.Token));
        Assert.Equal((expected.Id, cancellation.Token), repository.Received);
    }

    [Fact]
    public async Task MissingTransactionPreservesItsProblemDetailsError()
    {
        var handler = new GetTransactionByIdHandler(new TransactionReader(null));
        var error = await Assert.ThrowsAsync<BusinessException>(() => handler.Handle(Guid.Empty, CancellationToken.None));

        Assert.Equal("transaction.not_found", error.Code);
        Assert.Equal("Transaction does not exist.", error.Message);
        Assert.Equal(FailureKind.NotFound, error.Kind);
    }

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class CustomerReader(CustomerEligibility? result) : ICustomerRepository
    {
        public (Guid, DateTimeOffset, CancellationToken) Received { get; private set; }
        public Task<CustomerEligibility?> EligibilityAsync(Guid id, DateTimeOffset now, CancellationToken cancellationToken)
        {
            Received = (id, now, cancellationToken);
            return Task.FromResult(result);
        }
        public Task AddAsync(Customer customer, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<Customer?> FindAsync(Guid id, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task SaveAsync(CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    private sealed class TransactionReader(TransactionDetails? result) : ITransactionRepository
    {
        public (Guid, CancellationToken) Received { get; private set; }
        public Task<TransactionDetails?> FindAsync(Guid id, CancellationToken cancellationToken)
        {
            Received = (id, cancellationToken);
            return Task.FromResult(result);
        }
        public Task<BankingTransaction?> FindByExternalReferenceAsync(string source, string reference, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task AddAsync(BankingTransaction transaction, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<Page<TransactionDetails>> ByAccountAsync(TransactionFilter filter, CancellationToken cancellationToken) => throw new NotSupportedException();
    }
}
