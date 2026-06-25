using SecondBrain.Services.BrainService.Data.Repositories;
using SecondBrain.Services.BrainService.Entities;
using SecondBrain.Services.BrainService.Models;
using SecondBrain.Services.BrainService.Services.Interfaces;

namespace SecondBrain.Services.BrainService.Services.Implementations;

public class TagService : ITagService
{
    private readonly ITagRepository _tagRepository;
    private readonly INoteRepository _noteRepository;
    private readonly ILogger<TagService> _logger;

    public TagService(
        ITagRepository tagRepository,
        INoteRepository noteRepository,
        ILogger<TagService> logger)
    {
        _tagRepository = tagRepository;
        _noteRepository = noteRepository;
        _logger = logger;
    }

    public async Task<Guid> CreateTagAsync(TagCreate dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Tag name is empty", nameof(dto.Name));

        var existing = await _tagRepository.GetByNameAsync(dto.Name, ct);
        if (existing != null)
            throw new InvalidOperationException($"Tag '{dto.Name}' already exists");

        var tag = new TagEntity
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Name = dto.Name
        };

        await _tagRepository.CreateAsync(tag, ct);
        _logger.LogInformation("Tag with id: {TagId} created", tag.Id);
        return tag.Id;
    }

    public async Task<TagEntity> GetTagAsync(Guid id, CancellationToken ct = default)
    {
        var tag = await _tagRepository.GetByIdAsync(id, ct);
        if (tag == null)
            throw new KeyNotFoundException($"Tag with id: {id} not found");
        return tag;
    }

    public async Task<List<TagEntity>> GetTagsByNoteAsync(Guid noteId, CancellationToken ct = default)
        => await _tagRepository.GetTagsByNoteIdAsync(noteId, ct);

    public async Task<bool> ApplyTagToNoteAsync(ApplyTagToNote dto, CancellationToken ct = default)
    {
        var note = await _noteRepository.GetByIdAsync(dto.NoteId, ct)
            ?? throw new KeyNotFoundException($"Note with id: {dto.NoteId} not found");

        var tag = await _tagRepository.GetByIdAsync(dto.TagId, ct)
            ?? throw new KeyNotFoundException($"Tag with id: {dto.TagId} not found");

        var existing = await _tagRepository.GetNoteTagAsync(dto.NoteId, dto.TagId, ct);
        if (existing != null)
            throw new InvalidOperationException("Tag is already applied to this note");

        var noteTag = new NoteTagEntity
        {
            NoteId = dto.NoteId,
            TagId = dto.TagId
        };

        await _tagRepository.AddNoteTagAsync(noteTag, ct);
        _logger.LogInformation("Tag {TagId} applied to Note {NoteId}", dto.TagId, dto.NoteId);
        return true;
    }

    public async Task<bool> RemoveTagFromNoteAsync(ApplyTagToNote dto, CancellationToken ct = default)
    {
        var noteTag = await _tagRepository.GetNoteTagAsync(dto.NoteId, dto.TagId, ct)
            ?? throw new KeyNotFoundException($"Tag {dto.TagId} is not applied to note {dto.NoteId}");

        await _tagRepository.RemoveNoteTagAsync(noteTag, ct);
        _logger.LogInformation("Tag {TagId} removed from Note {NoteId}", dto.TagId, dto.NoteId);
        return true;
    }

    public async Task<bool> DeleteTagAsync(Guid id, CancellationToken ct = default)
    {
        var tag = await _tagRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Tag with id: {id} not found");

        await _tagRepository.DeleteAsync(id, ct);
        _logger.LogInformation("Tag with id: {TagId} deleted", id);
        return true;
    }
}