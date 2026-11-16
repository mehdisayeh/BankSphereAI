using BankSphere.SharedKernel;

namespace BankSphere.FraudInvestigation.Application;

public sealed class FraudEvidenceHandler(IFraudCaseRepository repository, TimeProvider clock)
{
    public async Task<FraudDetails> Handle(Guid id, AttachFraudEvidence command, RequestActor actor, CancellationToken cancellationToken)
    {
        actor.Require("banking-operator");
        var fraudCase = await repository.FindAsync(id, cancellationToken) ?? throw new BusinessException("fraud.not_found", "Investigation does not exist.", FailureKind.NotFound);
        Guard.Against(fraudCase.Version != command.ExpectedVersion, "fraud.version", "Investigation changed.", FailureKind.Conflict);
        fraudCase.AttachEvidence(command.Type, command.Source, command.Description, command.Reference, command.Sha256, clock.GetUtcNow());
        await repository.SaveAsync(cancellationToken); return FraudDetails.From(fraudCase);
    }
}
