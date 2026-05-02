using Medreserve.Features.Appointment;
using Medreserve.Features.Users;

namespace Medreserve.Features.Patient;

public class Patient
{
    public int PatientId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? Pesel { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Address { get; set; }

    public Medreserve.Features.Users.User User { get; set; } = null!;
    public ICollection<Appointment.Appointment> Appointments { get; set; } = new List<Appointment.Appointment>();
}
