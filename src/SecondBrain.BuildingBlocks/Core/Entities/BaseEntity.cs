
namespace SecondBrain.BuildingBlocks.Core.Entities; 

public class BaseEntity
{
    
    public required Guid Id {get; set; } 

    public required DateTime CreatedAt {get; set; } 

    public required DateTime UpdateAt {get; set; } 

}