namespace SecondBrain.Services.BrainService.Models; 

public record NoteCreateRequestDto
{
    public required string Title { get; set; }
    public required string Body { get; set; }

    public Guid? NotebookId { get; set; }

    public Guid OwnerId {get; set; }
}