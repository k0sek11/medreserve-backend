using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Medreserve.Features.Appointment;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController(IAppointmentService appointmentService) : ControllerBase
{
    private readonly IAppointmentService _appointmentService = appointmentService;

    private string GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
    }

    [HttpPost]
    public async Task<ActionResult<BookAppointmentResultDto>> BookAppointment(
        [FromBody] BookAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

        var result = await _appointmentService.BookAppointmentAsync(currentUserId, request, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = result.AppointmentId }, result);
    }

    [HttpPost("{id:int}/confirm")]
    public async Task<IActionResult> ConfirmAppointment(
        int id,
        [FromBody] ConfirmAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

        await _appointmentService.ConfirmAppointmentAsync(currentUserId, id, request.IsOnline, cancellationToken);

        return Ok(new { message = "Appointment confirmed successfully." });
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> CancelAppointment(
        int id,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

        await _appointmentService.CancelAppointmentAsync(currentUserId, id, cancellationToken);

        return Ok(new { message = "Appointment cancelled successfully." });
    }

    [HttpPost("{id:int}/complete")]
    public async Task<IActionResult> CompleteAppointment(
        int id,
        [FromBody] CompleteAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

        await _appointmentService.CompleteAppointmentAsync(currentUserId, id, request.Comment, cancellationToken);

        return Ok(new { message = "Appointment marked as completed." });
    }

    [HttpGet("mine")]
    public async Task<ActionResult<IReadOnlyList<AppointmentSummaryDto>>> GetMine(CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

        var result = await _appointmentService.GetMyAppointmentsAsync(currentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AppointmentDetailDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

        var result = await _appointmentService.GetAppointmentByIdAsync(currentUserId, id, cancellationToken);

        if (result is null) return NotFound();

        return Ok(result);
    }
}
