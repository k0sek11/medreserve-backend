using Medreserve.Features.Appointment;
using Medreserve.Features.AppointmentType;
using Medreserve.Features.Clinic;
using Medreserve.Features.Specialization;
using Medreserve.Features.Users;

namespace Medreserve.Features.Doctor;

public class Doctor
{
    public int DoctorId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string? Bio { get; set; }

    public Medreserve.Features.Users.User User { get; set; } = null!;
    public ICollection<DoctorSchedule> DoctorSchedules { get; set; } = new List<DoctorSchedule>();
    public ICollection<DoctorSpecialization> DoctorSpecializations { get; set; } = new List<DoctorSpecialization>();
    public ICollection<ClinicDoctor> ClinicDoctors { get; set; } = new List<ClinicDoctor>();
    public ICollection<DoctorAppointmentType> DoctorAppointmentTypes { get; set; } = new List<DoctorAppointmentType>();
    public ICollection<Appointment.Appointment> Appointments { get; set; } = new List<Appointment.Appointment>();
}

public class ClinicDoctor
{
    public int ClinicId { get; set; }
    public int DoctorId { get; set; }
    public bool IsOwner { get; set; }

    public Clinic.Clinic Clinic { get; set; } = null!;
    public Doctor Doctor { get; set; } = null!;
}

public class DoctorSpecialization
{
    public int DoctorId { get; set; }
    public int SpecializationId { get; set; }

    public Doctor Doctor { get; set; } = null!;
    public Specialization.Specialization Specialization { get; set; } = null!;
}

public class DoctorSchedule
{
    public int ScheduleId { get; set; }
    public int DoctorId { get; set; }
    public int DayOfWeek { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; }

    public Doctor Doctor { get; set; } = null!;
}

public class DoctorAppointmentType
{
    public int DoctorId { get; set; }
    public int AppointmentTypeId { get; set; }

    public Doctor Doctor { get; set; } = null!;
    public AppointmentType.AppointmentType AppointmentType { get; set; } = null!;
}
