
namespace SecondBrain.Services.BrainService.Services.Interfaces;

public interface INoteBookService
{
    Task CreateNoteBookAsync(string name);
    Task<string> GetNoteBookAsync(Guid id);
    Task UpdateNoteBookAsync(Guid id, string name);
    Task DeleteNoteBookAsync(Guid id);
}