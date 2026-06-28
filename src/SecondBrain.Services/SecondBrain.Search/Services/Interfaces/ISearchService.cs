
namespace SecondBrain.Services.SearchService.Services.Interfaces;

public record SearchResult(Guid NoteId, Guid OwnerId, string Title, string Body, DateTime IndexedAt);

public interface ISearchService
{
    Task<List<SearchResult>> SearchAsync(string query, Guid ownerId, CancellationToken ct = default);
}
