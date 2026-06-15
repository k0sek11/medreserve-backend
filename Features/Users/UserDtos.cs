namespace Medreserve.Features.Users;

public sealed record UpdatePatientProfileDto(
    string FirstName,
    string LastName,
    string PhoneNumber,
    DateOnly BirthDate,
    string Gender
);

public sealed record PatientProfileDto(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    string PhoneNumber,
    DateOnly? BirthDate,
    string? Gender,
    bool IsActive
);