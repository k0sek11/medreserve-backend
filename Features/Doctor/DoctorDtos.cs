namespace Medreserve.Features.Doctor;

public record CreateDoctorProfileDto(
    string LicenseNumber,
    string? Bio,
    List<int>? SpecializationIds
);
public sealed record DoctorDto(int DoctorId, string UserId, string LicenseNumber, string? Bio);

public sealed record DoctorAppointmentTypeDto(
    int AppointmentTypeId,
    string Name,
    string? Description,
    decimal BasePrice,
    int DurationMinutes
);

public sealed record DoctorClinicDto(
    int ClinicId,
    string Name,
    string City,
    string StreetAddress
);

public sealed record DoctorScheduleDto(
    int ScheduleId,
    int? ClinicId,
    string? ClinicName,
    int DayOfWeek,
    string StartTime,
    string EndTime,
    DateOnly ValidFrom,
    DateOnly? ValidTo,
    bool IsActive
);

public sealed record UpsertDoctorScheduleDto(
    int? ScheduleId,
    int ClinicId,
    int DayOfWeek,
    string StartTime,
    string EndTime,
    DateOnly ValidFrom,
    DateOnly? ValidTo,
    bool IsActive
);

public sealed record UpdateDoctorProfileDto(string? Bio);

public sealed record DoctorPublicProfileDto(
    int DoctorId,
    string FullName,
    string LicenseNumber,
    string? Bio,
    string? PhoneNumber,
    string? City,
    string? StreetAddress,
    double? Rating,
    IReadOnlyList<string> Specializations,
    IReadOnlyList<DoctorAppointmentTypeDto> AppointmentTypes,
    IReadOnlyList<DoctorClinicDto> Clinics
);

public sealed record DoctorProfileDto(
    int DoctorId,
    string FullName,
    string LicenseNumber,
    string? Bio,
    string? PhoneNumber,
    string? City,
    string? StreetAddress,
    double? Rating,
    IReadOnlyList<string> Specializations,
    IReadOnlyList<DoctorAppointmentTypeDto> AppointmentTypes,
    IReadOnlyList<DoctorScheduleDto> Schedules,
    IReadOnlyList<DoctorClinicDto> Clinics
);

public sealed record DoctorAvailabilitySlotDto(
    string StartAt,
    string EndAt,
    int TimeSlotId,
    bool IsBooked
);

public sealed record DoctorAvailabilityDto(
    int DoctorId,
    DateOnly Date,
    int AppointmentTypeId,
    int? ClinicId,
    string AppointmentTypeName,
    int DurationMinutes,
    IReadOnlyList<DoctorAvailabilitySlotDto> Slots
);

public sealed record DoctorAvailabilityCalendarDto(
    int DoctorId,
    int Year,
    int Month,
    int AppointmentTypeId,
    int? ClinicId,
    IReadOnlyList<DateOnly> AvailableDates
);

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