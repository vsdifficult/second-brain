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

    public Type? GetHandlerTypeForTopic(string topic)
    {
        return _handlers.TryGetValue(topic, out var handlerType) ? handlerType : null;
    }

    public IEnumerable<string> GetTopics()
    {
        return _handlers.Keys;
    }
}