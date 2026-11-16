using BankSphere.SharedKernel;

namespace BankSphere.Customers.Domain;

public sealed record CustomerCreated(Guid CustomerId, Guid EventId, DateTimeOffset OccurredAtUtc) : IDomainEvent;
