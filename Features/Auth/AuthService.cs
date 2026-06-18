using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Medreserve.Features.Doctor;
using Medreserve.Features.Users;

namespace Medreserve.Features.Auth;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IDoctorService _doctorService;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IDoctorService doctorService,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _doctorService = doctorService;
        _configuration = configuration;
    }

    public async Task<bool> RegisterAsync(RegisterDto request)
    {
        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
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

    public async Task<bool> LoginWithGoogleAsync(string googleToken)
{
    try
    {
        var googleClientId = _configuration["Authentication:Google:ClientId"];

                    if (string.IsNullOrEmpty(googleClientId))
                    {
                        Console.WriteLine("[Google OAuth Error] Brak ClientId w konfiguracji!");
                        return false;
                    }

        var settings = new GoogleJsonWebSignature.ValidationSettings()
        {
            Audience = new List<string>() { googleClientId }
        };

        var payload = await GoogleJsonWebSignature.ValidateAsync(googleToken, settings);

        var info = new UserLoginInfo("Google", payload.Subject, "Google");
        var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

        if (user == null)
        {
            user = await _userManager.FindByEmailAsync(payload.Email);

            if (user == null)
            {
                user = new User
                {
                    UserName = payload.Email,
                    Email = payload.Email,
                    FirstName = payload.GivenName ?? "", 
                    LastName = payload.FamilyName ?? "",
                    IsActive = false 
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded) return false;
            }

            var addLoginResult = await _userManager.AddLoginAsync(user, info);
            if (!addLoginResult.Succeeded) return false;
        }

        await _signInManager.SignInAsync(user, isPersistent: true);
        
        return true;
    }
    catch (InvalidJwtException ex)
    {
        Console.WriteLine($"[Google OAuth Error] JWT jest nieprawidłowy: {ex.Message}");
        return false;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Google OAuth Error] Inny błąd: {ex.Message}");
        return false;
    }
}

    public async Task<bool> CompleteProfileAsync(string userId, CompleteProfileDto request)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        if (!string.Equals(request.ProfileType, "Doctor", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(request.ProfileType, "Patient", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.PhoneNumber = request.PhoneNumber.Trim();
        user.BirthDate = request.BirthDate;
        user.Gender = request.Gender.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        if (!string.Equals(request.ProfileType, "Doctor", StringComparison.OrdinalIgnoreCase))
        {
            user.IsActive = true;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return false;
            }

            if (!await _userManager.IsInRoleAsync(user, "Patient"))
            {
                var roleResult = await _userManager.AddToRoleAsync(user, "Patient");
                if (!roleResult.Succeeded)
                {
                    return false;
                }
            }

            return true;
        }

        if (string.IsNullOrWhiteSpace(request.LicenseNumber))
        {
            return false;
        }

        var profileResult = await _doctorService.CreateProfileAsync(
            userId,
            new CreateDoctorProfileDto(request.LicenseNumber.Trim(), null, null)
        );

        if (!profileResult)
        {
            return false;
        }

        user.IsActive = true;
        var finalResult = await _userManager.UpdateAsync(user);
        return finalResult.Succeeded;
    }
}
