using BankSphere.Accounts.Domain;
using BankSphere.SharedKernel;

namespace BankSphere.Accounts.Application;

public sealed class OpenAccountHandler(IAccountRepository repository, ICustomerEligibilityGateway customers, TimeProvider clock)
{
    public async Task<AccountDetails> Handle(OpenAccount command, RequestActor actor, CancellationToken cancellationToken)
    {
        actor.Require("banking-operator"); Guard.Id(command.CustomerId, "Customer");
        Guard.Against(!await customers.ExistsAsync(command.CustomerId, cancellationToken), "account.customer_not_found", "Customer does not exist.", FailureKind.NotFound);
        var currency = new Currency(command.Currency);
        var account = BankAccount.OpenAccount(command.CustomerId, command.Type, command.ProductCode, command.ProductName, currency,
            command.BranchId, command.Purpose, new(command.DailyTransactionLimit, currency), new(command.MonthlyTransactionLimit, currency), actor.Subject, clock.GetUtcNow());
        await repository.AddAsync(account, cancellationToken); return AccountDetails.From(account);
    }
}
