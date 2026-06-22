using Microsoft.IdentityModel.Tokens;
using SecondBrain.BuildingBlocks.Core.Repositories;
using SecondBrain.Services.BrainService.Entities;
using SecondBrain.Services.BrainService.Models;
using SecondBrain.Services.BrainService.Services.Interfaces;

namespace SecondBrain.Services.BrainService.Services.Implementations;

public class NoteBookService : INoteBookService
{   
    private readonly IRepository<NoteBookEntity, Guid> _notebookrepository; 
    private readonly ILogger<NoteBookService> _logger; 
    public NoteBookService(
        IRepository<NoteBookEntity, Guid> notebookrepository,
        ILogger<NoteBookService> logger
    )
    {
        _notebookrepository = notebookrepository; 
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
        throw new NotImplementedException(); 
    }
}