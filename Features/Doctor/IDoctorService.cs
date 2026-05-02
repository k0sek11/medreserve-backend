namespace Medreserve.Features.Doctor;

public interface IDoctorService
{
    Task<bool> CreateProfileAsync(string userId, CreateDoctorProfileDto request);
}