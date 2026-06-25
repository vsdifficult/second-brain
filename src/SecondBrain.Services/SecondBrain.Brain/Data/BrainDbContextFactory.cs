using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql.EntityFrameworkCore.PostgreSQL; 

namespace SecondBrain.Services.BrainService.Data;

public class BrainDbContextFactory : IDesignTimeDbContextFactory<BrainDbContext>
{
    public BrainDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BrainDbContext>();

        var connectionString = Environment.GetEnvironmentVariable("BRAIN_DB")
            ?? "Host=localhost;Port=5432;Database=secondbrain;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(connectionString);

        return new BrainDbContext(optionsBuilder.Options);
    }
}