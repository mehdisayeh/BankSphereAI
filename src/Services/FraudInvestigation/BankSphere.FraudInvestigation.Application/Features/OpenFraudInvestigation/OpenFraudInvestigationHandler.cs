using BankSphere.FraudInvestigation.Domain;
using BankSphere.SharedKernel;

namespace BankSphere.FraudInvestigation.Application;

public sealed class OpenFraudInvestigationHandler(IFraudCaseRepository repository, IInvestigationTransactions transactions, TimeProvider clock)
{
    public async Task<FraudDetails> Handle(OpenFraudInvestigation command, RequestActor actor, CancellationToken cancellationToken)
    {
        actor.Require("banking-operator");
        var facts = await transactions.FindAsync(Guard.Id(command.TransactionId, "Transaction"), cancellationToken)
            ?? throw new BusinessException("fraud.transaction_not_found", "Transaction does not exist.", FailureKind.NotFound);
        Guard.Against(!facts.IsSynthetic, "fraud.provenance", "This phase accepts only simulator transactions.");
        var category = Guard.Text(command.Category, "Category", 80).ToUpperInvariant();
        Guard.Against(await repository.HasActiveAsync(command.TransactionId, category, cancellationToken), "fraud.duplicate_active", "An active investigation already exists for this transaction and category.", FailureKind.Conflict);
        var currency = new Currency(facts.Currency);
        Guard.Against(command.SuspectedFraudAmount > facts.Amount, "fraud.amount", "Suspected amount cannot exceed the linked transaction.");
        var fraudCase = FraudCase.OpenInvestigation(facts.CustomerId, facts.AccountId, facts.Id, command.CaseType, category, command.Severity,
            command.Priority, command.DetectionSource, command.DetectedAtUtc, command.RiskScore, command.RiskLevel, command.Description,
            new(command.SuspectedFraudAmount, currency), new(command.EstimatedFinancialExposure, currency), actor.Subject, command.ReviewDueAtUtc, clock.GetUtcNow());
        await repository.AddAsync(fraudCase, cancellationToken); return FraudDetails.From(fraudCase);
    }
}
