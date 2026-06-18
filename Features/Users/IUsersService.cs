namespace Medreserve.Features.Users;

public interface IUsersService
{
    Task<PatientProfileDto?> GetMyProfileAsync(string userId);
    Task<bool> UpdateMyProfileAsync(string userId, UpdatePatientProfileDto request);
}
