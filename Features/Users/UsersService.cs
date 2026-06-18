using Microsoft.AspNetCore.Identity;

namespace Medreserve.Features.Users;

public class UsersService : IUsersService
{
    private readonly UserManager<User> _userManager;

    public UsersService(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<PatientProfileDto?> GetMyProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return null;

        return new PatientProfileDto(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            user.PhoneNumber ?? "",
            user.BirthDate,
            user.Gender,
            user.IsActive
        );
    }

    public async Task<bool> UpdateMyProfileAsync(string userId, UpdatePatientProfileDto request)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return false;

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        if (request.PhoneNumber is not null)
            user.PhoneNumber = request.PhoneNumber.Trim();
        if (request.BirthDate is not null)
            user.BirthDate = request.BirthDate;
        if (request.Gender is not null)
            user.Gender = request.Gender.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }
}
