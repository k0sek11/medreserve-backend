namespace Medreserve.Features.Notification;

public interface INotificationService
{
    Task<IReadOnlyList<AppointmentNotificationDto>> GetAppointmentNotificationsAsync(string userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ClinicJoinRequestNotificationDto>> GetClinicJoinRequestsAsync(string userId, int? clinicId, CancellationToken cancellationToken);
    Task<string> AcceptClinicJoinRequestAsync(string userId, int notificationId, CancellationToken cancellationToken);
    Task<string> RejectClinicJoinRequestAsync(string userId, int notificationId, CancellationToken cancellationToken);
}
