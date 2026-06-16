namespace SecondBrain.Services.UserService.Models; 

public record LoginRequestDto
{
    public string Password {get; init;}

    public string Email {get; init; }
}