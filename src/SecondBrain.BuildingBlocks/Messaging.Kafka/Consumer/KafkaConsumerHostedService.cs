using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SecondBrain.BuildingBlocks.Infrastructure.Messaging.Options;
using SecondBrain.BuildingBlocks.Messaging.Kafka.Abstractions;

namespace SecondBrain.BuildingBlocks.Messaging.Kafka.Consumer;

public class KafkaConsumerHostedService : BackgroundService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly EventSubscriptionManager _subscriptions;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<KafkaConsumerHostedService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public KafkaConsumerHostedService(
        IOptions<KafkaOptions> options,
        EventSubscriptionManager subscriptions,
        IServiceScopeFactory scopeFactory,
        ILogger<KafkaConsumerHostedService> logger)
    {
        _subscriptions = subscriptions;
        _scopeFactory = scopeFactory;
        _logger = logger;

        var config = new ConsumerConfig
        {
            BootstrapServers = options.Value.BootstrapServers,
            GroupId = options.Value.GroupId,
            ClientId = options.Value.ClientId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        _consumer = new ConsumerBuilder<string, string>(config)
            .SetErrorHandler((_, e) => _logger.LogError("Kafka consumer error: {Reason}", e.Reason))
            .Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var topics = _subscriptions.GetTopics().ToList();
        if (topics.Count == 0)
        {
            _logger.LogWarning("No topic subscriptions registered; consumer host is idle");
            return;
        }

        _consumer.Subscribe(topics);

        while (!stoppingToken.IsCancellationRequested)
        {
            ConsumeResult<string, string>? result = null;
            try
            {
                result = _consumer.Consume(TimeSpan.FromSeconds(1));
                if (result is null) continue;

                await DispatchAsync(result, stoppingToken);
                _consumer.Commit(result);
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Kafka consume error: {Reason}", ex.Error.Reason);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Handler failed for {Topic}/{Partition}/{Offset}; offset not committed, message will be retried",
                    result?.Topic, result?.Partition, result?.Offset);
            }
        }
    }

    private async Task DispatchAsync(ConsumeResult<string, string> result, CancellationToken ct)
    {
        var handlerType = _subscriptions.GetHandlerTypeForTopic(result.Topic);
        if (handlerType is null)
        {
            _logger.LogWarning("No handler registered for topic {Topic}", result.Topic);
            return;
        }

        // Находим тип события из интерфейса обработчика
        var eventType = handlerType.GetInterfaces()
            .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>))
            .GetGenericArguments()[0];

        var @event = JsonSerializer.Deserialize(result.Message.Value, eventType, JsonOptions);
        if (@event is null)
        {
            _logger.LogWarning("Failed to deserialize message on topic {Topic}", result.Topic);
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService(handlerType);
        
        // Динамически вызываем метод HandleAsync
        var handleMethod = handlerType.GetMethod("HandleAsync")!;
        var task = (Task)handleMethod.Invoke(handler, [@event, ct])!;
        await task.ConfigureAwait(false);
    }

    public override void Dispose()
    {
        _consumer.Close();
        _consumer.Dispose();
        base.Dispose();
    }
}

public static class KafkaConsumerServiceCollectionExtensions
{
    public static IServiceCollection AddKafkaConsumer(
        this IServiceCollection services,
        Action<EventSubscriptionManager> configureSubscriptions)
    {
        var manager = new EventSubscriptionManager();
        configureSubscriptions(manager);
        services.AddSingleton(manager);
        return services.AddSingleton<IHostedService, KafkaConsumerHostedService>();
    }
}