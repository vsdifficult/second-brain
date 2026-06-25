using System.ComponentModel.DataAnnotations;

namespace SecondBrain.Services.UserService.Models;

public record RegisterRequestDto
{
    [Required]
    [MinLength(3)]
    public string UserName { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;
}