namespace SecondBrain.Services.BrainService.Models; 

public record TagCreate
{
    public required string Name {get; init; }
} 

public record ApplyTagToNote
{
    public required Guid TagId {get; init; } 
    public required Guid NoteId {get; init; }
}