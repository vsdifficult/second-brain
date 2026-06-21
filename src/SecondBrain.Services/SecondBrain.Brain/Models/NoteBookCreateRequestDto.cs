
namespace SecondBrain.Services.BrainService.Models; 

public record NoteBookCreateRequestDto
{
    public required string Name {get; init; }
}