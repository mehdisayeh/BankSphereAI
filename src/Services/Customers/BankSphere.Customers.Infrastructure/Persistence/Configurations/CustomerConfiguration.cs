using BankSphere.Customers.Domain;
using BankSphere.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankSphere.Customers.Infrastructure;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        BankingMapping.Root(builder, "customers", "Customers");
        builder.Property(value => value.CustomerNumber).HasMaxLength(40);
        builder.HasIndex(value => value.CustomerNumber).IsUnique();
        builder.OwnsOne(value => value.Individual, owned =>
        {
            owned.ToTable("CustomerIndividual", "customers");
        });
        builder.OwnsOne(value => value.Corporate, owned =>
        {
            owned.ToTable("CustomerCorporate", "customers");
        });
        builder.OwnsOne(value => value.Financial, owned =>
        {
            owned.ToTable("CustomerFinancial", "customers");
        });
        builder.OwnsMany(value => value.Addresses, owned =>
        {
            BankingMapping.Child(owned, "customers", "CustomerAddresses");
        });
        builder.OwnsMany(value => value.Contacts, owned =>
        {
            BankingMapping.Child(owned, "customers", "CustomerContacts");
        });
        builder.OwnsMany(value => value.IdentityDocuments, owned =>
        {
            BankingMapping.Child(owned, "customers", "CustomerIdentityDocuments");
        });
        builder.OwnsMany(value => value.Consents, owned =>
        {
            BankingMapping.Child(owned, "customers", "CustomerConsents");
        });
        builder.OwnsMany(value => value.Relationships, owned =>
        {
            BankingMapping.Child(owned, "customers", "CustomerRelationships");
        });
        builder.OwnsMany(value => value.BeneficialOwners, owned =>
        {
            BankingMapping.Child(owned, "customers", "CustomerBeneficialOwners");
            owned.ToTable("CustomerBeneficialOwners", "customers", table => table.HasCheckConstraint("CK_BeneficialOwnership_Percentage", "[OwnershipPercentage] >= 0 AND [OwnershipPercentage] <= 100"));
        });
        builder.OwnsMany(value => value.Countries, owned =>
        {
            BankingMapping.Child(owned, "customers", "CustomerCountries");
        });
        builder.OwnsMany(value => value.RiskProfiles, owned =>
        {
            BankingMapping.Child(owned, "customers", "CustomerRiskProfiles");
            owned.OwnsMany(value => value.Factors, factors => BankingMapping.Child(factors, "customers", "CustomerRiskFactors"));
        });
    }
}
