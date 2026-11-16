
namespace BankSphere.Accounts.Application;

public interface ICustomerEligibilityGateway
{
    Task<bool> ExistsAsync(Guid customer, CancellationToken cancellationToken);
}
