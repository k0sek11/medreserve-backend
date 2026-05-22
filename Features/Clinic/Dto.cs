namespace Medreserve.Features.Clinic;

public class Dto
{
    
public sealed record ClinicDto(
    int ClinicId,
    string Name,
    string? Description,
    string StreetAddress,
    string? OpeningHours,
    string? MapLocation,
    string? PhoneNumber,
    string? Email,
    bool IsActive
);

public sealed record CreateClinicRequest(
    string Name,
    string? Description,
    string StreetAddress,
    string? OpeningHours,
    string? MapLocation,
    int CityId,
    string? PhoneNumber,
    string? Email,
    bool IsActive
);

public sealed record UpdateClinicRequest(
    string Name,
    string? Description,
    string StreetAddress,
    string? OpeningHours,
    string? MapLocation,
    int CityId,
    string? PhoneNumber,
    string? Email,
    bool IsActive
);

public sealed record CityDto(
    int CityId,
    string Name,
    string District,
    string Voivodeship
);

public sealed record SpecializationDto(
    int SpecializationId,
    string Name,
    string? Description
);

public sealed record PagedResultDto<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount, int TotalPages);

public sealed record ClinicSearchQuery(
    string? Name,
    string? Location,
    int? CityId,
    int? SpecializationId,
    string? Sort,
    int Page = 1,
    int PageSize = 6
);

public sealed record ClinicListItemDto(
    int ClinicId,
    string Name,
    string StreetAddress,
    string City,
    int DoctorCount,
    IReadOnlyList<string> Specializations,
    bool IsActive,
    bool IsOwner
);

public sealed record ClinicDetailDto(
    int ClinicId,
    string Name,
    string? Description,
    string StreetAddress,
    string? OpeningHours,
    string? MapLocation,
    string? PhoneNumber,
    string? Email,
    int CityId,
    string City,
    string District,
    string Voivodeship,
    bool IsActive,
    int DoctorCount,
    IReadOnlyList<string> Specializations,
    IReadOnlyList<ClinicDoctorSummaryDto> Doctors,
    bool IsCurrentUserMember,
    bool IsCurrentUserOwner
);

public sealed record ClinicDoctorSummaryDto(
    int DoctorId,
    string FullName,
    string PrimarySpecialization,
    bool IsOwner
);

public sealed record CreateClinicJoinRequestDto(
    bool ConfirmDoctor,
    string? Message
);

}