using BankSphere.Customers.Application;
using BankSphere.Customers.Domain;
using Microsoft.EntityFrameworkCore;

namespace BankSphere.Customers.Infrastructure;

public sealed class CustomerRepository(CustomersDbContext context) : ICustomerRepository
{
    public async Task AddAsync(Customer customer, CancellationToken cancellationToken)
    {
        context.Customers.Add(customer);
        await context.SaveChangesAsync(cancellationToken);
    }
    public Task<Customer?> FindAsync(Guid id, CancellationToken cancellationToken) =>
        context.Customers.AsSplitQuery().SingleOrDefaultAsync(value => value.Id == id, cancellationToken);
    public async Task SaveAsync(CancellationToken cancellationToken) => await context.SaveChangesAsync(cancellationToken);
    public Task<CustomerEligibility?> EligibilityAsync(Guid id, DateTimeOffset now, CancellationToken cancellationToken) =>
        context.Customers.AsNoTracking().Where(value => value.Id == id)
            .Select(value => new CustomerEligibility(value.Id, true, value.Status == CustomerStatus.Active
                && value.KycStatus == KycStatus.Verified && value.KycNextReviewAtUtc > now, value.Version))
            .SingleOrDefaultAsync(cancellationToken);
}
