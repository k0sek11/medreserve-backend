namespace Medreserve.Features.AppointmentType;

public sealed record AppointmentTypeDto(
    int AppointmentTypeId,
    string Name,
    string? Description,
    decimal BasePrice,
    int DurationMinutes
);

public sealed record CreateAppointmentTypeRequest(
    string Name,
    string? Description,
    decimal BasePrice,
    int DurationMinutes
);

public sealed record UpdateAppointmentTypeRequest(
    string Name,
    string? Description,
    decimal BasePrice,
    int DurationMinutes
);
