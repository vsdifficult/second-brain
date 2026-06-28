using SecondBrain.BuildingBlocks.Messaging.Kafka.Abstractions;
using SecondBrain.BuildingBlocks.Messaging.Kafka.Events;
using SecondBrain.Services.SearchService.Data;
using SecondBrain.Services.SearchService.Entities;
using Microsoft.EntityFrameworkCore;

namespace SecondBrain.Services.SearchService.Handlers;

public class NoteCreatedHandler : IIntegrationEventHandler<NoteCreatedEvent>
{
    private readonly SearchDbContext _db;
    private readonly ILogger<NoteCreatedHandler> _logger;

    public NoteCreatedHandler(SearchDbContext db, ILogger<NoteCreatedHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task HandleAsync(NoteCreatedEvent @event, CancellationToken ct = default)
    {
        var existing = await _db.SearchIndex
            .FirstOrDefaultAsync(x => x.NoteId == @event.NoteId, ct);

        if (existing is not null)
        {
            _logger.LogWarning("SearchIndex already contains NoteId {NoteId}, skipping", @event.NoteId);
            return; 
        }

        var entry = new SearchIndexEntry
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            NoteId = @event.NoteId,
            OwnerId = @event.OwnerId,
            Title = @event.Title,
            Body = @event.Body,
            Tags = System.Text.Json.JsonSerializer.Serialize(@event.Tags),
            IndexedAt = DateTime.UtcNow
        };

        _db.SearchIndex.Add(entry);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Indexed Note {NoteId}", @event.NoteId);
    }
}

