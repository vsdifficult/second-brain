namespace SecondBrain.BuildingBlocks.Messaging.Kafka.Abstractions;

public interface IEventBus
{
    Task PublishAsync<T>(string topic, string key, T @event, CancellationToken cancellationToken = default)
        where T : Event;

    Task PublishRawAsync(string topic, string key, string payload,
        string? messageType = null, CancellationToken cancellationToken = default);
}