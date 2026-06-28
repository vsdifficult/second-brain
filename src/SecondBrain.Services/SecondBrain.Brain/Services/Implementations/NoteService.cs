using SecondBrain.BuildingBlocks.Core.Repositories;
using SecondBrain.BuildingBlocks.Infrastructure.Messaging.Topics;
using SecondBrain.BuildingBlocks.Messaging.Kafka.Events;
using SecondBrain.Services.BrainService.Entities;
using SecondBrain.Services.BrainService.Models;
using SecondBrain.Services.BrainService.Services.Interfaces;

namespace SecondBrain.Services.BrainService.Services.Implementations; 
public class NoteService : INoteService
{
    private readonly IRepository<NoteEntity, Guid> _noteRepository;
    private readonly ILogger<NoteService> _logger;

    public NoteService(IRepository<NoteEntity, Guid> noteRepository, ILogger<NoteService> logger)
    {
        _noteRepository = noteRepository;
        _logger = logger;
    }

    public async Task<Guid> CreateNoteAsync(NoteCreateRequestDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new ArgumentException("Title is empty", nameof(dto.Title));
        if (string.IsNullOrWhiteSpace(dto.Body))
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

        // Enqueue событие в той же транзакции
        _noteRepository.EnqueueOutboxMessage(
            TopicNames.NoteCreated,
            note.Id.ToString(),
            new NoteCreatedEvent
            {
                Id = Guid.NewGuid(),
                CreationDate = DateTime.UtcNow,
                NoteId = note.Id,
                Title = note.Title,
                Body = note.Body,
                OwnerId = note.OwnerId,
                NotebookId = note.NotebookId
            });

        await _noteRepository.SaveChangesAsync(ct); // Одна транзакция: Note + OutboxMessage

        _logger.LogInformation("Note {NoteId} created", note.Id);
        return note.Id;
    }

    public async Task<bool> UpdateNoteAsync(Guid id, string content, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content is empty");

        var note = await _noteRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Note {id} not found");

        note.Body = content;
        note.UpdatedAt = DateTime.UtcNow;

        await _noteRepository.UpdateAsync(note, ct);

        _noteRepository.EnqueueOutboxMessage(
            TopicNames.NoteUpdated,
            note.Id.ToString(),
            new NoteUpdatedEvent
            {
                Id = Guid.NewGuid(),
                CreationDate = DateTime.UtcNow,
                NoteId = note.Id,
                Body = note.Body,
                OwnerId = note.OwnerId
            });

        await _noteRepository.SaveChangesAsync(ct);

        _logger.LogInformation("Note {NoteId} updated", id);
        return true;
    }

    public async Task<bool> DeleteNoteAsync(Guid id, CancellationToken ct = default)
    {
        var note = await _noteRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Note {id} not found");

        _noteRepository.EnqueueOutboxMessage(
            TopicNames.NoteDeleted,
            note.Id.ToString(),
            new NoteDeletedEvent
            {
                Id = Guid.NewGuid(),
                CreationDate = DateTime.UtcNow,
                NoteId = note.Id,
                OwnerId = note.OwnerId
            });

        await _noteRepository.DeleteAsync(id, ct);
        await _noteRepository.SaveChangesAsync(ct);

        _logger.LogInformation("Note {NoteId} deleted", id);
        return true;
    }

    public async Task<NoteEntity> GetNoteAsync(Guid id, CancellationToken ct = default)
    {
        return await _noteRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Note {id} not found");
    }
}