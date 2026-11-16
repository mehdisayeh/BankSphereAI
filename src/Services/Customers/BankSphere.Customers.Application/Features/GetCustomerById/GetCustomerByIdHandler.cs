using BankSphere.SharedKernel;

namespace BankSphere.Customers.Application;

public sealed class GetCustomerByIdHandler(ICustomerRepository repository)
{
    public async Task<CustomerDetails> Handle(Guid id, CancellationToken cancellationToken) => CustomerDetails.From(
        await repository.FindAsync(Guard.Id(id, "Customer"), cancellationToken) ?? throw new BusinessException("customer.not_found", "Customer does not exist.", FailureKind.NotFound));
}
