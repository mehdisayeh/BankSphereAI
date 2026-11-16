
namespace BankSphere.Transactions.Application;

public interface ISimulatorAccountGateway
{
    Task<bool> IsOwnedByAsync(Guid account, Guid customer, CancellationToken cancellationToken);
}
