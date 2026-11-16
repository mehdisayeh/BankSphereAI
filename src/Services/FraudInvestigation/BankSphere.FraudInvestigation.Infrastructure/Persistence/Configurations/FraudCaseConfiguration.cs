using BankSphere.FraudInvestigation.Domain;
using BankSphere.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankSphere.FraudInvestigation.Infrastructure;

public sealed class FraudCaseConfiguration : IEntityTypeConfiguration<FraudCase>
{
    public void Configure(EntityTypeBuilder<FraudCase> builder)
    {
        BankingMapping.Root(builder, "fraudinvestigation", "Cases");
        builder.Property(value => value.CaseNumber).HasMaxLength(40);
        builder.HasIndex(value => value.CaseNumber).IsUnique();
        builder.Property(value => value.Category).HasMaxLength(80);
        builder.HasIndex(value => new { value.TransactionId, value.Category }).IsUnique().HasFilter("[IsActive] = 1");
        builder.HasIndex(value => new { value.AccountId, value.Status });
        BankingMapping.Money(builder.OwnsOne(value => value.SuspectedFraudAmount));
        builder.Navigation(value => value.SuspectedFraudAmount).IsRequired();
        BankingMapping.Money(builder.OwnsOne(value => value.EstimatedFinancialExposure));
        builder.Navigation(value => value.EstimatedFinancialExposure).IsRequired();
        builder.OwnsMany(value => value.Indicators, owned =>
        {
            BankingMapping.Child(owned, "fraudinvestigation", "FraudCaseIndicators");
        });
        builder.OwnsMany(value => value.Evidence, owned =>
        {
            BankingMapping.Child(owned, "fraudinvestigation", "FraudCaseEvidence");
        });
        builder.OwnsMany(value => value.Notes, owned =>
        {
            BankingMapping.Child(owned, "fraudinvestigation", "FraudCaseNotes");
        });
        builder.OwnsMany(value => value.Assignments, owned =>
        {
            BankingMapping.Child(owned, "fraudinvestigation", "FraudCaseAssignments");
        });
        builder.OwnsMany(value => value.StatusHistory, owned =>
        {
            BankingMapping.Child(owned, "fraudinvestigation", "FraudCaseStatusHistory");
        });
        builder.OwnsMany(value => value.HumanReviews, owned =>
        {
            BankingMapping.Child(owned, "fraudinvestigation", "FraudCaseHumanReviews");
        });
        builder.OwnsMany(value => value.Decisions, owned =>
        {
            BankingMapping.Child(owned, "fraudinvestigation", "FraudCaseDecisions");
        });
        builder.OwnsMany(value => value.TransactionLinks, owned =>
        {
            BankingMapping.Child(owned, "fraudinvestigation", "FraudCaseTransactionLinks");
        });
    }
}
