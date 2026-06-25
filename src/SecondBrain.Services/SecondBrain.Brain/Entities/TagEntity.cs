using SecondBrain.BuildingBlocks.Core.Entities;

namespace SecondBrain.Services.BrainService.Entities; 

public class TagEntity : BaseEntity
{
    public required string Name { get; set; }
    public List<NoteTagEntity> NoteTags { get; set; } = new();
}

public class NoteTagEntity // join entity
{
    public Guid NoteId { get; set; }
    public Guid TagId { get; set; }
    public NoteEntity Note { get; set; } = null!;
    public TagEntity Tag { get; set; } = null!;
}