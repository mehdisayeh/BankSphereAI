using BankSphere.FraudInvestigation.Domain;

namespace BankSphere.FraudInvestigation.Application;

public interface IFraudCaseRepository
{
    Task AddAsync(FraudCase fraudCase, CancellationToken cancellationToken);
    Task<FraudCase?> FindAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> HasActiveAsync(Guid transaction, string category, CancellationToken cancellationToken);
    Task SaveAsync(CancellationToken cancellationToken);
}
