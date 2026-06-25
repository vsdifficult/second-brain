using Microsoft.EntityFrameworkCore;
using SecondBrain.BuildingBlocks.Core.Repositories;
using SecondBrain.BuildingBlocks.EFCore;
using SecondBrain.Services.BrainService.Entities;

namespace SecondBrain.Services.BrainService.Data.Repositories;

public interface ITagRepository : IRepository<TagEntity, Guid>
{
    Task<TagEntity?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<NoteTagEntity?> GetNoteTagAsync(Guid noteId, Guid tagId, CancellationToken ct = default);
    Task AddNoteTagAsync(NoteTagEntity noteTag, CancellationToken ct = default);
    Task RemoveNoteTagAsync(NoteTagEntity noteTag, CancellationToken ct = default);
    Task<List<TagEntity>> GetTagsByNoteIdAsync(Guid noteId, CancellationToken ct = default);
}

public class TagRepository : GenericRepository<TagEntity, Guid>, ITagRepository
{
    private readonly BrainDbContext _dbContext;

    public TagRepository(BrainDbContext context) : base(context)
    {
        _dbContext = context;
    }

    public async Task<TagEntity?> GetByNameAsync(string name, CancellationToken ct = default)
        => await _dbContext.Tags.FirstOrDefaultAsync(t => t.Name == name, ct);

    public async Task<NoteTagEntity?> GetNoteTagAsync(Guid noteId, Guid tagId, CancellationToken ct = default)
        => await _dbContext.NoteTags
            .FirstOrDefaultAsync(nt => nt.NoteId == noteId && nt.TagId == tagId, ct);

    public async Task AddNoteTagAsync(NoteTagEntity noteTag, CancellationToken ct = default)
    {
        await _dbContext.NoteTags.AddAsync(noteTag, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task RemoveNoteTagAsync(NoteTagEntity noteTag, CancellationToken ct = default)
    {
        _dbContext.NoteTags.Remove(noteTag);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<List<TagEntity>> GetTagsByNoteIdAsync(Guid noteId, CancellationToken ct = default)
        => await _dbContext.NoteTags
            .Where(nt => nt.NoteId == noteId)
            .Select(nt => nt.Tag)
            .ToListAsync(ct);
}