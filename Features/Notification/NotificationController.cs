using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Medreserve.Features.Notification;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _service;

    public NotificationsController(INotificationService service)
    {
        _service = service;
    }

    [HttpGet("appointments")]
    public async Task<ActionResult<IReadOnlyList<AppointmentNotificationDto>>> GetAppointmentNotifications(
        CancellationToken cancellationToken)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
            return Unauthorized();

        var result = await _service.GetAppointmentNotificationsAsync(currentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("clinic-join-requests")]
    public async Task<ActionResult<IReadOnlyList<ClinicJoinRequestNotificationDto>>> GetClinicJoinRequests(
        [FromQuery] int? clinicId,
        CancellationToken cancellationToken)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
            return Unauthorized();

        var result = await _service.GetClinicJoinRequestsAsync(currentUserId, clinicId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("clinic-join-requests/{notificationId:int}/accept")]
    public async Task<IActionResult> AcceptClinicJoinRequest(
        int notificationId,
        CancellationToken cancellationToken)
    {
        return await HandleClinicJoinRequestStatusAsync(
            notificationId, (u, n, ct) => _service.AcceptClinicJoinRequestAsync(u, n, ct), cancellationToken);
    }

    [HttpPost("clinic-join-requests/{notificationId:int}/reject")]
    public async Task<IActionResult> RejectClinicJoinRequest(
        int notificationId,
        CancellationToken cancellationToken)
    {
        return await HandleClinicJoinRequestStatusAsync(
            notificationId, (u, n, ct) => _service.RejectClinicJoinRequestAsync(u, n, ct), cancellationToken);
    }

    private async Task<IActionResult> HandleClinicJoinRequestStatusAsync(
        int notificationId,
        Func<string, int, CancellationToken, Task<string>> handler,
        CancellationToken cancellationToken)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
            return Unauthorized();

        try
        {
            var status = await handler(currentUserId, notificationId, cancellationToken);
            return Ok(new { message = $"Request {status.ToLowerInvariant()}." });
        }
        catch (InvalidOperationException ex)
        {
            return ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
                ? NotFound()
                : BadRequest(ex.Message);
        }
    }
}
