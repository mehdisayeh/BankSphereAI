using BankSphere.CreditAssessment.Application;
using BankSphere.CreditAssessment.Domain;
using Microsoft.EntityFrameworkCore;

namespace BankSphere.CreditAssessment.Infrastructure;

public sealed class CreditApplicationRepository(CreditAssessmentDbContext context) : ICreditApplicationRepository
{
    public async Task AddAsync(CreditApplication application, CancellationToken cancellationToken)
    {
        context.Applications.Add(application);
        await context.SaveChangesAsync(cancellationToken);
    }
    public Task<CreditStatus?> StatusAsync(Guid id, CancellationToken cancellationToken) =>
        context.Applications.AsNoTracking().Where(value => value.Id == id).Select(value => new CreditStatus(value.Id, value.ApplicationNumber,
            value.CustomerId, value.Status, value.LoanProductId, value.LoanPurpose, value.RequestedAmount.Amount, value.RequestedAmount.Currency.Code,
            value.RequestedTermMonths, value.SubmittedAtUtc, value.ReviewedAtUtc, value.AssignedCreditOfficerId, value.Version)).SingleOrDefaultAsync(cancellationToken);
}
