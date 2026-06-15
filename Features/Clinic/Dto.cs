namespace Medreserve.Features.Clinic;

public static class Dto
{
    public sealed record ClinicDto(
        int ClinicId,
        string Name,
        string? Description,
        string StreetAddress,
        string? OpeningHours,
        double? Latitude,
        double? Longitude,
        string? PhoneNumber,
        string? Email,
        bool IsActive
    );

    public sealed record CreateClinicRequest(
        string Name,
        string? Description,
        string StreetAddress,
        string? OpeningHours,
        double? Latitude,
        double? Longitude,
        string City,
        string? PhoneNumber,
        string? Email,
        bool IsActive
    );

    public sealed record UpdateClinicRequest(
        string? Name,
        string? Description,
        string? StreetAddress,
        string? OpeningHours,
        double? Latitude,
        double? Longitude,
        string? City,
        string? PhoneNumber,
        string? Email,
        bool? IsActive
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

    public sealed record ClinicListQuery
    {
        public string? Name { get; init; }
        public string? Location { get; init; }
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
        double? Latitude,
        double? Longitude,
        string? PhoneNumber,
        string? Email,
        string City,
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