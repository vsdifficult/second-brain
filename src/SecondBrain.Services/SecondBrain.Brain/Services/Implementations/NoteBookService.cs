using Microsoft.IdentityModel.Tokens;
using SecondBrain.BuildingBlocks.Core.Repositories;
using SecondBrain.Services.BrainService.Entities;
using SecondBrain.Services.BrainService.Models;
using SecondBrain.Services.BrainService.Services.Interfaces;
using SecondBrain.Services.BrainService.Data.Repositories; 

namespace SecondBrain.Services.BrainService.Services.Implementations;

public class NoteBookService : INoteBookService
{   
    private readonly INoteBookRepository _notebookrepository; 
    private readonly INoteRepository _noterepository; 
    private readonly ILogger<NoteBookService> _logger; 
    public NoteBookService(
        INoteBookRepository notebookrepository,
        INoteRepository noterepository,
        ILogger<NoteBookService> logger
    )
    {
        _notebookrepository = notebookrepository; 
        _noterepository = noterepository;
        _logger = logger; 
    }

    public async Task<Guid> CreateNoteBookAsync(NoteBookCreateRequestDto dto, CancellationToken ct)
    {
        if (dto.Name.IsNullOrEmpty())
            throw new Exception("NoteBook Name is empty"); 
        if (dto.OwnerId == null)
            throw new Exception("NoteBook isnt have owner");
        
        var notebook = new NoteBookEntity
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            NoteBookName = dto.Name,
            OwnerId = dto.OwnerId
        };  

        await _notebookrepository.CreateAsync(notebook, ct); 

        _logger.LogInformation("Note with id: {NotebookId} created", notebook.Id);

        return notebook.Id;
    }

    public async Task<bool> DeleteNoteBookAsync(Guid id, CancellationToken ct)
    {
        var notebook = await _notebookrepository.GetByIdAsync(id); 
        if (notebook==null)
            throw new Exception($"NoteBook with id: {id} not found" );  
        
        await _notebookrepository.DeleteAsync(id, ct); 
        _logger.LogInformation("NoteBook with id: {NoteId} deleted", id); 
        return true;
    }

    public async Task<NoteBookEntity> GetNoteBookAsync(Guid id, CancellationToken ct)
    {
        var notebook = await _notebookrepository.GetByIdAsync(id, ct); 
        if (notebook == null)
            throw new Exception($"NoteBook with id: {id} not found");
        return notebook; 
    }

    public async Task<bool> UpdateNoteBookAsync(Guid id, string name, CancellationToken ctv)
    {
        throw new NotImplementedException();
    } 

    public async Task<bool> AddNoteAsync(Guid NoteId, CancellationToken ct)
    {
        var note = await _noterepository.GetByIdAsync(NoteId, ct); 
        if (note == null)
            throw new Exception($"Note with id: {NoteId} not found"); 

        if (note.NotebookId == null)
            throw new Exception($"Note with id: {NoteId} is not associated with any notebook");

        var notebookId = note.NotebookId.Value; 
        var result = await _notebookrepository.AddNoteAsync(notebookId, note, ct); 
        return result;
    }
}