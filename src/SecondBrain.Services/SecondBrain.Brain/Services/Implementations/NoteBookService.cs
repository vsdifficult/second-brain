using SecondBrain.Services.BrainService.Data.Repositories;
using SecondBrain.Services.BrainService.Entities;
using SecondBrain.Services.BrainService.Models;
using SecondBrain.Services.BrainService.Services.Interfaces;

namespace SecondBrain.Services.BrainService.Services.Implementations;

public class NoteBookService : INoteBookService
{
    private readonly INoteBookRepository _notebookRepository;
    private readonly INoteRepository _noteRepository;
    private readonly ILogger<NoteBookService> _logger;

    public NoteBookService(
        INoteBookRepository notebookRepository,
        INoteRepository noteRepository,
        ILogger<NoteBookService> logger)
    {
        _notebookRepository = notebookRepository;
        _noteRepository = noteRepository;
        _logger = logger;
    }

    public async Task<Guid> CreateNoteBookAsync(NoteBookCreateRequestDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("NoteBook name is empty", nameof(dto.Name));

        if (dto.OwnerId == Guid.Empty)
            throw new ArgumentException("NoteBook must have an owner", nameof(dto.OwnerId));

        var notebook = new NoteBookEntity
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            NoteBookName = dto.Name,
            OwnerId = dto.OwnerId
        };

        await _notebookRepository.CreateAsync(notebook, ct);
        _logger.LogInformation("NoteBook with id: {NotebookId} created", notebook.Id);
        return notebook.Id;
    }

    public async Task<NoteBookEntity> GetNoteBookAsync(Guid id, CancellationToken ct)
    {
        var notebook = await _notebookRepository.GetByIdAsync(id, ct);
        if (notebook == null)
            throw new KeyNotFoundException($"NoteBook with id: {id} not found");
        return notebook;
    }

    public async Task<bool> UpdateNoteBookAsync(Guid id, string name, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("NoteBook name is empty", nameof(name));

        var notebook = await _notebookRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"NoteBook with id: {id} not found");

        notebook.NoteBookName = name;
        notebook.UpdatedAt = DateTime.UtcNow;

        await _notebookRepository.UpdateAsync(notebook, ct);
        _logger.LogInformation("NoteBook with id: {NotebookId} updated", id);
        return true;
    }

    public async Task<bool> DeleteNoteBookAsync(Guid id, CancellationToken ct)
    {
        _ = await _notebookRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"NoteBook with id: {id} not found");

        await _notebookRepository.DeleteAsync(id, ct);
        _logger.LogInformation("NoteBook with id: {NotebookId} deleted", id);
        return true;
    }

    public async Task<bool> AddNoteAsync(Guid noteId, CancellationToken ct)
    {
        var note = await _noteRepository.GetByIdAsync(noteId, ct)
            ?? throw new KeyNotFoundException($"Note with id: {noteId} not found");

        if (note.NotebookId == null)
            throw new InvalidOperationException($"Note {noteId} has no NotebookId set");

        return await _notebookRepository.AddNoteAsync(note.NotebookId.Value, note, ct);
    }
}