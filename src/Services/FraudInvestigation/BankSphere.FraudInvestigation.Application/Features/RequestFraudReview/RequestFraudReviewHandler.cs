using BankSphere.SharedKernel;

namespace BankSphere.FraudInvestigation.Application;

public sealed class RequestFraudReviewHandler(IFraudCaseRepository repository, TimeProvider clock)
{
    public async Task<FraudDetails> Handle(Guid id, RequestFraudReview command, RequestActor actor, CancellationToken cancellationToken)
    {
        actor.Require("banking-operator");
        var fraudCase = await repository.FindAsync(id, cancellationToken) ?? throw new BusinessException("fraud.not_found", "Investigation does not exist.", FailureKind.NotFound);
        Guard.Against(fraudCase.Version != command.ExpectedVersion, "fraud.version", "Investigation changed; reload and retry.", FailureKind.Conflict);
        fraudCase.RequestHumanReview(command.Reviewer, command.Reason, actor.Subject, clock.GetUtcNow());
        await repository.SaveAsync(cancellationToken); return FraudDetails.From(fraudCase);
    }
}
