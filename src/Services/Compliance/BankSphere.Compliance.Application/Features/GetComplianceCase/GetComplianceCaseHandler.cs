using BankSphere.SharedKernel;

namespace BankSphere.Compliance.Application;

public sealed class GetComplianceCaseHandler(IComplianceRepository repository)
{
    public async Task<ComplianceDetails> Handle(Guid id, CancellationToken cancellationToken) => ComplianceDetails.From(
        await repository.FindAsync(Guard.Id(id, "Compliance case"), cancellationToken) ?? throw new BusinessException("compliance.not_found", "Compliance case does not exist.", FailureKind.NotFound));
}
