namespace SecondBrain.BuildingBlocks.Core.Entities; 

public abstract class AuditableEntity : BaseEntity
{
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; } // Soft Delete
}