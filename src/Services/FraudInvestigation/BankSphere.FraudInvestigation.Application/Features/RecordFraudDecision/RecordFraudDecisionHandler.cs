using BankSphere.SharedKernel;

namespace BankSphere.FraudInvestigation.Application;

public sealed class RecordFraudDecisionHandler(IFraudCaseRepository repository, TimeProvider clock)
{
    public async Task<FraudDetails> Handle(Guid id, RecordFraudDecision command, RequestActor actor, CancellationToken cancellationToken)
    {
        actor.Require("fraud-reviewer");
        var fraudCase = await repository.FindAsync(id, cancellationToken) ?? throw new BusinessException("fraud.not_found", "Investigation does not exist.", FailureKind.NotFound);
        Guard.Against(fraudCase.Version != command.ExpectedVersion, "fraud.version", "Investigation changed.", FailureKind.Conflict);
        fraudCase.RecordHumanDecision(command.Classification, command.Rationale, actor.Subject, command.EvidenceReference, clock.GetUtcNow());
        await repository.SaveAsync(cancellationToken); return FraudDetails.From(fraudCase);
    }
}
