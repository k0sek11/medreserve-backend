namespace Medreserve.Features.Auth;

public interface IAuthService
{
    Task<bool> RegisterAsync(RegisterDto request);
    Task<bool> LoginAsync(LoginDto request);
    Task LogoutAsync();
    Task<bool> LoginWithGoogleAsync(string googleToken);
    Task<UserSessionDto?> GetCurrentUserAsync(string userId);
    Task<bool> CompleteProfileAsync(string userId, CompleteProfileDto request);
}
