using BankSphere.Accounts.Domain;
using BankSphere.Compliance.Domain;
using BankSphere.CreditAssessment.Application;
using BankSphere.CreditAssessment.Domain;
using BankSphere.Customers.Domain;
using BankSphere.FraudInvestigation.Domain;
using BankSphere.SharedKernel;
using Xunit;

namespace BankSphere.Banking.Tests;

public sealed class DomainTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 13, 12, 0, 0, TimeSpan.Zero);
    private static Currency Eur => new("EUR");
    private static Money EurMoney(decimal value) => new(value, Eur);
    internal static BankAccount Account() => BankAccount.OpenAccount(Guid.NewGuid(), AccountType.Current, "SYNTHETIC-CURRENT", "Synthetic current account", new("EUR"), "TEST-BRANCH", "Synthetic testing", new(1000, new("EUR")), new(10000, new("EUR")), "test-operator", Now);
    internal static FraudCase Case(Guid? transaction = null) => FraudCase.OpenInvestigation(Guid.NewGuid(), Guid.NewGuid(), transaction ?? Guid.NewGuid(), "Card", "velocity", FraudSeverity.High, 2, "synthetic-simulator", Now, 85, "High", "Synthetic fraud scenario", EurMoney(50), EurMoney(50), "test-operator", Now.AddDays(1), Now);
    internal static CreditApplication Credit()
    {
        var value = CreditApplication.CreateDraftApplication(Guid.NewGuid(), "SYNTHETIC-PERSONAL", "Synthetic test loan", EurMoney(10000), 24, "Test", Financial(), Now);
        value.AddIncomeSource("salary", EurMoney(3000), "protected:salary-test", Now);
        value.AttachDocument("income", "protected:salary-test", Now); return value;
    }
    internal static ApplicantFinancialProfile Financial() => new("Employed", EurMoney(3000), EurMoney(500), EurMoney(200), EurMoney(600));

    [Theory]
    [InlineData(-1)]
    [InlineData(0.001)]
    public void RejectsNegativeAndOverPreciseAmounts(decimal amount) => Assert.Throws<BusinessException>(() => EurMoney(amount));

    [Theory]
    [InlineData("XYZ")]
    [InlineData("ABC")]
    [InlineData("")]
    public void RejectsUnsupportedCurrency(string currency) => Assert.Throws<BusinessException>(() => new Currency(currency));

    [Fact]
    public void UsesCurrencySpecificPrecision()
    {
        Assert.Throws<BusinessException>(() => new Money(1.1m, new("JPY")));
        Assert.Equal(1.123m, new Money(1.123m, new("KWD")).Amount);
        Assert.Throws<BusinessException>(() => new Money(1.1234m, new("KWD")));
    }

    [Fact]
    public void CurrencyMismatchIsNotAnImplicitConversion() => Assert.Throws<BusinessException>(() => EurMoney(1).Add(new(1, new("USD"))));

    [Fact]
    public void IbanChecksChecksum()
    {
        Assert.Equal("FR1420041010050500013M02606", new Iban("FR1420041010050500013M02606").Value);
        Assert.Throws<BusinessException>(() => new Iban("FR1420041010050500013M02607"));
    }

    [Fact]
    public void AccountCannotFreezeBeforeActivation() => Assert.Throws<BusinessException>(() => Account().FreezeAccount("reason", "actor", Now));

    [Fact]
    public void ActivationRequiresBothEligibilityChecksAndFreshEvidence()
    {
        var value = Account();
        Assert.Throws<BusinessException>(() => value.ActivateAccount(new(true, false, Now, "evidence"), "actor", Now));
        Assert.Throws<BusinessException>(() => value.ActivateAccount(new(true, true, Now.AddHours(-1), "evidence"), "actor", Now));
        value.ActivateAccount(new(true, true, Now, "evidence"), "actor", Now);
        Assert.Throws<BusinessException>(() => value.ActivateAccount(new(true, true, Now, "evidence"), "actor", Now));
    }

    [Fact]
    public void UnfreezeRequiresEligibilityBeforeResumingActivity()
    {
        var value = Account(); value.ActivateAccount(new(true, true, Now, "evidence"), "actor", Now);
        value.FreezeAccount("Review", "actor", Now); value.UnfreezeAccount("Reviewed", "actor", Now);
        Assert.Equal(AccountStatus.Suspended, value.Status);
    }

    [Fact]
    public void SimulatorBalanceCannotAuthorizeClosure()
    {
        var value = Account();
        var projection = AccountBalanceSnapshot.FromProjection(EurMoney(0), EurMoney(0), EurMoney(0), Now, "simulator:batch", BalanceSource.Simulator);
        Assert.Throws<BusinessException>(() => value.CloseAccount(projection, "Closed", "actor", Now));
        var nonzero = AccountBalanceSnapshot.FromProjection(EurMoney(1), EurMoney(0), EurMoney(0), Now, "ledger:reference", BalanceSource.AuthoritativeLedger);
        Assert.Throws<BusinessException>(() => value.CloseAccount(nonzero, "Closed", "actor", Now));
    }

    [Fact]
    public void FraudCannotCloseWithoutResolutionOrResolveWithoutHumanDecision()
    {
        var value = Case();
        Assert.Throws<BusinessException>(() => value.CloseInvestigation("actor", "reason", Now));
        Assert.Throws<BusinessException>(() => value.ResolveInvestigation("fraud", "reason", "actor", Now));
    }

    [Fact]
    public void FraudReviewCannotBeDuplicatedAndDecisionRequiresAssignedReviewerAndEvidence()
    {
        var value = Case(); value.RequestHumanReview("reviewer", "reason", "operator", Now);
        Assert.Throws<BusinessException>(() => value.RequestHumanReview("reviewer", "again", "operator", Now));
        Assert.Throws<BusinessException>(() => value.RecordHumanDecision(FraudDecisionClassification.FraudConfirmed, "Reason", "intruder", "evidence", Now));
        Assert.Throws<BusinessException>(() => value.RecordHumanDecision(FraudDecisionClassification.FraudConfirmed, "Reason", "reviewer", "evidence", Now));
        value.AttachEvidence("statement", "synthetic", "Synthetic evidence", "evidence", new string('A', 64), Now);
        value.RecordHumanDecision(FraudDecisionClassification.FraudConfirmed, "Verified evidence", "reviewer", "evidence", Now);
        value.ResolveInvestigation("FraudConfirmed", "Human reviewed", "operator", Now); value.CloseInvestigation("operator", "Resolved", Now);
        Assert.False(value.IsActive); value.ReopenInvestigation("operator", "New evidence", Now); Assert.True(value.IsActive);
    }

    [Fact]
    public void EvidenceRequiresIntegrityDigest() => Assert.Throws<BusinessException>(() => Case().AttachEvidence("type", "source", "description", "protected:ref", "invalid", Now));

    [Fact]
    public void CreditCannotApproveDraftOrAssessBeforeSubmission()
    {
        var value = Credit();
        Assert.Throws<BusinessException>(() => value.ApproveApplication("Reason", "protected:salary-test", "officer", Now));
        Assert.Throws<BusinessException>(() => value.BeginAssessment("officer", Now));
    }

    [Fact]
    public void CreditSubmissionRequiresEvidenceAndCannotBeRepeated()
    {
        var value = Credit(); value.SubmitApplication(Now);
        Assert.Throws<BusinessException>(() => value.SubmitApplication(Now));
        Assert.Throws<BusinessException>(() => value.UpdateFinancialInformation(Financial(), Now));
    }

    [Fact]
    public void RecommendationNeverAutomaticallyApprovesCredit()
    {
        var value = Credit(); value.SubmitApplication(Now); value.BeginAssessment("officer", Now);
        var assessment = BankSphere.CreditAssessment.Domain.CreditAssessment.Evaluate(Financial(), new FinancialCalculationPolicy("synthetic-v1", true), "officer",
            [new(Guid.NewGuid(), "income", "Synthetic income evidence", "protected:salary-test")], Now);
        value.RecordAssessment(assessment, Now);
        value.RecommendDecision(CreditDecisionRecommendation.Approve, "Synthetic policy result", "protected:salary-test", "officer", Now);
        Assert.Equal(CreditApplicationStatus.Assessing, value.Status);
        value.RequestHumanReview("reviewer", "Review required", Now);
        Assert.Throws<BusinessException>(() => value.ApproveApplication("Reason", "protected:salary-test", "intruder", Now));
        value.ApproveApplication("Reviewed income evidence", "protected:salary-test", "reviewer", Now);
        Assert.Equal(CreditApplicationStatus.Approved, value.Status);
        Assert.Throws<BusinessException>(() => value.WithdrawApplication("actor", "reason", Now));
    }

    [Fact]
    public void CreditFinancialProfileRejectsMixedCurrencies()
    {
        var profile = Financial() with { HousingExpenses = new(1, new("USD")) };
        Assert.Throws<BusinessException>(() => Credit().UpdateFinancialInformation(profile, Now));
    }

    [Fact]
    public void CorporateAndIndividualIdentityAreMutuallyExclusive()
    {
        var financial = new FinancialProfile("Synthetic", EmploymentStatus.Employed, "Test employer", null, "20000-40000", "Salary", "Savings", "Monthly salary");
        Assert.Throws<BusinessException>(() => Customer.RegisterCustomer(CustomerType.Individual, CustomerSegment.Retail, null, null, financial, "Test", "en", "email", Now));
    }

    [Fact]
    public void ComplianceApprovalRequiresEvidenceAndDiligence()
    {
        var value = ComplianceCase.Open(Guid.NewGuid(), ComplianceCaseType.PeriodicKyc, "FR", "Periodic review", "synthetic-source", Now.AddDays(30), Now);
        value.BeginReview("officer", Now);
        Assert.Throws<BusinessException>(() => value.RecordDecision(true, "officer", "Reviewed", Now));
        value.RecordKycReview("officer", "Synthetic review", "protected:kyc", Now.AddYears(1), Now);
        value.AttachDocument("kyc", "protected:kyc", Now); value.AssessRisk(RiskClassification.High, "pep", "Synthetic PEP scenario", "officer", Now);
        value.RecordDueDiligence(false, "Standard", "Synthetic funds", "officer", "protected:kyc", Now);
        Assert.Throws<BusinessException>(() => value.RecordDecision(true, "officer", "Reviewed", Now));
        value.RecordDueDiligence(true, "Enhanced", "Synthetic funds", "officer", "protected:kyc", Now);
        value.RecordDecision(true, "officer", "Reviewed evidence", Now); Assert.Equal(ComplianceStatus.Approved, value.Status);
    }
}
