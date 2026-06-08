namespace Medreserve.Features.Appointment;

public interface IAppointmentService
{
    Task<BookAppointmentResultDto> BookAppointmentAsync(string userId, BookAppointmentRequest request, CancellationToken cancellationToken);
    Task ConfirmAppointmentAsync(string userId, int appointmentId, CancellationToken cancellationToken);
    Task CancelAppointmentAsync(string userId, int appointmentId, CancellationToken cancellationToken);
    Task CompleteAppointmentAsync(string userId, int appointmentId, CompleteAppointmentRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<AppointmentSummaryDto>> GetMyAppointmentsAsync(string userId, CancellationToken cancellationToken);
    Task<AppointmentDetailDto?> GetAppointmentByIdAsync(string userId, int appointmentId, CancellationToken cancellationToken);
}