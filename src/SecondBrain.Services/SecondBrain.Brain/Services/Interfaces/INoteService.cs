

using SecondBrain.Services.BrainService.Entities;
using SecondBrain.Services.BrainService.Models;

namespace SecondBrain.Services.BrainService.Services.Interfaces; 

public interface INoteService
{
    Task<Guid> CreateNoteAsync(NoteCreateRequestDto dto, CancellationToken ct = default);
    Task<NoteEntity> GetNoteAsync(Guid id, CancellationToken ct = default);
    Task<bool> UpdateNoteAsync(Guid id, string content, CancellationToken ct = default);
    Task<bool> DeleteNoteAsync(Guid id, CancellationToken ct = default);
}
