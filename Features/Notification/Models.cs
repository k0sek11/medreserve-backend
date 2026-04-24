using Medreserve.Features.Appointment;
using Medreserve.Features.User;

namespace Medreserve.Features.Notification;

public class Notification
{
    public int NotificationId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int? AppointmentId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public string? FailureReason { get; set; }

    public Medreserve.Features.User.User User { get; set; } = null!;
    public Appointment.Appointment? Appointment { get; set; }
}
