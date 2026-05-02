namespace Medreserve.Features.Auth;

public interface IAuthService
{
    Task<bool> RegisterAsync(RegisterDto request);
    Task<bool> LoginAsync(LoginDto request);
    Task LogoutAsync();
    
    Task<UserSessionDto?> GetCurrentUserAsync(string userId);
}