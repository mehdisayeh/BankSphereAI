
namespace BankSphere.Accounts.Domain;

public sealed record AccountStatusHistory(Guid Id, AccountStatus PreviousStatus, AccountStatus NewStatus, string Reason, string ChangedBy, DateTimeOffset ChangedAtUtc);
