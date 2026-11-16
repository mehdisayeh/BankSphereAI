using System.Text.Json;
using BankSphere.SharedKernel;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace BankSphere.Persistence;

public abstract class OutboxDbContext(DbContextOptions options) : DbContext(options)
{
    protected static void ConfigureOutbox(ModelBuilder modelBuilder, string schema)
    {
        modelBuilder.Entity<OutboxMessage>(builder =>
        {
            builder.ToTable("OutboxMessages", schema);
            builder.HasKey(message => message.Id);
            builder.Property(message => message.Type).HasMaxLength(250).IsRequired();
            builder.Property(message => message.Payload).IsRequired();
            builder.HasIndex(message => new { message.PublishedAtUtc, message.OccurredAtUtc });
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var roots = ChangeTracker.Entries<BankingAggregate>().Select(entry => entry.Entity).ToArray();
        var events = roots.SelectMany(root => root.DomainEvents).ToArray();
        foreach (var domainEvent in events)
        {
            Set<OutboxMessage>().Add(new() { Id = domainEvent.EventId, Type = domainEvent.GetType().Name + ".v1",
                Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()), OccurredAtUtc = domainEvent.OccurredAtUtc });
        }
        try
        {
            // One relational SaveChanges transaction persists aggregate + owned history + outbox.
            var result = await base.SaveChangesAsync(cancellationToken);
            foreach (var root in roots) root.ClearDomainEvents();
            return result;
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new BusinessException("persistence.concurrency", "The resource changed; reload and retry.", FailureKind.Conflict);
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqlException { Number: 2601 or 2627 })
        {
            throw new BusinessException("persistence.duplicate", "A conflicting business identifier or active investigation already exists.", FailureKind.Conflict);
        }
    }
}
