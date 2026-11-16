using BankSphere.Compliance.Domain;

namespace BankSphere.Compliance.Application;

public interface IComplianceRepository
{
    Task AddAsync(ComplianceCase complianceCase, CancellationToken cancellationToken);
    Task<ComplianceCase?> FindAsync(Guid id, CancellationToken cancellationToken);
}
