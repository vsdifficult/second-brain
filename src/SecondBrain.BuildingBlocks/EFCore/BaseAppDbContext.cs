using Microsoft.EntityFrameworkCore; 
using SecondBrain.BuildingBlocks.Core.Entities; 

namespace SecondBrain.BuildingBlocks.EFCore; 

public abstract class BaseBbContext : DbContext
{
    protected BaseBbContext(DbContextOptions options) : base(options) {}

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt = utcNow;
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = utcNow;
        }

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt = utcNow;
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = utcNow;
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}