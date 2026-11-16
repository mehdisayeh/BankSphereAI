using BankSphere.Accounts.Domain;

namespace BankSphere.Accounts.Application;

public sealed record OwnerDetails(Guid CustomerId, OwnershipType Type, DateTimeOffset FromUtc, DateTimeOffset? ToUtc);
