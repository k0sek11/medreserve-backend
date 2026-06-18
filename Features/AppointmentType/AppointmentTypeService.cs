using Microsoft.EntityFrameworkCore;
using Medreserve.Infrastructure;

namespace Medreserve.Features.AppointmentType;

public class AppointmentTypeService : IAppointmentTypeService
{
    private readonly DatabaseContext _dbContext;

    public AppointmentTypeService(DatabaseContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AppointmentTypeDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _dbContext
            .AppointmentTypes
            .AsNoTracking()
            .OrderBy(x => x.AppointmentTypeId)
            .Select(x => new AppointmentTypeDto(
                x.AppointmentTypeId,
                x.Name,
                x.Description,
                x.BasePrice,
                x.DurationMinutes
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<AppointmentTypeDto?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _dbContext
            .AppointmentTypes
            .AsNoTracking()
            .Where(x => x.AppointmentTypeId == id)
            .Select(x => new AppointmentTypeDto(
                x.AppointmentTypeId,
                x.Name,
                x.Description,
                x.BasePrice,
                x.DurationMinutes
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<AppointmentTypeDto> CreateAsync(CreateAppointmentTypeRequest request, CancellationToken cancellationToken)
    {
        var appointmentType = new AppointmentType
        {
            Name = request.Name,
            Description = request.Description,
            BasePrice = request.BasePrice,
            DurationMinutes = request.DurationMinutes
        };

        _dbContext.AppointmentTypes.Add(appointmentType);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AppointmentTypeDto(
            appointmentType.AppointmentTypeId,
            appointmentType.Name,
            appointmentType.Description,
            appointmentType.BasePrice,
            appointmentType.DurationMinutes
        );
    }

    public async Task<AppointmentTypeDto?> UpdateAsync(int id, UpdateAppointmentTypeRequest request, CancellationToken cancellationToken)
    {
        var appointmentType = await _dbContext.AppointmentTypes.FirstOrDefaultAsync(
            x => x.AppointmentTypeId == id,
            cancellationToken
        );

        if (appointmentType is null)
            return null;

        appointmentType.Name = request.Name;
        appointmentType.Description = request.Description;
        appointmentType.BasePrice = request.BasePrice;
        appointmentType.DurationMinutes = request.DurationMinutes;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AppointmentTypeDto(
            appointmentType.AppointmentTypeId,
            appointmentType.Name,
            appointmentType.Description,
            appointmentType.BasePrice,
            appointmentType.DurationMinutes
        );
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var appointmentType = await _dbContext.AppointmentTypes.FirstOrDefaultAsync(
            x => x.AppointmentTypeId == id,
            cancellationToken
        );

        if (appointmentType is null)
            return false;

        var appointments = await _dbContext.Appointments
            .Where(x => x.AppointmentTypeId == id)
            .ToListAsync(cancellationToken);

        foreach (var appointment in appointments)
        {
            appointment.AppointmentTypeId = null;
            appointment.AppointmentType = null;
        }

        _dbContext.AppointmentTypes.Remove(appointmentType);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
