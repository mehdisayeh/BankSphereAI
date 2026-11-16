
namespace BankSphere.Customers.Application;

public sealed record AddressDetails(string Type, string Street, string City, string? Region, string PostalCode, string Country, DateTimeOffset FromUtc, DateTimeOffset? ToUtc);
