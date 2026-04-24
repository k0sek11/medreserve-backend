using Medreserve.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medreserve.Features.AppointmentType;

[ApiController]
[Route("api/appointment-types")]
public class AppointmentTypesController(DatabaseContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AppointmentTypeDto>>> GetAll(CancellationToken cancellationToken)
    {
        var appointmentTypes = await dbContext
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

        return Ok(appointmentTypes);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AppointmentTypeDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var appointmentType = await dbContext
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

        return appointmentType is null ? NotFound() : Ok(appointmentType);
    }

    [HttpPost]
    public async Task<ActionResult<AppointmentTypeDto>> Create(
        CreateAppointmentTypeRequest request,
        CancellationToken cancellationToken
    )
    {
        var appointmentType = new AppointmentType
        {
            Name = request.Name,
            Description = request.Description,
            BasePrice = request.BasePrice,
            DurationMinutes = request.DurationMinutes
        };

        dbContext.AppointmentTypes.Add(appointmentType);
        await dbContext.SaveChangesAsync(cancellationToken);

        var result = new AppointmentTypeDto(
            appointmentType.AppointmentTypeId,
            appointmentType.Name,
            appointmentType.Description,
            appointmentType.BasePrice,
            appointmentType.DurationMinutes
        );

        return CreatedAtAction(nameof(GetById), new { id = appointmentType.AppointmentTypeId }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AppointmentTypeDto>> Update(
        int id,
        UpdateAppointmentTypeRequest request,
        CancellationToken cancellationToken
    )
    {
        var appointmentType = await dbContext.AppointmentTypes.FirstOrDefaultAsync(
            x => x.AppointmentTypeId == id,
            cancellationToken
        );
        if (appointmentType is null)
        {
            return NotFound();
        }

        appointmentType.Name = request.Name;
        appointmentType.Description = request.Description;
        appointmentType.BasePrice = request.BasePrice;
        appointmentType.DurationMinutes = request.DurationMinutes;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(
            new AppointmentTypeDto(
                appointmentType.AppointmentTypeId,
                appointmentType.Name,
                appointmentType.Description,
                appointmentType.BasePrice,
                appointmentType.DurationMinutes
            )
        );
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var appointmentType = await dbContext.AppointmentTypes.FirstOrDefaultAsync(
            x => x.AppointmentTypeId == id,
            cancellationToken
        );
        if (appointmentType is null)
        {
            return NotFound();
        }

        dbContext.AppointmentTypes.Remove(appointmentType);
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}

public sealed record AppointmentTypeDto(
    int AppointmentTypeId,
    string Name,
    string? Description,
    decimal BasePrice,
    int DurationMinutes
);

public sealed record CreateAppointmentTypeRequest(
    string Name,
    string? Description,
    decimal BasePrice,
    int DurationMinutes
);

public sealed record UpdateAppointmentTypeRequest(
    string Name,
    string? Description,
    decimal BasePrice,
    int DurationMinutes
);
