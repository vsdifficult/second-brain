// src/SecondBrain.BuildingBlocks/EFCore/BaseAppDbContext.cs
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SecondBrain.BuildingBlocks.Core.Entities;
using SecondBrain.BuildingBlocks.EFCore.Outbox;
using SecondBrain.BuildingBlocks.Messaging.Kafka.Abstractions;

namespace SecondBrain.BuildingBlocks.EFCore;

public abstract class BaseBbContext : DbContext
{
    protected BaseBbContext(DbContextOptions options) : base(options) { }

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public void EnqueueOutboxMessage<TEvent>(string topic, string key, TEvent @event)
        where TEvent : Event
    {
        OutboxMessages.Add(new OutboxMessage
        {
            Topic = topic,
            Key = key,
            Payload = JsonSerializer.Serialize(@event, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }),
            Type = typeof(TEvent).Name,
            OccurredAt = DateTime.UtcNow
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt = utcNow;
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = utcNow;
        }

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt = utcNow;
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = utcNow;
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}