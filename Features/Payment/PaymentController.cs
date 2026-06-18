using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Medreserve.Features.Payment.PayU;

namespace Medreserve.Features.Payment;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController(IPaymentService service) : ControllerBase
{

    [HttpPost("init-offline")]
    public async Task<IActionResult> InitOffline([FromBody] InitPaymentDto request)
    {
        var success = await service.CreateOfflinePaymentIntentAsync(request.AppointmentId);

        if (!success) return BadRequest("Nie można zainicjować płatności dla tej wizyty.");

        return Ok(new { message = "Pomyślnie wybrano płatność w placówce." });
    }

    [HttpPost("{id}/confirm-offline")]
    public async Task<IActionResult> ConfirmOffline(int id, [FromBody] ConfirmOfflinePaymentDto request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("Nie udało się zidentyfikować użytkownika.");
        }

        var success = await service.ConfirmOfflinePaymentAsync(id, userId, request.Comment);

        if (!success)
        {
            return NotFound("Płatność nie została znaleziona.");
        }

        return Ok(new { message = "Płatność offline została pomyślnie zatwierdzona." });
    }

    [HttpPost("init-payu")]
    public async Task<IActionResult> InitPayu([FromBody] InitPaymentDto request)
    {
        var redirectUri = await service.InitPayuPaymentAsync(request.AppointmentId);

        return Ok(new { redirectUri = redirectUri });
    }

    [HttpPost("check-status/{appointmentId}")]
    public async Task<IActionResult> CheckStatus(int appointmentId)
    {
        var payuStatus = await service.CheckPayuStatusAsync(appointmentId);

        var isPaid = await service.UpdateStatusAsync(appointmentId, payuStatus);

        return Ok(new { isPaid, payuStatus });
    }

    [HttpPost("payu-notify")]
    [AllowAnonymous]
    public async Task<IActionResult> PayUNotify([FromBody] PayUNotificationRequest request)
    {
        await service.ProcessPayUNotificationAsync(request);

        return Ok();
    }
}
