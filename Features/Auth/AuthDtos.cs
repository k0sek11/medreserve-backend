namespace Medreserve.Features.Auth;

public record RegisterDto(string Email, string Password, string FirstName, string LastName);
public record LoginDto(string Email, string Password);

public record UserSessionDto(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    bool IsActive,
    IList<string> Roles,
    int? DoctorProfileId
);