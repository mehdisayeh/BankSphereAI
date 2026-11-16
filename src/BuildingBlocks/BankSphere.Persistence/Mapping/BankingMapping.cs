using BankSphere.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankSphere.Persistence;

public static class BankingMapping
{
    public static void Root<T>(EntityTypeBuilder<T> builder, string schema, string table) where T : BankingAggregate
    {
        builder.ToTable(table, schema, tableBuilder => tableBuilder.HasCheckConstraint($"CK_{table}_Audit", "[Version] > 0 AND [UpdatedAtUtc] >= [CreatedAtUtc] AND DATEPART(TZOFFSET, [CreatedAtUtc]) = 0 AND DATEPART(TZOFFSET, [UpdatedAtUtc]) = 0"));
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).ValueGeneratedNever();
        builder.Property(entity => entity.Version).IsConcurrencyToken();
        builder.Ignore(entity => entity.DomainEvents);
    }

    public static void Money<TOwner>(OwnedNavigationBuilder<TOwner, Money> builder) where TOwner : class
    {
        builder.Property(money => money.Amount).HasPrecision(19, 4);
        builder.Property(money => money.Currency).HasConversion(currency => currency.Code, code => new Currency(code)).HasColumnType("char(3)");
    }

    public static void Child<TOwner, TChild>(OwnedNavigationBuilder<TOwner, TChild> builder, string schema, string table)
        where TOwner : class where TChild : class
    {
        builder.ToTable(table, schema);
        builder.WithOwner().HasForeignKey("ParentId");
        builder.HasKey("Id");
        builder.Property<Guid>("Id").ValueGeneratedNever();
        builder.HasIndex("ParentId");
    }

    public static void Conventions(ModelBuilder modelBuilder)
    {
        foreach (var property in modelBuilder.Model.GetEntityTypes().SelectMany(entity => entity.GetProperties()))
        {
            if (property.ClrType == typeof(string) && property.Name != nameof(OutboxMessage.Payload)) property.SetMaxLength(property.GetMaxLength() ?? 2000);
            if (property.ClrType == typeof(decimal) || property.ClrType == typeof(decimal?)) { property.SetPrecision(19); property.SetScale(4); }
        }
    }
}
