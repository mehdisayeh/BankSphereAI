using BankSphere.FraudInvestigation.Application;
using BankSphere.FraudInvestigation.Domain;
using Microsoft.EntityFrameworkCore;

namespace BankSphere.FraudInvestigation.Infrastructure;

public sealed class FraudCaseRepository(FraudInvestigationDbContext context) : IFraudCaseRepository
{
    public async Task AddAsync(FraudCase fraudCase, CancellationToken cancellationToken)
    {
        context.Cases.Add(fraudCase);
        await context.SaveChangesAsync(cancellationToken);
    }
    public Task<FraudCase?> FindAsync(Guid id, CancellationToken cancellationToken) =>
        context.Cases.AsSplitQuery().SingleOrDefaultAsync(value => value.Id == id, cancellationToken);
    public async Task SaveAsync(CancellationToken cancellationToken) => await context.SaveChangesAsync(cancellationToken);
    public Task<bool> HasActiveAsync(Guid transaction, string category, CancellationToken cancellationToken) =>
        context.Cases.AnyAsync(value => value.TransactionId == transaction && value.Category == category && value.IsActive, cancellationToken);
}
