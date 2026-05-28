namespace Medreserve.Features.Notification;

public static class NotificationKinds
{
    public const string ClinicJoinRequest = "ClinicJoinRequest";
    public const string AppointmentBooked = "AppointmentBooked";
    public const string AppointmentStatusChanged = "AppointmentStatusChanged";
}

public sealed record ClinicJoinRequestPayload(
    string RequestId,
    int ClinicId,
    string ClinicName,
    int RequesterDoctorId,
    string RequesterUserId,
    string RequesterName,
    string? Message
);

public sealed record ClinicJoinRequestNotificationDto(
    int NotificationId,
    int ClinicId,
    string ClinicName,
    int RequesterDoctorId,
    string RequesterName,
    string? Message,
    string Status,
    DateTime CreatedAt
);

public sealed record AppointmentNotificationDto(
    int NotificationId,
    int AppointmentId,
    int DoctorId,
    string DoctorName,
    string PatientName,
    string? AppointmentType,
    DateOnly Date,
    string StartTime,
    string EndTime,
    string Status,
    string NotificationStatus,
    DateTime CreatedAt,
    string? Message
);

public sealed record AppointmentBookingPayload(
    int AppointmentId,
    int DoctorId,
    string DoctorUserId,
    string DoctorName,
    string PatientUserId,
    string PatientName,
    string AppointmentType,
    DateOnly Date,
    string StartTime,
    string EndTime
);