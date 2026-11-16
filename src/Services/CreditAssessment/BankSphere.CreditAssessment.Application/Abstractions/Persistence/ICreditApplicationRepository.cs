using BankSphere.CreditAssessment.Domain;

namespace BankSphere.CreditAssessment.Application;

public interface ICreditApplicationRepository
{
    Task AddAsync(CreditApplication application, CancellationToken cancellationToken);
    Task<CreditStatus?> StatusAsync(Guid id, CancellationToken cancellationToken);
}
