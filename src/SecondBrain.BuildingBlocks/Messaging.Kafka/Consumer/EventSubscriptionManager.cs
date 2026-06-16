using SecondBrain.BuildingBlocks.Messaging.Kafka.Abstractions;

namespace SecondBrain.BuildingBlocks.Messaging.Kafka.Consumer;

public class EventSubscriptionManager
{
    private readonly Dictionary<string, Type> _handlers = new();

    public void AddSubscription<TEvent, THandler>(string topic)
        where TEvent : Event
        where THandler : IIntegrationEventHandler<TEvent>
    {
        _handlers[topic] = typeof(THandler);
    }

    public Type? GetHandlerTypeForTopic(string topic) => _handlers.GetValueOrDefault(topic);
    public IEnumerable<string> GetTopics() => _handlers.Keys;
}
