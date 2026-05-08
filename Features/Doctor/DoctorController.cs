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

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResultDto<DoctorSearchItemDto>>> SearchDoctors(
        [FromQuery] DoctorSearchQueryDto query,
        CancellationToken cancellationToken
    )
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize switch
        {
            < 1 => 8,
            > 50 => 50,
            _ => query.PageSize
        };

        var doctorsQuery = _dbContext
            .Doctors
            .AsNoTracking()
            .Where(x => x.User.IsActive)
            .AsQueryable();

        if (query.CityId.HasValue)
        {
            doctorsQuery = doctorsQuery.Where(x =>
                x.ClinicDoctors.Any(cd => cd.Clinic.CityId == query.CityId.Value)
            );
        }

        if (query.SpecializationId.HasValue)
        {
            doctorsQuery = doctorsQuery.Where(x =>
                x.DoctorSpecializations.Any(ds => ds.SpecializationId == query.SpecializationId.Value)
            );
        }

        if (query.PriceMax.HasValue)
        {
            doctorsQuery = doctorsQuery.Where(x =>
                x.DoctorAppointmentTypes.Any(dat => dat.AppointmentType.BasePrice <= query.PriceMax.Value)
            );
        }

        if (query.Date.HasValue)
        {
            var dateValue = query.Date.Value.ToDateTime(TimeOnly.MinValue);
            var dayOfWeek = (int)query.Date.Value.DayOfWeek;
            var isoDayOfWeek = dayOfWeek == 0 ? 7 : dayOfWeek;

            doctorsQuery = doctorsQuery.Where(x =>
                x.DoctorSchedules.Any(ds =>
                    ds.IsActive
                    && (ds.DayOfWeek == dayOfWeek || ds.DayOfWeek == isoDayOfWeek)
                    && ds.ValidFrom <= dateValue
                    && (ds.ValidTo == null || ds.ValidTo >= dateValue)
                )
            );
        }

        var totalCount = await doctorsQuery.CountAsync(cancellationToken);

        var projectedQuery = doctorsQuery.Select(x => new
        {
            x.DoctorId,
            FullName = x.User.FirstName + " " + x.User.LastName,
            City = x.ClinicDoctors
                .Where(cd => !query.CityId.HasValue || cd.Clinic.CityId == query.CityId.Value)
                .Select(cd => cd.Clinic.City.Name)
                .FirstOrDefault() ?? string.Empty,
            Specialization = x.DoctorSpecializations
                .Where(ds => !query.SpecializationId.HasValue || ds.SpecializationId == query.SpecializationId.Value)
                .Select(ds => ds.Specialization.Name)
                .FirstOrDefault() ?? string.Empty,
            LowestPrice = x.DoctorAppointmentTypes
                .Select(dat => (decimal?)dat.AppointmentType.BasePrice)
                .Min() ?? 0m,
        });

        var sort = query.Sort?.Trim().ToLowerInvariant() ?? "priceasc";
        projectedQuery = sort switch
        {
            "pricedesc" => projectedQuery.OrderByDescending(x => x.LowestPrice).ThenBy(x => x.FullName),
            "nameasc" => projectedQuery.OrderBy(x => x.FullName),
            "namedesc" => projectedQuery.OrderByDescending(x => x.FullName),
            _ => projectedQuery.OrderBy(x => x.LowestPrice).ThenBy(x => x.FullName)
        };

        var items = await projectedQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new DoctorSearchItemDto(
                x.DoctorId,
                x.FullName,
                x.City,
                x.Specialization,
                x.LowestPrice,
                null
            ))
            .ToListAsync(cancellationToken);

        var totalPages = totalCount == 0 ? 1 : (int)Math.Ceiling(totalCount / (double)pageSize);
        return Ok(new PagedResultDto<DoctorSearchItemDto>(items, page, pageSize, totalCount, totalPages));
    }

}