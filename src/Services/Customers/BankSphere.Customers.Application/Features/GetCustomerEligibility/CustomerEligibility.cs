
namespace BankSphere.Customers.Application;

public sealed record CustomerEligibility(Guid Id, bool Exists, bool Eligible, long Version);
