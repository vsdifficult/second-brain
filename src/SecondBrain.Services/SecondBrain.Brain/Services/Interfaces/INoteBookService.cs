
using SecondBrain.Services.BrainService.Entities;
using SecondBrain.Services.BrainService.Models;

namespace SecondBrain.Services.BrainService.Services.Interfaces;

public interface INoteBookService
{
    Task<Guid> CreateNoteBookAsync(NoteBookCreateRequestDto dto, CancellationToken ct);
    Task<NoteBookEntity> GetNoteBookAsync(Guid id, CancellationToken ct);
    Task<bool> UpdateNoteBookAsync(Guid id, string name, CancellationToken ct);
    Task<bool> DeleteNoteBookAsync(Guid id, CancellationToken ct);
    Task<bool> AddNoteAsync(Guid NoteId, CancellationToken ct); 

}