using BankSphere.CreditAssessment.Domain;
using BankSphere.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankSphere.CreditAssessment.Infrastructure;

public sealed class CreditApplicationConfiguration : IEntityTypeConfiguration<CreditApplication>
{
    public void Configure(EntityTypeBuilder<CreditApplication> builder)
    {
        BankingMapping.Root(builder, "credit", "Applications");
        builder.Property(value => value.ApplicationNumber).HasMaxLength(40);
        builder.HasIndex(value => value.ApplicationNumber).IsUnique();
        builder.HasIndex(value => new { value.CustomerId, value.Status });
        BankingMapping.Money(builder.OwnsOne(value => value.RequestedAmount));
        builder.Navigation(value => value.RequestedAmount).IsRequired();
        builder.OwnsOne(value => value.Financial, owned =>
        {
            owned.ToTable("CreditApplicationFinancial", "credit");
            BankingMapping.Money(owned.OwnsOne(value => value.DeclaredMonthlyIncome));
            owned.Navigation(value => value.DeclaredMonthlyIncome).IsRequired();
            BankingMapping.Money(owned.OwnsOne(value => value.DeclaredMonthlyExpenses));
            owned.Navigation(value => value.DeclaredMonthlyExpenses).IsRequired();
            BankingMapping.Money(owned.OwnsOne(value => value.ExistingMonthlyDebtPayments));
            owned.Navigation(value => value.ExistingMonthlyDebtPayments).IsRequired();
            BankingMapping.Money(owned.OwnsOne(value => value.HousingExpenses));
            owned.Navigation(value => value.HousingExpenses).IsRequired();
        });
        builder.OwnsMany(value => value.IncomeSources, owned =>
        {
            BankingMapping.Child(owned, "credit", "CreditApplicationIncomeSources");
        });
        builder.OwnsMany(value => value.ExistingDebts, owned =>
        {
            BankingMapping.Child(owned, "credit", "CreditApplicationExistingDebts");
        });
        builder.OwnsMany(value => value.Obligations, owned =>
        {
            BankingMapping.Child(owned, "credit", "CreditApplicationObligations");
        });
        builder.OwnsMany(value => value.Collateral, owned =>
        {
            BankingMapping.Child(owned, "credit", "CreditApplicationCollateral");
        });
        builder.OwnsMany(value => value.Documents, owned =>
        {
            BankingMapping.Child(owned, "credit", "CreditApplicationDocuments");
        });
        builder.OwnsMany(value => value.Assessments, owned =>
        {
            BankingMapping.Child(owned, "credit", "CreditApplicationAssessments");
            owned.OwnsMany(value => value.RiskFactors, factors => BankingMapping.Child(factors, "credit", "CreditAssessmentRiskFactors"));
        });
        builder.OwnsMany(value => value.Recommendations, owned =>
        {
            BankingMapping.Child(owned, "credit", "CreditApplicationRecommendations");
        });
        builder.OwnsMany(value => value.ReviewRequests, owned =>
        {
            BankingMapping.Child(owned, "credit", "CreditApplicationReviewRequests");
        });
        builder.OwnsMany(value => value.DecisionHistory, owned =>
        {
            BankingMapping.Child(owned, "credit", "CreditApplicationDecisionHistory");
        });
    }
}
