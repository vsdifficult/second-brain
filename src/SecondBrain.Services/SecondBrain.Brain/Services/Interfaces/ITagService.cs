using SecondBrain.Services.BrainService.Entities;
using SecondBrain.Services.BrainService.Models;

namespace SecondBrain.Services.BrainService.Services.Interfaces;

public interface ITagService
{
    Task<Guid> CreateTagAsync(TagCreate dto, CancellationToken ct = default);
    Task<TagEntity> GetTagAsync(Guid id, CancellationToken ct = default);
    Task<List<TagEntity>> GetTagsByNoteAsync(Guid noteId, CancellationToken ct = default);
    Task<bool> ApplyTagToNoteAsync(ApplyTagToNote dto, CancellationToken ct = default);
    Task<bool> RemoveTagFromNoteAsync(ApplyTagToNote dto, CancellationToken ct = default);
    Task<bool> DeleteTagAsync(Guid id, CancellationToken ct = default);
}