using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using SecondBrain.Services.UserService.Entites; 
using SecondBrain.BuildingBlocks.Core.Repositories;
using SecondBrain.Services.UserService.Services.Interfaces;
using SecondBrain.Services.UserService.Models;

namespace SecondBrain.Services.UserService.Services.Implementations;

public class UserService : IUserService
{
    private readonly IRepository<UserEntity, Guid> _userRepository;
    private readonly JwtSettings _jwtSettings;
    
    public UserService(
        IRepository<UserEntity, Guid> userRepository,
        IOptions<JwtSettings> jwtSettings)
    {
        _userRepository = userRepository;
        _jwtSettings = jwtSettings.Value;
    }
    
    public async Task<UserEntity> RegisterAsync(RegisterRequestDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
            throw new InvalidOperationException("Password must be at least 6 characters long");

        if (!dto.Password.Any(char.IsDigit))
            throw new InvalidOperationException("Password must contain at least one digit");

        if (!dto.Password.Any(char.IsUpper))
            throw new InvalidOperationException("Password must contain at least one uppercase letter");

        if (await _userRepository.ExistsAsync(u => u.Email == dto.Email, ct))
            throw new InvalidOperationException("User with this email already exists");

        if (await _userRepository.ExistsAsync(u => u.UserName == dto.UserName, ct))
            throw new InvalidOperationException("User with this username already exists");

        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = dto.Email,
            UserName = dto.UserName,
            PasswordHash = HashPassword(dto.Password),
            IsActive = true,
            CreatedAt = DateTime.UtcNow, 
            UpdatedAt = DateTime.UtcNow
        };
        
        await _userRepository.CreateAsync(user, ct);
        await _userRepository.SaveChangesAsync(ct);
        
        return user;
    }
    
    public async Task<string> LoginAsync(LoginRequestDto dto, CancellationToken ct = default)
    {
        var users = await _userRepository.FindAsync(u => u.Email == dto.Email, ct);
        var userEntity = users.FirstOrDefault();
        
        if (userEntity == null || !VerifyPassword(dto.Password, userEntity.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials");
            
        if (!userEntity.IsActive)
            throw new UnauthorizedAccessException("User account is inactive");
            
        userEntity.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(userEntity, ct);
        await _userRepository.SaveChangesAsync(ct);
        
        return GenerateJwtToken(userEntity);
    }
    
    public async Task<UserEntity> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(id, ct);
        if (user == null)
            throw new KeyNotFoundException($"User with id {id} not found");
        return user;
    }
    
    public async Task<UserEntity> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var users = await _userRepository.FindAsync(u => u.Email == email, ct);
        var user = users.FirstOrDefault();
        if (user == null)
            throw new KeyNotFoundException($"User with email {email} not found");
        return user;
    }
    
    public async Task<bool> ValidateUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct);
        return user != null && user.IsActive;
    }
    
    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }
    
    private bool VerifyPassword(string password, string hash)
    {
        // BCrypt hashes always start with $2
        if (hash.StartsWith("$2"))
            return BCrypt.Net.BCrypt.Verify(password, hash);

        // legacy SHA256 fallback — remove once all users have migrated
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var legacyHash = Convert.ToBase64String(sha.ComputeHash(bytes));
        return legacyHash == hash;
    }
    private string GenerateJwtToken(UserEntity user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.UserName)
        };
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(_jwtSettings.ExpiryHours),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}