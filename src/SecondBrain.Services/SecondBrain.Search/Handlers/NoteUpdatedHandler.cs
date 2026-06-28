using SecondBrain.BuildingBlocks.Messaging.Kafka.Abstractions;
using SecondBrain.BuildingBlocks.Messaging.Kafka.Events;
using SecondBrain.Services.SearchService.Data;
using SecondBrain.Services.SearchService.Entities;
using Microsoft.EntityFrameworkCore;

namespace SecondBrain.Services.SearchService.Handlers;

public class NoteUpdatedHandler : IIntegrationEventHandler<NoteUpdatedEvent>
{
    private readonly SearchDbContext _db;
    private readonly ILogger<NoteUpdatedHandler> _logger;

    public NoteUpdatedHandler(SearchDbContext db, ILogger<NoteUpdatedHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task HandleAsync(NoteUpdatedEvent @event, CancellationToken ct = default)
    {
        var entry = await _db.SearchIndex
            .FirstOrDefaultAsync(x => x.NoteId == @event.NoteId, ct);

        if (entry is null)
        {
            _logger.LogWarning("SearchIndex: NoteId {NoteId} not found for update", @event.NoteId);
            return;
        }

        entry.Body = @event.Body;
        entry.UpdatedAt = DateTime.UtcNow;
        entry.IndexedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Re-indexed Note {NoteId}", @event.NoteId);
    }
}

