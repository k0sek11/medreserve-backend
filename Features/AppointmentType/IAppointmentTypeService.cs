namespace Medreserve.Features.AppointmentType;

public interface IAppointmentTypeService
{
    Task<IReadOnlyList<AppointmentTypeDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<AppointmentTypeDto?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<AppointmentTypeDto> CreateAsync(CreateAppointmentTypeRequest request, CancellationToken cancellationToken);
    Task<AppointmentTypeDto?> UpdateAsync(int id, UpdateAppointmentTypeRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}
