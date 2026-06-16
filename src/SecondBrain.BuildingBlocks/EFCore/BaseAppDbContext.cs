using Microsoft.EntityFrameworkCore; 
using SecondBrain.BuildingBlocks.Core.Entities; 

namespace SecondBrain.BuildingBlocks.EFCore; 

public abstract class BaseBbContext: DbContext
{
    protected BaseBbContext(DbContextOptions options) : base(options) {}

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added) 
                entry.Entity.CreateAt = DateTime.UtcNow; 
            if (entry.State == EntityState.Modified) 
                entry.Entity.UpdateAt = DateTime.UtcNow;  
        }
        return await base.SaveChangesAsync(cancellationToken);
    }
}