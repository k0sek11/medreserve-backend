using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Medreserve.Features.Users;

namespace Medreserve.Features.Auth;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public AuthService(UserManager<User> userManager, SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<bool> RegisterAsync(RegisterDto request)
    {
        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,

            IsActive = false
        };

        var result = await _userManager.CreateAsync(user, request.Password);


        if (!result.Succeeded)
        {
            return false;
        }

        return true;
    }

    public async Task<bool> LoginAsync(LoginDto request)
    {
        var result = await _signInManager.PasswordSignInAsync(
            request.Email,
            request.Password,
            isPersistent: true,
            lockoutOnFailure: false);

        return result.Succeeded;
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }

    public async Task<UserSessionDto?> GetCurrentUserAsync(string userId)
    {
        var user = await _userManager.Users
            .Include(x => x.DoctorProfile)
            .FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null) return null;

        var roles = await _userManager.GetRolesAsync(user);

        return new UserSessionDto(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            user.IsActive,
            roles,
            user.DoctorProfile?.DoctorId
        );
    }
}