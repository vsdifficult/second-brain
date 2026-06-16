
using SecondBrain.BuildingBlocks.Core.Entities; 

namespace SecondBrain.Services.UserService.Entites; 

public class UserEntity: BaseEntity
{
    public required string UserName {get; set; } 

    public required string PasswordHash {get; set; } 

    public required string Email {get; set; } 

    public bool IsActive { get; set; } = true;

    public DateTime? LastLoginAt { get; set; }
}