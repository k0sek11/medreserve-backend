using Medreserve.Features.Appointment;
using Medreserve.Features.Doctor;

namespace Medreserve.Features.AppointmentType;

public class AppointmentType
{
    public int AppointmentTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal BasePrice { get; set; }
    public int DurationMinutes { get; set; }

    public ICollection<DoctorAppointmentType> DoctorAppointmentTypes { get; set; } = new List<DoctorAppointmentType>();
    public ICollection<Appointment.Appointment> Appointments { get; set; } = new List<Appointment.Appointment>();
}
