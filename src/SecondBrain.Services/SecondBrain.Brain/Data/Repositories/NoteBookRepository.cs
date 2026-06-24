using SecondBrain.BuildingBlocks.Core.Repositories;
using SecondBrain.BuildingBlocks.EFCore;
using SecondBrain.Services.BrainService.Entities; 
using SecondBrain.Services.BrainService.Data; 
using Microsoft.EntityFrameworkCore;

namespace SecondBrain.Services.BrainService.Data.Repositories; 

public interface INoteBookRepository : IRepository<NoteBookEntity, Guid>
{
    Task<bool> AddNoteAsync(Guid notebookId, NoteEntity note, CancellationToken ct);
}

public class NoteBookRepository: GenericRepository<NoteBookEntity, Guid>, INoteBookRepository
{
    private readonly BrainDbContext _dbContext;

    public NoteBookRepository(BrainDbContext context) : base(context)
    {
        _dbContext = context;
    }

    public async Task<bool> AddNoteAsync(Guid notebookId, NoteEntity note, CancellationToken ct)
    {
        if (note == null) throw new ArgumentNullException(nameof(note));

        var notebook = await _dbContext.NoteBooks.FirstOrDefaultAsync(nb => nb.Id == notebookId, ct);
        if (notebook == null)
            throw new Exception($"NoteBook with id: {notebookId} not found");

        // Ensure note has an Id
        if (note.Id == Guid.Empty) note.Id = Guid.NewGuid();

        // Ensure owner consistency: if note.OwnerId not set, inherit from notebook; otherwise require match
        if (note.OwnerId == Guid.Empty)
            note.OwnerId = notebook.OwnerId;
        else if (note.OwnerId != notebook.OwnerId)
            throw new InvalidOperationException("Note owner does not match notebook owner");

        note.NotebookId = notebookId;

        // Keep in-memory relationship consistent
        notebook.Notes.Add(note);

        await _dbContext.Notes.AddAsync(note, ct);
        var result = await _dbContext.SaveChangesAsync(ct);
        return result > 0;
    }
}

