using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Medreserve.Features.Auth;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto request)
    {
        var success = await _authService.RegisterAsync(request);
        
        if (!success)
        {
            return BadRequest(new { message = "Registration failed. Check your data." });
        }
        
        return Ok(new { message = "User registered successfully." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto request)
    {
        var success = await _authService.LoginAsync(request);
        
        if (!success)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        return Ok(new { message = "Logged in successfully." });
    }
    
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (userId == null) return Unauthorized();

        var sessionData = await _authService.GetCurrentUserAsync(userId);
        
        if (sessionData == null) return Unauthorized();

        return Ok(sessionData);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync();
        return Ok(new { message = "Logged out successfully." });
    }
}