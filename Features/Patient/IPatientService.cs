namespace Medreserve.Features.Patient;

public interface IPatientService
{
    Task<bool> CreateProfileAsync(string userId, CreatePatientProfileDto request);
}