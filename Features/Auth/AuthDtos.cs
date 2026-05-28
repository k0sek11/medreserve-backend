namespace Medreserve.Features.Auth;

public record RegisterDto(string Email, string Password);
public record LoginDto(string Email, string Password);

public sealed record CompleteProfileDto(
    string ProfileType,
    string FirstName,
    string LastName,
    string PhoneNumber,
    DateOnly BirthDate,
    string Gender,
    string? LicenseNumber
);

public class GoogleLoginDto
{
    public string Token { get; set; } = string.Empty;
}

public record UserSessionDto(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    bool IsActive,
    IList<string> Roles,
    int? DoctorProfileId
);