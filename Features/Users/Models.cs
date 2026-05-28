using Microsoft.AspNetCore.Identity;
using Medreserve.Features.Appointment;
using Medreserve.Features.Doctor;
using Medreserve.Features.Notification;
using Medreserve.Features.Payment;

namespace Medreserve.Features.Users;

public class User : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly? BirthDate { get; set; }
    public string? Gender { get; set; }
    public bool IsActive { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Doctor.Doctor? DoctorProfile { get; set; }
    public ICollection<Appointment.Appointment> Appointments { get; set; } = new List<Appointment.Appointment>();
    public ICollection<Notification.Notification> Notifications { get; set; } = new List<Notification.Notification>();
    public ICollection<OfflinePaymentApproval> OfflinePaymentApprovals { get; set; } = new List<OfflinePaymentApproval>();
}
