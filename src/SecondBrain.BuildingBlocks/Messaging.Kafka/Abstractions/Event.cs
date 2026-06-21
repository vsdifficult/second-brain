
namespace SecondBrain.BuildingBlocks.Messaging.Kafka.Abstractions; 

public abstract record Event
{
    public required Guid Id {get ; set; } = Guid.NewGuid(); 

    public required DateTime CreationDate {get; set; } = DateTime.UtcNow; 
}