
namespace SecondBrain.BuildingBlocks.Core.Entities; 

public class BaseEntity
{
    
    public required Guid Id {get; set; } 

    public required DateTime CreateAt {get; set; } 

    public required DateTime UpdateAt {get; set; } 

}