using Medreserve.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medreserve.Features.Clinic;

[ApiController]
[Route("api/clinics")]
public class ClinicsController(DatabaseContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ClinicDto>>> GetAll(CancellationToken cancellationToken)
    {
        var clinics = await dbContext
            .Clinics
            .AsNoTracking()
            .OrderBy(x => x.ClinicId)
            .Select(x => new ClinicDto(
                x.ClinicId,
                x.Name,
                x.Address,
                x.PhoneNumber,
                x.Email,
                x.IsActive
            ))
            .ToListAsync(cancellationToken);

        return Ok(clinics);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClinicDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var clinic = await dbContext
            .Clinics
            .AsNoTracking()
            .Where(x => x.ClinicId == id)
            .Select(x => new ClinicDto(
                x.ClinicId,
                x.Name,
                x.Address,
                x.PhoneNumber,
                x.Email,
                x.IsActive
            ))
            .FirstOrDefaultAsync(cancellationToken);

        return clinic is null ? NotFound() : Ok(clinic);
    }

    [HttpPost]
    public async Task<ActionResult<ClinicDto>> Create(CreateClinicRequest request, CancellationToken cancellationToken)
    {
        var clinic = new Clinic
        {
            Name = request.Name,
            Address = request.Address,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            IsActive = request.IsActive
        };

        dbContext.Clinics.Add(clinic);
        await dbContext.SaveChangesAsync(cancellationToken);

        var result = new ClinicDto(
            clinic.ClinicId,
            clinic.Name,
            clinic.Address,
            clinic.PhoneNumber,
            clinic.Email,
            clinic.IsActive
        );

        return CreatedAtAction(nameof(GetById), new { id = clinic.ClinicId }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ClinicDto>> Update(
        int id,
        UpdateClinicRequest request,
        CancellationToken cancellationToken
    )
    {
        var clinic = await dbContext.Clinics.FirstOrDefaultAsync(x => x.ClinicId == id, cancellationToken);
        if (clinic is null)
        {
            return NotFound();
        }

        clinic.Name = request.Name;
        clinic.Address = request.Address;
        clinic.PhoneNumber = request.PhoneNumber;
        clinic.Email = request.Email;
        clinic.IsActive = request.IsActive;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(
            new ClinicDto(
                clinic.ClinicId,
                clinic.Name,
                clinic.Address,
                clinic.PhoneNumber,
                clinic.Email,
                clinic.IsActive
            )
        );
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var clinic = await dbContext.Clinics.FirstOrDefaultAsync(x => x.ClinicId == id, cancellationToken);
        if (clinic is null)
        {
            return NotFound();
        }

        dbContext.Clinics.Remove(clinic);
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}

public sealed record ClinicDto(
    int ClinicId,
    string Name,
    string Address,
    string? PhoneNumber,
    string? Email,
    bool IsActive
);

public sealed record CreateClinicRequest(
    string Name,
    string Address,
    string? PhoneNumber,
    string? Email,
    bool IsActive
);

public sealed record UpdateClinicRequest(
    string Name,
    string Address,
    string? PhoneNumber,
    string? Email,
    bool IsActive
);
