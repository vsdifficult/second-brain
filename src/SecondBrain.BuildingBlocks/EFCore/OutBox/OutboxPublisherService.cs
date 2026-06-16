using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore; 
using SecondBrain.BuildingBlocks.Messaging.Kafka.Abstractions;

namespace SecondBrain.BuildingBlocks.EFCore.Outbox;

public class OutboxPublisherHostedService<TContext> : BackgroundService where TContext : BaseBbContext
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IEventBus _eventBus;
    private readonly ILogger<OutboxPublisherHostedService<TContext>> _logger;
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);
    private const int BatchSize = 50;

    public OutboxPublisherHostedService(
        IServiceScopeFactory scopeFactory,
        IEventBus eventBus,
        ILogger<OutboxPublisherHostedService<TContext>> logger)
    {
        _scopeFactory = scopeFactory;
        _eventBus = eventBus;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublishBatchAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox relay batch failed");
            }

            await Task.Delay(PollInterval, stoppingToken);
        }
    }

    private async Task PublishBatchAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TContext>();

        var pending = await context.OutboxMessages
            .Where(m => m.ProcessedAt == null)
            .OrderBy(m => m.OccurredAt)
            .Take(BatchSize)
            .ToListAsync(ct);

        if (pending.Count == 0) return;

        foreach (var message in pending)
        {
            try
            {
                await _eventBus.PublishRawAsync(message.Topic, message.Key, message.Payload, message.Type, ct);
                message.ProcessedAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                message.RetryCount++;
                message.Error = ex.Message;
                _logger.LogWarning(ex, "Failed to publish outbox message {Id} (attempt {Attempt})", message.Id, message.RetryCount);
            }
        }

        await context.SaveChangesAsync(ct);
    }
}

public static class OutboxServiceCollectionExtensions
{
    public static IServiceCollection AddOutboxPublisher<TContext>(this IServiceCollection services)
        where TContext : BaseBbContext
        => services.AddHostedService<OutboxPublisherHostedService<TContext>>();
}