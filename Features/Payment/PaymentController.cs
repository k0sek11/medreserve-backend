using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Medreserve.Features.Payment.PayU; 

namespace Medreserve.Features.Payment;

/// <summary>
/// Kontroler płatności. Zgodnie z architekturą "Thin Controller", nie zawiera logiki biznesowej.
/// Jego jedynym zadaniem jest autoryzacja żądania, przekazanie go do serwisu i zwrócenie odpowiedniego kodu HTTP.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] 
public class PaymentsController(IPaymentService service) : ControllerBase // Zmieniono '_service' na 'service' (zgodnie z konwencją)
{
    // =========================================================================
    // 1. GOTÓWKA (W PLACÓWCE)
    // =========================================================================
    
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
        // Pobieramy ID zalogowanego pracownika recepcji/lekarza z tokenu JWT
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
        
    // =========================================================================
    // 2. PAYU (ONLINE)
    // =========================================================================
    
    [HttpPost("init-payu")]
    public async Task<IActionResult> InitPayu([FromBody] InitPaymentDto request)
    {
        var redirectUri = await service.InitPayuPaymentAsync(request.AppointmentId);
        
        // Zwracamy link do bramki PayU, pod który React musi przekierować pacjenta
        return Ok(new { redirectUri = redirectUri });
    }
        
    [HttpPost("check-status/{appointmentId}")]
    public async Task<IActionResult> CheckStatus(int appointmentId)
    {
        // ZASADA SRP W PRAKTYCE (Zgodnie z wymaganiem prowadzącego):
        // Krok 1: Tylko zapytaj PayU, co się dzieje z pieniędzmi (np. "COMPLETED", "CANCELED")
        var payuStatus = await service.CheckPayuStatusAsync(appointmentId);
        
        // Krok 2: Na podstawie tego statusu, zaktualizuj naszą bazę danych
        var isPaid = await service.UpdateStatusAsync(appointmentId, payuStatus);
        
        return Ok(new { isPaid });
    }
        
    // =========================================================================
    // 3. WEBHOOKI (PRODUKCJA)
    // =========================================================================
    
    [HttpPost("payu-notify")]
    [AllowAnonymous] // Wyjątek: Serwery PayU nie mają tokenu JWT pacjenta, muszą tu wejść bez logowania
    public async Task<IActionResult> PayUNotify([FromBody] PayUNotificationRequest request)
    {
        await service.ProcessPayUNotificationAsync(request);
        
        // PayU WYMAGA kodu 200 OK w odpowiedzi, niezależnie od tego czy status to sukces, czy błąd.
        // W przeciwnym razie będą próbowali wysłać powiadomienie ponownie.
        return Ok();
    }
}