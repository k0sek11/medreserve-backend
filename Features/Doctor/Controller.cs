using Medreserve.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medreserve.Features.Doctor;

[ApiController]
[Route("api/doctors")]
public class DoctorsController(DatabaseContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DoctorDto>>> GetAll(CancellationToken cancellationToken)
    {
        var doctors = await dbContext
            .Doctors
            .AsNoTracking()
            .OrderBy(x => x.DoctorId)
            .Select(x => new DoctorDto(x.DoctorId, x.UserId, x.LicenseNumber, x.Bio))
            .ToListAsync(cancellationToken);

        return Ok(doctors);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DoctorDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var doctor = await dbContext
            .Doctors
            .AsNoTracking()
            .Where(x => x.DoctorId == id)
            .Select(x => new DoctorDto(x.DoctorId, x.UserId, x.LicenseNumber, x.Bio))
            .FirstOrDefaultAsync(cancellationToken);

        return doctor is null ? NotFound() : Ok(doctor);
    }

    [HttpPost]
    public async Task<ActionResult<DoctorDto>> Create(
        CreateDoctorRequest request,
        CancellationToken cancellationToken
    )
    {
        var doctor = new Doctor
        {
            UserId = request.UserId,
            LicenseNumber = request.LicenseNumber,
            Bio = request.Bio
        };

        dbContext.Doctors.Add(doctor);
        await dbContext.SaveChangesAsync(cancellationToken);

        var result = new DoctorDto(doctor.DoctorId, doctor.UserId, doctor.LicenseNumber, doctor.Bio);
        return CreatedAtAction(nameof(GetById), new { id = doctor.DoctorId }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<DoctorDto>> Update(
        int id,
        UpdateDoctorRequest request,
        CancellationToken cancellationToken
    )
    {
        var doctor = await dbContext.Doctors.FirstOrDefaultAsync(x => x.DoctorId == id, cancellationToken);
        if (doctor is null)
        {
            return NotFound();
        }

        doctor.UserId = request.UserId;
        doctor.LicenseNumber = request.LicenseNumber;
        doctor.Bio = request.Bio;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new DoctorDto(doctor.DoctorId, doctor.UserId, doctor.LicenseNumber, doctor.Bio));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var doctor = await dbContext.Doctors.FirstOrDefaultAsync(x => x.DoctorId == id, cancellationToken);
        if (doctor is null)
        {
            return NotFound();
        }

        dbContext.Doctors.Remove(doctor);
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}

public sealed record DoctorDto(int DoctorId, string UserId, string LicenseNumber, string? Bio);

public sealed record CreateDoctorRequest(string UserId, string LicenseNumber, string? Bio);

public sealed record UpdateDoctorRequest(string UserId, string LicenseNumber, string? Bio);
