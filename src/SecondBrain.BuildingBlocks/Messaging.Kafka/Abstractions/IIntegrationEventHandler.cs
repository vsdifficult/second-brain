
namespace SecondBrain.BuildingBlocks.Messaging.Kafka.Abstractions;

public interface IIntegrationEventHandler<in TIntegrationEvent> 
    where TIntegrationEvent : Event
{
    Task HandleAsync(TIntegrationEvent @event, CancellationToken cancellationToken = default);
}