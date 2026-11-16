using BankSphere.CreditAssessment.Domain;
using BankSphere.SharedKernel;

namespace BankSphere.CreditAssessment.Application;

public sealed class SubmitCreditApplicationHandler(ICreditApplicationRepository repository, ICreditCustomerGateway customers, TimeProvider clock)
{
    public async Task<CreditStatus> Handle(SubmitCreditApplication command, RequestActor actor, CancellationToken cancellationToken)
    {
        actor.Require("banking-operator"); Guard.Id(command.CustomerId, "Customer");
        ArgumentNullException.ThrowIfNull(command.IncomeSources); ArgumentNullException.ThrowIfNull(command.ExistingDebts);
        ArgumentNullException.ThrowIfNull(command.Obligations); ArgumentNullException.ThrowIfNull(command.Documents);
        Guard.Against(!await customers.ExistsAsync(command.CustomerId, cancellationToken), "credit.customer_not_found", "Customer does not exist.", FailureKind.NotFound);
        var currency = new Currency(command.Currency); var now = clock.GetUtcNow();
        Guard.Against(command.IncomeSources.Sum(item => item.MonthlyAmount) != command.MonthlyIncome
            || command.ExistingDebts.Sum(item => item.MonthlyPayment) != command.MonthlyDebtPayments,
            "credit.financial_totals", "Declared income and debt payments must match their detailed sources.");
        var financial = new ApplicantFinancialProfile(command.EmploymentType, new(command.MonthlyIncome, currency), new(command.MonthlyExpenses, currency),
            new(command.MonthlyDebtPayments, currency), new(command.HousingExpenses, currency));
        var application = CreditApplication.CreateDraftApplication(command.CustomerId, command.LoanProductId, command.LoanPurpose,
            new(command.RequestedAmount, currency), command.TermMonths, command.SubmissionChannel, financial, now);
        foreach (var source in command.IncomeSources) application.AddIncomeSource(source.Type, new(source.MonthlyAmount, currency), source.EvidenceReference, now);
        foreach (var debt in command.ExistingDebts) application.AddExistingDebt(debt.LenderReference, new(debt.OutstandingAmount, currency), new(debt.MonthlyPayment, currency), now);
        foreach (var obligation in command.Obligations) application.AddObligation(obligation.Type, new(obligation.MonthlyAmount, currency), now);
        foreach (var document in command.Documents) application.AttachDocument(document.Type, document.ProtectedReference, now);
        application.SubmitApplication(now); await repository.AddAsync(application, cancellationToken);
        return new(application.Id, application.ApplicationNumber, application.CustomerId, application.Status, application.LoanProductId,
            application.LoanPurpose, application.RequestedAmount.Amount, currency.Code, application.RequestedTermMonths,
            application.SubmittedAtUtc, application.ReviewedAtUtc, application.AssignedCreditOfficerId, application.Version);
    }
}
