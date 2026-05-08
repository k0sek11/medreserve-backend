namespace Medreserve.Features.Doctor;

public record CreateDoctorProfileDto(
    string LicenseNumber,
    string? Bio,
    List<int>? SpecializationIds
);
public sealed record DoctorDto(int DoctorId, string UserId, string LicenseNumber, string? Bio);

public sealed record DoctorSearchQueryDto(
    int? CityId,
    int? SpecializationId,
    DateOnly? Date,
    decimal? PriceMax,
    string? Sort,
    int Page = 1,
    int PageSize = 8
);

public sealed record DoctorSearchItemDto(
    int DoctorId,
    string FullName,
    string City,
    string Specialization,
    decimal LowestPrice,
    double? Rating
);

public sealed record PagedResultDto<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages
);