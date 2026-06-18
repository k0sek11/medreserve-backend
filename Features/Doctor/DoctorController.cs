using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Medreserve.Features.Doctor;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DoctorsController : ControllerBase
{
    private readonly IDoctorService _doctorService;

    public DoctorsController(IDoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    [HttpGet("me/profile")]
    public async Task<ActionResult<DoctorProfileDto>> GetMyProfile(CancellationToken cancellationToken)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var profile = await _doctorService.GetMyProfileAsync(currentUserId, cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpPut("me/profile")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateDoctorProfileDto request, CancellationToken cancellationToken)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var success = await _doctorService.UpdateMyProfileAsync(currentUserId, request, cancellationToken);
        return success ? Ok(new { message = "Doctor profile updated successfully." }) : NotFound();
    }

    [HttpPost("me/appointment-types")]
    public async Task<ActionResult<DoctorAppointmentTypeDto>> CreateMyAppointmentType(
        [FromBody] CreateDoctorAppointmentTypeDto request,
        CancellationToken cancellationToken
    )
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        try
        {
            var appointmentType = await _doctorService.CreateMyAppointmentTypeAsync(currentUserId, request, cancellationToken);
            return appointmentType is null ? NotFound() : Ok(appointmentType);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpDelete("me/appointment-types/{appointmentTypeId:int}")]
    public async Task<IActionResult> DeleteMyAppointmentType(int appointmentTypeId, CancellationToken cancellationToken)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var success = await _doctorService.DeleteMyAppointmentTypeAsync(currentUserId, appointmentTypeId, cancellationToken);
        return success ? NoContent() : NotFound();
    }

    [HttpGet("me/schedules")]
    public async Task<ActionResult<IReadOnlyList<DoctorScheduleDto>>> GetMySchedules(CancellationToken cancellationToken)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var schedules = await _doctorService.GetMySchedulesAsync(currentUserId, cancellationToken);
        return schedules is null ? NotFound() : Ok(schedules);
    }

    [HttpPost("me/schedules")]
    public async Task<ActionResult<DoctorScheduleDto>> UpsertMySchedule(
        [FromBody] UpsertDoctorScheduleDto request,
        CancellationToken cancellationToken
    )
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        try
        {
            var schedule = await _doctorService.UpsertMyScheduleAsync(currentUserId, request, cancellationToken);
            return schedule is null ? NotFound() : Ok(schedule);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPut("me/schedules/{scheduleId:int}")]
    public async Task<ActionResult<DoctorScheduleDto>> UpdateMySchedule(
        int scheduleId,
        [FromBody] UpsertDoctorScheduleDto request,
        CancellationToken cancellationToken
    )
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        try
        {
            var schedule = await _doctorService.UpsertMyScheduleAsync(
                currentUserId,
                request with { ScheduleId = scheduleId },
                cancellationToken
            );

            return schedule is null ? NotFound() : Ok(schedule);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpDelete("me/schedules/{scheduleId:int}")]
    public async Task<IActionResult> DeleteMySchedule(int scheduleId, CancellationToken cancellationToken)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var success = await _doctorService.DeleteMyScheduleAsync(currentUserId, scheduleId, cancellationToken);
        return success ? NoContent() : NotFound();
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
        var doctors = await _doctorService.GetAllAsync(cancellationToken);
        return Ok(doctors);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<DoctorDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var doctor = await _doctorService.GetByIdAsync(id, cancellationToken);
        return doctor is null ? NotFound() : Ok(doctor);
    }

    [HttpGet("{id:int}/profile")]
    [AllowAnonymous]
    public async Task<ActionResult<DoctorPublicProfileDto>> GetPublicProfile(int id, CancellationToken cancellationToken)
    {
        var profile = await _doctorService.GetPublicProfileAsync(id, cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpGet("{id:int}/availability")]
    [AllowAnonymous]
    public async Task<ActionResult<DoctorAvailabilityDto>> GetAvailability(
        int id,
        [FromQuery] DateOnly date,
        [FromQuery] int appointmentTypeId,
        [FromQuery] int? clinicId,
        CancellationToken cancellationToken
    )
    {
        var availability = await _doctorService.GetAvailabilityAsync(id, date, appointmentTypeId, clinicId, cancellationToken);
        return availability is null ? NotFound() : Ok(availability);
    }

    [HttpGet("{id:int}/availability/calendar")]
    [AllowAnonymous]
    public async Task<ActionResult<DoctorAvailabilityCalendarDto>> GetAvailabilityCalendar(
        int id,
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] int appointmentTypeId,
        [FromQuery] int? clinicId,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var availability = await _doctorService.GetAvailabilityCalendarAsync(
                id,
                year,
                month,
                appointmentTypeId,
                clinicId,
                cancellationToken
            );

            return availability is null ? NotFound() : Ok(availability);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResultDto<DoctorSearchItemDto>>> SearchDoctors(
        [FromQuery] DoctorSearchQueryDto query,
        CancellationToken cancellationToken
    )
    {
        var result = await _doctorService.SearchDoctorsAsync(query, cancellationToken);
        return Ok(result);
    }

}
