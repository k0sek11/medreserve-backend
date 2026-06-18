using System.Text.Json;
using Medreserve.Features.Clinic;
using Medreserve.Features.Doctor;
using Medreserve.Infrastructure;
using Microsoft.EntityFrameworkCore;
using NotificationEntity = Medreserve.Features.Notification.Notification;

namespace Medreserve.Features.Notification;

public class NotificationService : INotificationService
{
    private readonly DatabaseContext _dbContext;

    public NotificationService(DatabaseContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AppointmentNotificationDto>> GetAppointmentNotificationsAsync(
        string userId, CancellationToken cancellationToken)
    {
        var notifications = await _dbContext.Notifications
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.Type == NotificationKinds.AppointmentBooked)
            .Include(x => x.Appointment)
                .ThenInclude(x => x!.User)
            .Include(x => x.Appointment)
                .ThenInclude(x => x!.Doctor)
                    .ThenInclude(x => x.User)
            .Include(x => x.Appointment)
                .ThenInclude(x => x!.AppointmentType)
            .Include(x => x.Appointment).ThenInclude(x => x!.Payments)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return notifications
            .Where(x => x.Appointment is not null)
            .Select(MapAppointmentNotification)
            .ToList();
    }

    public async Task<IReadOnlyList<ClinicJoinRequestNotificationDto>> GetClinicJoinRequestsAsync(
        string userId, int? clinicId, CancellationToken cancellationToken)
    {
        var notifications = await _dbContext.Notifications
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.Type == NotificationKinds.ClinicJoinRequest)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return notifications
            .Select(MapJoinRequestNotification)
            .Where(x => x is not null)
            .Select(x => x!)
            .Where(x => !clinicId.HasValue || x.ClinicId == clinicId.Value)
            .ToList();
    }

    public async Task<string> AcceptClinicJoinRequestAsync(
        string userId, int notificationId, CancellationToken cancellationToken)
    {
        return await UpdateClinicJoinRequestStatusAsync(userId, notificationId, "Accepted", accept: true, cancellationToken);
    }

    public async Task<string> RejectClinicJoinRequestAsync(
        string userId, int notificationId, CancellationToken cancellationToken)
    {
        return await UpdateClinicJoinRequestStatusAsync(userId, notificationId, "Rejected", accept: false, cancellationToken);
    }

    private async Task<string> UpdateClinicJoinRequestStatusAsync(
        string userId,
        int notificationId,
        string status,
        bool accept,
        CancellationToken cancellationToken)
    {
        var notification = await _dbContext.Notifications.FirstOrDefaultAsync(
            x => x.NotificationId == notificationId
                 && x.UserId == userId
                 && x.Type == NotificationKinds.ClinicJoinRequest,
            cancellationToken);

        if (notification is null)
            throw new InvalidOperationException("Notification not found.");

        var payload = DeserializePayload(notification)
            ?? throw new InvalidOperationException("Malformed join request payload.");

        var currentTimestamp = DateTime.UtcNow;
        var groupedNotifications = await _dbContext.Notifications
            .Where(x => x.Type == NotificationKinds.ClinicJoinRequest && x.Content == notification.Content)
            .ToListAsync(cancellationToken);

        foreach (var joinNotification in groupedNotifications)
        {
            joinNotification.Status = status;
            joinNotification.SentAt = currentTimestamp;
        }

        if (accept)
        {
            var membershipExists = await _dbContext.ClinicDoctors.AnyAsync(
                x => x.ClinicId == payload.ClinicId && x.DoctorId == payload.RequesterDoctorId,
                cancellationToken);

            if (!membershipExists)
            {
                _dbContext.ClinicDoctors.Add(new ClinicDoctor
                {
                    ClinicId = payload.ClinicId,
                    DoctorId = payload.RequesterDoctorId,
                    IsOwner = false,
                });
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return status;
    }

    private static AppointmentNotificationDto MapAppointmentNotification(NotificationEntity notification)
    {
        var appointment = notification.Appointment!;
        var endTime = appointment.StartTime.AddMinutes(appointment.AppointmentTypeDurationMinutes);

        var latestPayment = appointment.Payments?.OrderByDescending(p => p.CreatedAt).FirstOrDefault();

        return new AppointmentNotificationDto(
            notification.NotificationId,
            appointment.AppointmentId,
            appointment.DoctorId,
            $"{appointment.Doctor.User.FirstName} {appointment.Doctor.User.LastName}",
            $"{appointment.User.FirstName} {appointment.User.LastName}",
            appointment.AppointmentType?.Name,
            appointment.AppointmentDate,
            appointment.StartTime.ToString("HH:mm"),
            endTime.ToString("HH:mm"),
            appointment.Status,
            notification.Status,
            notification.CreatedAt,
            notification.Subject,
            latestPayment?.PaymentId,
            latestPayment?.Status,
            latestPayment?.Method
        );
    }

    private static ClinicJoinRequestNotificationDto? MapJoinRequestNotification(NotificationEntity notification)
    {
        var payload = DeserializePayload(notification);
        if (payload is null)
            return null;

        return new ClinicJoinRequestNotificationDto(
            notification.NotificationId,
            payload.ClinicId,
            payload.ClinicName,
            payload.RequesterDoctorId,
            payload.RequesterName,
            payload.Message,
            notification.Status,
            notification.CreatedAt
        );
    }

    private static ClinicJoinRequestPayload? DeserializePayload(NotificationEntity notification)
    {
        try
        {
            return JsonSerializer.Deserialize<ClinicJoinRequestPayload>(notification.Content);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
