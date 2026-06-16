using Microsoft.AspNetCore.Mvc;
using SecondBrain.Services.UserService.Models;
using SecondBrain.Services.UserService.Services.Interfaces;

namespace SecondBrain.Services.UserService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    
    public AuthController(IUserService userService)
    {
        _userService = userService;
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequestDto request,
        CancellationToken cancellationToken) 
    {
        try
        {
            var user = await _userService.RegisterAsync(
                request,
                cancellationToken); 
            
            return Ok(new { user.Id, user.Email, user.UserName });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequestDto request,
        CancellationToken cancellationToken) 
    {
        try
        {
            var token = await _userService.LoginAsync(
                request,
                cancellationToken);
            
            return Ok(new { token });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { error = "Invalid credentials" });
        }
    }
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUser(
        Guid id,
        CancellationToken cancellationToken) 
    {
        try
        {
            var user = await _userService.GetByIdAsync(
                id,
                cancellationToken); 
            
            return Ok(new { user.Id, user.Email, user.UserName });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}