namespace SecondBrain.Services.UserService.Models; 

public record RegisterRequestDto
{
    public string UserName {get; init;} 

    public string Password {get; init;}

    public string Email {get; init; }
}