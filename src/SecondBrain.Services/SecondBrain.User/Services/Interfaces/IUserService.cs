using SecondBrain.Services.UserService.Entites;
using SecondBrain.Services.UserService.Models;

namespace SecondBrain.Services.UserService.Services.Interfaces; 

public interface IUserService
{
    Task<UserEntity> RegisterAsync(RegisterRequestDto dto, CancellationToken ct = default);
    Task<string> LoginAsync(LoginRequestDto dto, CancellationToken ct = default);
    Task<UserEntity> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<UserEntity> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> ValidateUserAsync(Guid userId, CancellationToken ct = default);
}