
using Microsoft.EntityFrameworkCore;
using SecondBrain.BuildingBlocks.EFCore; 
using SecondBrain.Services.UserService.Entites; 

namespace SecondBrain.Services.UserService.Data; 

public class UserDbContext: BaseDbContext
{
    public DbSet<UserEntity> Users {get; set; }
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) {}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntity>()
            .HasIndex(u => u.Email)
            .IsUnique(); 

        modelBuilder.Entity<UserEntity>()
            .HasIndex(u => u.UserName)
            .IsUnique(); 
            
        base.OnModelCreating(modelBuilder);
    }

}