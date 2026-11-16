using BankSphere.SharedKernel;

namespace BankSphere.Customers.Application;

public sealed class UpdateCustomerContactHandler(ICustomerRepository repository, TimeProvider clock)
{
    public async Task<CustomerDetails> Handle(Guid id, UpdateCustomerContact command, CancellationToken cancellationToken)
    {
        var customer = await repository.FindAsync(id, cancellationToken) ?? throw new BusinessException("customer.not_found", "Customer does not exist.", FailureKind.NotFound);
        Guard.Against(customer.Version != command.ExpectedVersion, "customer.version", "Customer changed; reload and retry.", FailureKind.Conflict);
        customer.UpdateContactInformation("email", command.Email, clock.GetUtcNow());
        if (command.Phone is not null) customer.UpdateContactInformation("phone", command.Phone, clock.GetUtcNow());
        await repository.SaveAsync(cancellationToken); return CustomerDetails.From(customer);
    }
}
