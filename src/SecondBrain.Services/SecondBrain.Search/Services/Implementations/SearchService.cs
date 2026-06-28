using Microsoft.EntityFrameworkCore;
using SecondBrain.Services.SearchService.Data;
using SecondBrain.Services.SearchService.Services.Interfaces; 

namespace SecondBrain.Services.SearchService.Services.Implementations;

public class SearchService : ISearchService
{
    private readonly SearchDbContext _db;

    public SearchService(SearchDbContext db) => _db = db;

    public async Task<List<SearchResult>> SearchAsync(string query, Guid ownerId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return [];

        var q = query.Trim().ToLowerInvariant();

        // Простой LIKE-поиск — достаточно для старта.
        // Позже заменить на EF.Functions.FreeText() (SQL Server)
        // или EF.Functions.ToTsVector() (PostgreSQL).
        return await _db.SearchIndex
            .Where(x => x.OwnerId == ownerId &&
                        (EF.Functions.Like(x.Title.ToLower(), $"%{q}%") ||
                         EF.Functions.Like(x.Body.ToLower(), $"%{q}%") ||
                         EF.Functions.Like(x.Tags.ToLower(), $"%{q}%")))
            .OrderByDescending(x => x.IndexedAt)
            .Take(50)
            .Select(x => new SearchResult(x.NoteId, x.OwnerId, x.Title, x.Body, x.IndexedAt))
            .ToListAsync(ct);
    }
}