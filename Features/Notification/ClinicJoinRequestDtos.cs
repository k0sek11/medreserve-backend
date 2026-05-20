namespace Medreserve.Features.Notification;

public static class NotificationKinds
{
    public const string ClinicJoinRequest = "ClinicJoinRequest";
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