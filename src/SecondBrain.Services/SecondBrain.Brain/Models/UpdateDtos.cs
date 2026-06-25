namespace SecondBrain.Services.BrainService.Models;

public record NoteUpdateRequestDto
{
    public required string Body { get; init; }
}

public record NoteBookUpdateRequestDto
{
    public required string Name { get; init; }
}
