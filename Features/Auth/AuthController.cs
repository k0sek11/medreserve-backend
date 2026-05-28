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
    
    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto request)
    {
        var success = await _authService.LoginWithGoogleAsync(request.Token);
    
        if (!success)
        {
            return Unauthorized("Nieprawidłowy token Google lub błąd po stronie serwera.");
        }

        return Ok();
    }

    [HttpPost("complete-profile")]
    [Authorize]
    public async Task<IActionResult> CompleteProfile([FromBody] CompleteProfileDto request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            return Unauthorized();
        }

        var success = await _authService.CompleteProfileAsync(userId, request);

        if (!success)
        {
            return BadRequest(new { message = "Nie udało się zapisać profilu." });
        }

        return Ok(new { message = "Profile completed successfully." });
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