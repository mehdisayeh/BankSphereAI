using BankSphere.SharedKernel;

namespace BankSphere.CreditAssessment.Application;

public sealed class GetCreditApplicationStatusHandler(ICreditApplicationRepository repository)
{
    public async Task<CreditStatus> Handle(Guid id, CancellationToken cancellationToken) =>
        await repository.StatusAsync(Guard.Id(id, "Application"), cancellationToken) ?? throw new BusinessException("credit.not_found", "Application does not exist.", FailureKind.NotFound);
}
