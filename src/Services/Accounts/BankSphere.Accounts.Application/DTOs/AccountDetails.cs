using BankSphere.Accounts.Domain;

namespace BankSphere.Accounts.Application;

public sealed record AccountDetails(Guid Id, string AccountNumber, string? Iban, string? Bic, AccountType Type, AccountStatus Status,
    string ProductCode, string ProductName, string Currency, Guid PrimaryOwnerId, string BranchId, string Purpose,
    decimal DailyTransactionLimit, decimal MonthlyTransactionLimit, DateTimeOffset OpenedAtUtc, DateTimeOffset? ActivatedAtUtc,
    DateTimeOffset? ClosedAtUtc, long Version, IReadOnlyList<OwnerDetails> Owners)
{
    public static AccountDetails From(BankAccount account) => new(account.Id, account.AccountNumber, account.Iban?.Value, account.Bic,
        account.Type, account.Status, account.ProductCode, account.ProductName, account.Currency.Code, account.PrimaryOwnerId,
        account.BranchId, account.Purpose, account.DailyTransactionLimit.Amount, account.MonthlyTransactionLimit.Amount,
        account.OpeningDateUtc, account.ActivationDateUtc, account.ClosingDateUtc, account.Version,
        account.Ownerships.Select(owner => new OwnerDetails(owner.CustomerId, owner.Type, owner.StartDateUtc, owner.EndDateUtc)).ToArray());
}
