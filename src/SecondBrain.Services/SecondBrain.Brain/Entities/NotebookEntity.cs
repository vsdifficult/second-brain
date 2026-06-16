using SecondBrain.BuildingBlocks.Core.Entities;

namespace SecondBrain.Services.BrainService.Entities;

public class NoteBookEntity : BaseEntity
{
    public required string NoteBookName { get; set; }
    public required Guid OwnerId { get; set; }
    public List<NoteEntity> Notes { get; set; } = new();
}