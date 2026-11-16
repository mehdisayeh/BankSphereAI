using BankSphere.Customers.Domain;

namespace BankSphere.Customers.Application;

public interface ICustomerRepository
{
    Task AddAsync(Customer customer, CancellationToken cancellationToken);
    Task<Customer?> FindAsync(Guid id, CancellationToken cancellationToken);
    Task SaveAsync(CancellationToken cancellationToken);
    Task<CustomerEligibility?> EligibilityAsync(Guid id, DateTimeOffset now, CancellationToken cancellationToken);
}
