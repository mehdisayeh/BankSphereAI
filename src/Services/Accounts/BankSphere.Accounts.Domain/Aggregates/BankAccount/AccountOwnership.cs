
namespace BankSphere.Accounts.Domain;

public sealed record AccountOwnership(Guid Id, Guid CustomerId, OwnershipType Type, DateTimeOffset StartDateUtc, DateTimeOffset? EndDateUtc);
