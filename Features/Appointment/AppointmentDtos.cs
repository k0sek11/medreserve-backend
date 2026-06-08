namespace Medreserve.Features.Appointment;

public sealed record BookAppointmentRequest(
    int DoctorId,
    int AppointmentTypeId,
    int ClinicId,
    DateOnly Date,
    string StartTime
);

public sealed record BookAppointmentResultDto(
    int AppointmentId,
    int DoctorId,
    int AppointmentTypeId,
    DateOnly Date,
    string StartTime,
    string EndTime,
    string Status,
    string DoctorName,
    string DoctorSpecialization
);

public class CompleteAppointmentRequest
{
    public string? Comment { get; set; }
}