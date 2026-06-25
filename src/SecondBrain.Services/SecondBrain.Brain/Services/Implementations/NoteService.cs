
using Microsoft.IdentityModel.Tokens;
using SecondBrain.BuildingBlocks.Core.Repositories;
using SecondBrain.Services.BrainService.Entities;
using SecondBrain.Services.BrainService.Models;
using SecondBrain.Services.BrainService.Services.Interfaces; 

namespace SecondBrain.Services.BrainService.Services.Implementations;
public class NoteService : INoteService
{
    private readonly IRepository<NoteEntity, Guid> _noteRepository;
    private readonly ILogger<NoteService> _logger; 
    public NoteService(
        IRepository<NoteEntity, Guid> noteRepository,
        ILogger<NoteService> logger
    )
    {
        _noteRepository = noteRepository;
        _logger = logger;
    }

    public async Task<Guid> CreateNoteAsync(NoteCreateRequestDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(dto.Title))
            throw new ArgumentException("Title is empty", nameof(dto.Title));

        if (string.IsNullOrEmpty(dto.Body))
            throw new ArgumentException("Body is empty", nameof(dto.Body));

        if (dto.OwnerId == Guid.Empty)
            throw new ArgumentException("Note must have an owner", nameof(dto.OwnerId));

        var note = new NoteEntity
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Title = dto.Title,
            Body = dto.Body,
            OwnerId = dto.OwnerId,
            NotebookId = dto.NotebookId
        };

        await _noteRepository.CreateAsync(note, ct);
        _logger.LogInformation("Note with id: {NoteId} created", note.Id);
        return note.Id;
    }

    public async Task<bool> DeleteNoteAsync(Guid id, CancellationToken ct = default)
    {
        var note = await _noteRepository.GetByIdAsync(id);
        if (note == null)
            throw new Exception($"Note with id: {id} not found");
        await _noteRepository.DeleteAsync(id, ct);
        _logger.LogInformation("Note with id: {NoteId} deleted", id);
        return true;
    }

    public async Task<NoteEntity> GetNoteAsync(Guid id, CancellationToken ct = default)
    {
        var note = await _noteRepository.GetByIdAsync(id, ct); 
        if (note == null)
            throw new Exception($"Note with id: {id} not found");
        return note; 
    }

    public async Task<bool> UpdateNoteAsync(Guid id, string content, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new Exception("Content is empty");

        var note = await _noteRepository.GetByIdAsync(id);
        if (note == null)
            throw new Exception($"Note with id: {id} not found");

        note.Body = content;
        note.UpdatedAt = DateTime.UtcNow;

        await _noteRepository.UpdateAsync(note, ct);
        _logger.LogInformation("Note with id: {NoteId} updated", id);
        return true;
    }
}