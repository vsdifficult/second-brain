
using SecondBrain.BuildingBlocks.Core.Entities; 

namespace SecondBrain.Services.BrainService.Entities; 
public class NoteEntity : BaseEntity
{
    public required string Title { get; set; }
    public required string Body { get; set; }
    public required Guid OwnerId { get; set; }
    public Guid? NotebookId { get; set; }
    public NoteBookEntity? Notebook { get; set; }
    public List<NoteTagEntity> NoteTags { get; set; } = new();
    public List<NoteLinkEntity> OutgoingLinks { get; set; } = new();
    public List<NoteLinkEntity> IncomingLinks { get; set; } = new();
} 

public class NoteLinkEntity : BaseEntity
{
    public required Guid SourceNoteId { get; set; }
    public required Guid TargetNoteId { get; set; }
}