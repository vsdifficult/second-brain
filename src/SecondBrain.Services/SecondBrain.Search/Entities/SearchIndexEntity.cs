using SecondBrain.BuildingBlocks.Core.Entities;

namespace SecondBrain.Services.SearchService.Entities;

public class SearchIndexEntry : BaseEntity
{
    public required Guid NoteId { get; set; }
    public required Guid OwnerId { get; set; }
    public required string Title { get; set; }
    public required string Body { get; set; }
    public string Tags { get; set; } = ""; // JSON array: ["tag1","tag2"]
    public DateTime IndexedAt { get; set; } = DateTime.UtcNow;
}