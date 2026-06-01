using System.Security.Claims;
using System.Text.Json;
using Medreserve.Features.Appointment;
using Medreserve.Features.Clinic;
using Medreserve.Features.Doctor;
using Medreserve.Features.Users;
using Medreserve.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotificationEntity = Medreserve.Features.Notification.Notification;

namespace Medreserve.Features.Notification;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController(DatabaseContext dbContext) : ControllerBase
{
    [HttpGet("appointments")]
    public async Task<ActionResult<IReadOnlyList<AppointmentNotificationDto>>> GetAppointmentNotifications(
        CancellationToken cancellationToken
    )
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var notifications = await dbContext.Notifications
            .AsNoTracking()
            .Where(x => x.UserId == currentUserId && x.Type == NotificationKinds.AppointmentBooked)
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

        var result = notifications
            .Where(x => x.Appointment is not null)
            .Select(MapAppointmentNotification)
            .ToList();

        return Ok(result);
    }

    [HttpPost("appointments/{notificationId:int}/confirm")]
    public async Task<IActionResult> ConfirmAppointment(
        int notificationId,
        CancellationToken cancellationToken
    )
    {
        return await UpdateAppointmentStatusAsync(notificationId, "Confirmed", "Wizyta potwierdzona", cancellationToken);
    }

    [HttpPost("appointments/{notificationId:int}/cancel")]
    public async Task<IActionResult> CancelAppointment(
        int notificationId,
        CancellationToken cancellationToken
    )
    {
        return await UpdateAppointmentStatusAsync(notificationId, "Cancelled", "Wizyta anulowana", cancellationToken);
    }

    [HttpGet("clinic-join-requests")]
    public async Task<ActionResult<IReadOnlyList<ClinicJoinRequestNotificationDto>>> GetClinicJoinRequests(
        [FromQuery] int? clinicId,
        CancellationToken cancellationToken
    )
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var notifications = await dbContext.Notifications
            .AsNoTracking()
            .Where(x => x.UserId == currentUserId && x.Type == NotificationKinds.ClinicJoinRequest)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var result = notifications
            .Select(MapJoinRequestNotification)
            .Where(x => x is not null)
            .Select(x => x!)
            .Where(x => !clinicId.HasValue || x.ClinicId == clinicId.Value)
            .ToList();

        return Ok(result);
    }

    [HttpPost("clinic-join-requests/{notificationId:int}/accept")]
    public async Task<IActionResult> AcceptClinicJoinRequest(
        int notificationId,
        CancellationToken cancellationToken
    )
    {
        return await UpdateClinicJoinRequestStatusAsync(notificationId, "Accepted", accept: true, cancellationToken);
    }

    [HttpPost("clinic-join-requests/{notificationId:int}/reject")]
    public async Task<IActionResult> RejectClinicJoinRequest(
        int notificationId,
        CancellationToken cancellationToken
    )
    {
        return await UpdateClinicJoinRequestStatusAsync(notificationId, "Rejected", accept: false, cancellationToken);
    }

    private async Task<IActionResult> UpdateClinicJoinRequestStatusAsync(
        int notificationId,
        string status,
        bool accept,
        CancellationToken cancellationToken
    )
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var notification = await dbContext.Notifications.FirstOrDefaultAsync(
            x => x.NotificationId == notificationId
                 && x.UserId == currentUserId
                 && x.Type == NotificationKinds.ClinicJoinRequest,
            cancellationToken
        );

        if (notification is null)
        {
            return NotFound();
        }

        var payload = DeserializePayload(notification);
        if (payload is null)
        {
            return BadRequest("Malformed join request payload.");
        }

        var currentTimestamp = DateTime.UtcNow;
        var groupedNotifications = await dbContext.Notifications
            .Where(x => x.Type == NotificationKinds.ClinicJoinRequest && x.Content == notification.Content)
            .ToListAsync(cancellationToken);

        foreach (var joinNotification in groupedNotifications)
        {
            joinNotification.Status = status;
            joinNotification.SentAt = currentTimestamp;
        }

        if (accept)
        {
            var membershipExists = await dbContext.ClinicDoctors.AnyAsync(
                x => x.ClinicId == payload.ClinicId && x.DoctorId == payload.RequesterDoctorId,
                cancellationToken
            );

            if (!membershipExists)
            {
                dbContext.ClinicDoctors.Add(new ClinicDoctor
                {
                    ClinicId = payload.ClinicId,
                    DoctorId = payload.RequesterDoctorId,
                    IsOwner = false,
                });
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new { message = $"Request {status.ToLowerInvariant()}." });
    }

    private async Task<IActionResult> UpdateAppointmentStatusAsync(
        int notificationId,
        string appointmentStatus,
        string notificationStatus,
        CancellationToken cancellationToken
    )
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var notification = await dbContext.Notifications
            .Include(x => x.Appointment)
                .ThenInclude(x => x!.User)
            .Include(x => x.Appointment)
                .ThenInclude(x => x!.Doctor)
                    .ThenInclude(x => x.User)
            .Include(x => x.Appointment)
                .ThenInclude(x => x!.AppointmentType)
            .FirstOrDefaultAsync(
                x => x.NotificationId == notificationId
                     && x.UserId == currentUserId
                     && x.Type == NotificationKinds.AppointmentBooked,
                cancellationToken
            );

        if (notification is null || notification.Appointment is null)
        {
            return NotFound();
        }

        if (!string.Equals(notification.Appointment.Status, "Pending", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(notification.Appointment.Status, "Confirmed", StringComparison.OrdinalIgnoreCase))
        {
            return Conflict(new { message = "Wizyta została już rozliczona lub anulowana." });
        }

        notification.Appointment.Status = appointmentStatus;
        notification.Appointment.UpdatedAt = DateTime.UtcNow;
        notification.Status = notificationStatus;
        notification.SentAt = DateTime.UtcNow;

        dbContext.Notifications.Add(new NotificationEntity
        {
            UserId = notification.Appointment.UserId,
            AppointmentId = notification.Appointment.AppointmentId,
            Type = NotificationKinds.AppointmentStatusChanged,
            Subject = appointmentStatus == "Confirmed" ? "Wizyta potwierdzona" : "Wizyta anulowana",
            Content = JsonSerializer.Serialize(new
            {
                notification.Appointment.AppointmentId,
                notification.Appointment.DoctorId,
                DoctorName = $"{notification.Appointment.Doctor.User.FirstName} {notification.Appointment.Doctor.User.LastName}",
                AppointmentType = notification.Appointment.AppointmentType?.Name ?? "Nieznane",
                notification.Appointment.Status,
            }),
            Status = "Sent",
            CreatedAt = DateTime.UtcNow,
            SentAt = DateTime.UtcNow,
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new { message = $"Appointment {appointmentStatus.ToLowerInvariant()}." });
    }

    private static ClinicJoinRequestNotificationDto? MapJoinRequestNotification(NotificationEntity notification)
    {
        var payload = DeserializePayload(notification);
        if (payload is null)
        {
            return null;
        }

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

    private static AppointmentNotificationDto MapAppointmentNotification(NotificationEntity notification)
    {
        var appointment = notification.Appointment!;
        var (_, date, startTime) = AppointmentSchedulingHelper.DecodeTimeSlotId(appointment.TimeSlotId);
        var endTime = startTime.AddMinutes(appointment.AppointmentTypeDurationMinutes);

        
        var latestPayment = appointment.Payments?.OrderByDescending(p => p.CreatedAt).FirstOrDefault();
        
        return new AppointmentNotificationDto(
            notification.NotificationId,
            appointment.AppointmentId,
            appointment.DoctorId,
            $"{appointment.Doctor.User.FirstName} {appointment.Doctor.User.LastName}",
            $"{appointment.User.FirstName} {appointment.User.LastName}",
            appointment.AppointmentType?.Name,
            date,
            startTime.ToString("HH:mm"),
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