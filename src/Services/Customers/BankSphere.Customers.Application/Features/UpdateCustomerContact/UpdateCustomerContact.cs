
namespace BankSphere.Customers.Application;

public sealed record UpdateCustomerContact(string Email, string? Phone, long ExpectedVersion);
