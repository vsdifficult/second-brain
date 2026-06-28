using SecondBrain.BuildingBlocks.Messaging.Kafka.Abstractions;

namespace SecondBrain.BuildingBlocks.Messaging.Kafka.Events;

public record NoteCreatedEvent : Event
{
    public required Guid NoteId { get; init; }
    public required string Title { get; init; }
    public required string Body { get; init; }
    public required Guid OwnerId { get; init; }
    public Guid? NotebookId { get; init; }
    public List<string> Tags { get; init; } = [];
}