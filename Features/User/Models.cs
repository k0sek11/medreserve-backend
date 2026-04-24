using Microsoft.AspNetCore.Identity;
using Medreserve.Features.Doctor;
using Medreserve.Features.Notification;
using Medreserve.Features.Patient;
using Medreserve.Features.Payment;

namespace Medreserve.Features.User;

public class User : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Patient.Patient? PatientProfile { get; set; }
    public Doctor.Doctor? DoctorProfile { get; set; }
    public ICollection<Notification.Notification> Notifications { get; set; } = new List<Notification.Notification>();
    public ICollection<OfflinePaymentApproval> OfflinePaymentApprovals { get; set; } = new List<OfflinePaymentApproval>();
}
