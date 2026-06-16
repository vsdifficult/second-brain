using System.ComponentModel.DataAnnotations;

namespace SecondBrain.Services.UserService.Models;

public record LoginRequestDto
{
    [Required]
    public string Password { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;
}