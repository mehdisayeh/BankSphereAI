using BankSphere.Compliance.Domain;
using BankSphere.SharedKernel;

namespace BankSphere.Compliance.Application;

public sealed class OpenComplianceCaseHandler(IComplianceRepository repository, TimeProvider clock)
{
    public async Task<ComplianceDetails> Handle(OpenComplianceCase command, RequestActor actor, CancellationToken cancellationToken)
    {
        actor.Require("compliance-officer");
        var value = ComplianceCase.Open(command.CustomerId, command.Type, command.Jurisdiction, command.Reason, command.ScreeningSource, command.DueAtUtc, clock.GetUtcNow());
        await repository.AddAsync(value, cancellationToken); return ComplianceDetails.From(value);
    }
}
