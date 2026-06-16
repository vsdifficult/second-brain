
namespace SecondBrain.BuildingBlocks.Messaging.Kafka.Abstractions; 

public interface IEventBus
{
    Task PublishAsync<T>(string topic, string key, T @event, CancellationToken cancellationToken = default   ) where T: Event;  
} 

