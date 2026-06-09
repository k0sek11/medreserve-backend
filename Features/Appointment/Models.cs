using Medreserve.Features.AppointmentType;
using Medreserve.Features.Doctor;
using Medreserve.Features.Notification;
using Medreserve.Features.Payment;
using Medreserve.Features.Users;

namespace Medreserve.Features.Appointment;

public static class AppointmentStatus
{
    public const string PendingConfirmation = "PendingConfirmation";
    public const string AwaitingPayment = "AwaitingPayment";
    public const string Confirmed = "Confirmed";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";
    public const string Unpaid = "Unpaid";
    public const string AwaitingOnSitePayment = "AwaitingOnSitePayment";
}

public class Appointment
{
    public int AppointmentId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int DoctorId { get; set; }
    public DateOnly AppointmentDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public int? AppointmentTypeId { get; set; }
    public int AppointmentTypeDurationMinutes { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? DoctorNotes { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    public User User { get; set; } = null!;
    public Doctor.Doctor Doctor { get; set; } = null!;
    public AppointmentType.AppointmentType? AppointmentType { get; set; }
    public ICollection<Payment.Payment> Payments { get; set; } = new List<Payment.Payment>();
    public ICollection<Notification.Notification> Notifications { get; set; } = new List<Notification.Notification>();

    public DateTime GetStartDateTime() =>
        AppointmentSchedulingHelper.ToDateTime(AppointmentDate, StartTime);

    public DateTime GetEndDateTime() =>
        GetStartDateTime().AddMinutes(AppointmentTypeDurationMinutes);
}

public sealed record AppointmentSummaryDto(
    int AppointmentId,
    int DoctorId,
    string DoctorName,
    string DoctorSpecialization,
    string? AppointmentType,
    DateOnly Date,
    string StartTime,
    string EndTime,
    string Status,

    int? PaymentId = null,
    string? PaymentStatus = null,
    string? PaymentMethod = null,
        decimal Price = 0
);

public sealed record AppointmentDetailDto(
    int AppointmentId,
    int DoctorId,
    string DoctorName,
    string DoctorSpecialization,
    string? AppointmentType,
    DateOnly Date,
    string StartTime,
    string EndTime,
    string Status,
    DateTime CreatedAt,

    int? PaymentId = null,
    string? PaymentStatus = null,
    string? PaymentMethod = null,
    string? DoctorNotes = null
);