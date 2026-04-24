using Medreserve.Features.AppointmentType;
using Medreserve.Features.Doctor;
using Medreserve.Features.Notification;
using Medreserve.Features.Patient;
using Medreserve.Features.Payment;

namespace Medreserve.Features.Appointment;

public class Appointment
{
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public int TimeSlotId { get; set; }
    public int AppointmentTypeId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? DoctorNotes { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    public Patient.Patient Patient { get; set; } = null!;
    public Doctor.Doctor Doctor { get; set; } = null!;
    public AppointmentType.AppointmentType AppointmentType { get; set; } = null!;
    public ICollection<Payment.Payment> Payments { get; set; } = new List<Payment.Payment>();
    public ICollection<Notification.Notification> Notifications { get; set; } = new List<Notification.Notification>();
}
