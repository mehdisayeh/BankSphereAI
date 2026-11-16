using BankSphere.Compliance.Application;
using BankSphere.Compliance.Domain;
using Microsoft.EntityFrameworkCore;

namespace BankSphere.Compliance.Infrastructure;

public sealed class ComplianceRepository(ComplianceDbContext context) : IComplianceRepository
{
    public async Task AddAsync(ComplianceCase complianceCase, CancellationToken cancellationToken)
    {
        context.Cases.Add(complianceCase);
        await context.SaveChangesAsync(cancellationToken);
    }
    public Task<ComplianceCase?> FindAsync(Guid id, CancellationToken cancellationToken) =>
        context.Cases.AsSplitQuery().SingleOrDefaultAsync(value => value.Id == id, cancellationToken);
}
