namespace Medreserve.Features.Clinic;

public static class Dto
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
        string? Name,
        string? Description,
        string? StreetAddress,
        string? OpeningHours,
        string? MapLocation,
        int? CityId,
        string? PhoneNumber,
        string? Email,
        bool? IsActive
    );

    public sealed record CityDto(
        int CityId,
        string Name,
        string District,
        string Voivodeship
    );

    public sealed record ClinicSpecializationDto(
        int SpecializationId,
        string Name,
        string? Description
    );

    public sealed record PagedResultDto<T>(
        IReadOnlyList<T> Items,
        int Page,
        int PageSize,
        int TotalCount,
        int TotalPages
    );

    /// <summary>
    /// Consolidated query for GET /api/clinics.
    /// Supports three views via the <c>View</c> parameter:
    /// <list type="bullet">
    ///   <item><c>clinics</c> (default) – paginated list of clinics with filters.</item>
    ///   <item><c>cities</c> – flat list of cities (optionally filtered by specializationId).</item>
    ///   <item><c>specializations</c> – flat list of specializations (optionally filtered by cityId).</item>
    /// </list>
    /// </summary>
    public sealed record ClinicListQuery
    {
        public string? Name { get; init; }
        public string? Location { get; init; }
        public int? CityId { get; init; }
        public int? SpecializationId { get; init; }
        public string? Sort { get; init; }
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 6;
        public string? View { get; init; }
    }

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