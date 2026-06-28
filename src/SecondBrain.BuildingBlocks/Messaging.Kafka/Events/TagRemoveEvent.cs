using SecondBrain.BuildingBlocks.Messaging.Kafka.Abstractions;

namespace SecondBrain.BuildingBlocks.Messaging.Kafka.Events; 
public record TagRemovedEvent : Event
{
    public required Guid NoteId { get; init; }
    public required Guid TagId { get; init; }
    public required string TagName { get; init; }
}