namespace Medreserve.Features.Doctor;

public record CreateDoctorProfileDto(
    string LicenseNumber, 
    string? Bio,
    List<int>? SpecializationIds
);
public sealed record DoctorDto(int DoctorId, string UserId, string LicenseNumber, string? Bio);