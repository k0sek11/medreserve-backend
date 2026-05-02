using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Medreserve.Infrastructure;

namespace Medreserve.Features.Doctor;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DoctorsController : ControllerBase
{
    private readonly IDoctorService _doctorService;
    private readonly DatabaseContext _dbContext;

    public DoctorsController(IDoctorService doctorService, DatabaseContext dbContext)
    {
        _doctorService = doctorService;
        _dbContext = dbContext;
    }
    
    [HttpPost("profile")]
    public async Task<IActionResult> CreateProfile([FromBody] CreateDoctorProfileDto request)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId == null) return Unauthorized();

        var success = await _doctorService.CreateProfileAsync(currentUserId, request);

        if (!success) return BadRequest(new { message = "Failed to create doctor profile." });

        return Ok(new { message = "Doctor profile created successfully!" });
    }
    
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<DoctorDto>>> GetAll(CancellationToken cancellationToken)
    {
        var doctors = await _dbContext.Set<Doctor>()
            .AsNoTracking()
            .OrderBy(x => x.DoctorId)
            .Select(x => new DoctorDto(x.DoctorId, x.UserId, x.LicenseNumber, x.Bio))
            .ToListAsync(cancellationToken);

        return Ok(doctors);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<DoctorDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var doctor = await _dbContext.Set<Doctor>()
            .AsNoTracking()
            .Where(x => x.DoctorId == id)
            .Select(x => new DoctorDto(x.DoctorId, x.UserId, x.LicenseNumber, x.Bio))
            .FirstOrDefaultAsync(cancellationToken);

        return doctor is null ? NotFound() : Ok(doctor);
    }
    
}