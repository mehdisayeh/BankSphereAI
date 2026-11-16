using BankSphere.Accounts.Domain;

namespace BankSphere.Accounts.Application;

public sealed record OpenAccount(Guid CustomerId, AccountType Type, string ProductCode, string ProductName, string Currency,
    string BranchId, string Purpose, decimal DailyTransactionLimit, decimal MonthlyTransactionLimit);
