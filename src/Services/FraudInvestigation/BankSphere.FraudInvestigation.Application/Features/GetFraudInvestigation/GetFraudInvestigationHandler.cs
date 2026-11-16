using BankSphere.SharedKernel;

namespace BankSphere.FraudInvestigation.Application;

public sealed class GetFraudInvestigationHandler(IFraudCaseRepository repository)
{
    public async Task<FraudDetails> Handle(Guid id, CancellationToken cancellationToken) => FraudDetails.From(
        await repository.FindAsync(Guard.Id(id, "Investigation"), cancellationToken) ?? throw new BusinessException("fraud.not_found", "Investigation does not exist.", FailureKind.NotFound));
}
