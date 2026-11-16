namespace BankSphere.Customers.Application;

public sealed class GetCustomerEligibilityHandler(ICustomerRepository repository, TimeProvider clock)
{
    public Task<CustomerEligibility?> Handle(Guid id, CancellationToken cancellationToken) =>
        repository.EligibilityAsync(id, clock.GetUtcNow(), cancellationToken);
}
