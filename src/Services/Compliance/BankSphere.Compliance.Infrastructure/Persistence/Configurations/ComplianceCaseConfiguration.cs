using BankSphere.Compliance.Domain;
using BankSphere.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankSphere.Compliance.Infrastructure;

public sealed class ComplianceCaseConfiguration : IEntityTypeConfiguration<ComplianceCase>
{
    public void Configure(EntityTypeBuilder<ComplianceCase> builder)
    {
        BankingMapping.Root(builder, "compliance", "Cases");
        builder.Property(value => value.CaseNumber).HasMaxLength(40);
        builder.HasIndex(value => value.CaseNumber).IsUnique();
        builder.HasIndex(value => new { value.CustomerId, value.Status });
        builder.OwnsMany(value => value.KycReviews, owned =>
        {
            BankingMapping.Child(owned, "compliance", "ComplianceCaseKycReviews");
        });
        builder.OwnsMany(value => value.DueDiligence, owned =>
        {
            BankingMapping.Child(owned, "compliance", "ComplianceCaseDueDiligence");
        });
        builder.OwnsMany(value => value.Screenings, owned =>
        {
            BankingMapping.Child(owned, "compliance", "ComplianceCaseScreenings");
        });
        builder.OwnsMany(value => value.Documents, owned =>
        {
            BankingMapping.Child(owned, "compliance", "ComplianceCaseDocuments");
        });
        builder.OwnsMany(value => value.ReviewHistory, owned =>
        {
            BankingMapping.Child(owned, "compliance", "ComplianceCaseReviewHistory");
        });
        builder.OwnsMany(value => value.RiskFactors, owned =>
        {
            BankingMapping.Child(owned, "compliance", "ComplianceCaseRiskFactors");
        });
    }
}
