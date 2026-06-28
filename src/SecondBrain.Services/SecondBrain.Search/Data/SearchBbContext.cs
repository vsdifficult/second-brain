using Microsoft.EntityFrameworkCore;
using SecondBrain.BuildingBlocks.EFCore;
using SecondBrain.Services.SearchService.Entities;

namespace SecondBrain.Services.SearchService.Data;

public class SearchDbContext : BaseDbContext
{
    public DbSet<SearchIndexEntry> SearchIndex { get; set; }

    public SearchDbContext(DbContextOptions<SearchDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SearchIndexEntry>(e =>
        {
            e.HasIndex(x => x.NoteId).IsUnique();
            e.HasIndex(x => x.OwnerId);
            e.HasIndex(x => new { x.Title, x.Body })
                .HasMethod("gin")
                .IsTsVectorExpressionIndex("english");
        });

        base.OnModelCreating(modelBuilder);
    }
}