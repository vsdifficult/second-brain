
using SecondBrain.BuildingBlocks.Messaging.Kafka.Abstractions;

namespace SecondBrain.BuildingBlocks.Messaging.Kafka.Events;

public record NoteUpdatedEvent : Event
{
    public required Guid NoteId { get; init; }
    public required string Body { get; init; }
    public required Guid OwnerId { get; init; }
}