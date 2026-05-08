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
                x.StreetAddress,
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
                x.StreetAddress,
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
            StreetAddress = request.StreetAddress,
            CityId = request.CityId,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            IsActive = request.IsActive
        };

        dbContext.Clinics.Add(clinic);
        await dbContext.SaveChangesAsync(cancellationToken);

        var result = new ClinicDto(
            clinic.ClinicId,
            clinic.Name,
            clinic.StreetAddress,
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

        clinic.StreetAddress = request.StreetAddress;
        clinic.CityId = request.CityId;
        clinic.PhoneNumber = request.PhoneNumber;
        clinic.Email = request.Email;
        clinic.IsActive = request.IsActive;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(
            new ClinicDto(
                clinic.ClinicId,
                clinic.Name,
                clinic.StreetAddress,
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

    [HttpGet("cities")]
    public async Task<ActionResult<IReadOnlyList<CityDto>>> GetCities(CancellationToken cancellationToken)
    {
        var cities = await dbContext
            .Cities
            .AsNoTracking()
            .OrderBy(x => x.Voivodeship)
            .ThenBy(x => x.District)
            .ThenBy(x => x.Name)
            .Select(x => new CityDto(x.CityId, x.Name, x.District, x.Voivodeship))
            .ToListAsync(cancellationToken);

        return Ok(cities);
    }

    [HttpGet("specializations")]
    public async Task<ActionResult<IReadOnlyList<SpecializationDto>>> GetSpecializations(CancellationToken cancellationToken)
    {
        var specializations = await dbContext
            .Specializations
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new SpecializationDto(x.SpecializationId, x.Name, x.Description))
            .ToListAsync(cancellationToken);

        return Ok(specializations);
    }

    [HttpGet("cities/{cityId:int}/specializations")]
    public async Task<ActionResult<IReadOnlyList<SpecializationDto>>> GetSpecializationsByCity(
        int cityId,
        CancellationToken cancellationToken
    )
    {
        var cityExists = await dbContext
            .Cities
            .AsNoTracking()
            .AnyAsync(x => x.CityId == cityId, cancellationToken);

        if (!cityExists)
        {
            return NotFound("City not found");
        }

        var specializations = await dbContext.Doctors
            .AsNoTracking()
            .Where(d => d.ClinicDoctors.Any(cd => cd.Clinic.CityId == cityId))
            .SelectMany(d => d.DoctorSpecializations)
            .Select(x => x.Specialization)
            .Distinct()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return Ok(specializations.Select(x => new SpecializationDto(
            x.SpecializationId,
            x.Name,
            x.Description
        )));
    }

    [HttpGet("cities/by-specialization/{specializationId:int}")]
    public async Task<ActionResult<IReadOnlyList<CityDto>>> GetCitiesBySpecialization(
        int specializationId,
        CancellationToken cancellationToken
    )
    {
        var specializationExists = await dbContext
            .Specializations
            .AsNoTracking()
            .AnyAsync(x => x.SpecializationId == specializationId, cancellationToken);

        if (!specializationExists)
        {
            return NotFound("Specialization not found");
        }

        var cities = await dbContext.Doctors
            .AsNoTracking()
            .Where(d => d.DoctorSpecializations.Any(ds => ds.SpecializationId == specializationId))
            .SelectMany(d => d.ClinicDoctors.Select(cd => cd.Clinic.City))
            .Distinct()
            .OrderBy(x => x.Voivodeship)
            .ThenBy(x => x.District)
            .ThenBy(x => x.Name)
            .Select(x => new CityDto(x.CityId, x.Name, x.District, x.Voivodeship))
            .ToListAsync(cancellationToken);

        return Ok(cities);
    }

    [HttpGet("by-city/{cityId:int}")]
    public async Task<ActionResult<IReadOnlyList<ClinicDto>>> GetClinicsByCity(
        int cityId,
        CancellationToken cancellationToken
    )
    {
        var clinics = await dbContext
            .Clinics
            .AsNoTracking()
            .Where(x => x.CityId == cityId && x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new ClinicDto(
                x.ClinicId,
                x.Name,
                x.StreetAddress,
                x.PhoneNumber,
                x.Email,
                x.IsActive
            ))
            .ToListAsync(cancellationToken);

        return Ok(clinics);
    }
}

public sealed record ClinicDto(
    int ClinicId,
    string Name,
    string StreetAddress,
    string? PhoneNumber,
    string? Email,
    bool IsActive
);

public sealed record CreateClinicRequest(
    string Name,
    string StreetAddress,
    int CityId,
    string? PhoneNumber,
    string? Email,
    bool IsActive
);

public sealed record UpdateClinicRequest(
    string Name,
    string StreetAddress,
    int CityId,
    string? PhoneNumber,
    string? Email,
    bool IsActive
);

public sealed record CityDto(
    int CityId,
    string Name,
    string District,
    string Voivodeship
);

public sealed record SpecializationDto(
    int SpecializationId,
    string Name,
    string? Description
);
