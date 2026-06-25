using SecondBrain.BuildingBlocks.Core.Repositories;
using SecondBrain.BuildingBlocks.EFCore;
using SecondBrain.Services.BrainService.Entities; 
using SecondBrain.Services.BrainService.Data; 
using Microsoft.EntityFrameworkCore; 


namespace SecondBrain.Services.BrainService.Data.Repositories; 

public interface INoteRepository : IRepository<NoteEntity, Guid>
{
}

public class NoteRepository: GenericRepository<NoteEntity, Guid>, INoteRepository
{
    private readonly BrainDbContext _dbContext;

    public NoteRepository(BrainDbContext context) : base(context)
    {
        _dbContext = context;
    }
}

