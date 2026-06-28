using SecondBrain.BuildingBlocks.Messaging.Kafka.Abstractions;
using SecondBrain.BuildingBlocks.Messaging.Kafka.Events;
using SecondBrain.Services.SearchService.Data;
using SecondBrain.Services.SearchService.Entities;
using Microsoft.EntityFrameworkCore;

namespace SecondBrain.Services.SearchService.Handlers;
public class NoteDeletedHandler : IIntegrationEventHandler<NoteDeletedEvent>
{
    private readonly SearchDbContext _db;
    private readonly ILogger<NoteDeletedHandler> _logger;

    public NoteDeletedHandler(SearchDbContext db, ILogger<NoteDeletedHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task HandleAsync(NoteDeletedEvent @event, CancellationToken ct = default)
    {
        var entry = await _db.SearchIndex
            .FirstOrDefaultAsync(x => x.NoteId == @event.NoteId, ct);

        if (entry is null) return; // Уже удалено — idempotent

        _db.SearchIndex.Remove(entry);
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Removed Note {NoteId} from index", @event.NoteId);
    }
}