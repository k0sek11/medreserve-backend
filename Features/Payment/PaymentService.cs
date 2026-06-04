using System.Collections.Concurrent;
using Medreserve.Features.Payment.PayU;
using Medreserve.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Medreserve.Features.Payment;
using PaymentEntity = Medreserve.Features.Payment.Payment;
/// <summary>
/// Główny serwis obsługujący logikę biznesową płatności.
/// Odpowiada za operacje na bazie danych, ale nie wychodzi do internetu (od tego jest IPayUService).
/// </summary>
public class PaymentService(DatabaseContext _context, IPayUService _payUService) : IPaymentService
{
    // Słownik in-memory, który w środowisku testowym (localhost) pamięta przypisanie 
    // naszego lokalnego PaymentId do OrderId nadanego przez system PayU.
    private static readonly ConcurrentDictionary<int, string> _payuOrders = new();

    // =========================================================================
    // 1. GOTÓWKA (W PLACÓWCE)
    // =========================================================================
    
    public async Task<bool> CreateOfflinePaymentIntentAsync(int appointmentId)
    {
        // 1. Pobierzmy wizytę i jej cennik, żeby znać kwotę.
        var apt = await _context.Appointments
            .Include(a => a.AppointmentType)
            .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

        if (apt?.AppointmentType == null) return false;

        // 2. Zamiast 'AnyAsync', sprawdzamy, CZY i JAKA płatność już istnieje.
        var existingPayment = await _context.Payments
            .FirstOrDefaultAsync(p => p.AppointmentId == appointmentId);

        if (existingPayment != null)
        {
            // Jeśli wizyta jest już fizycznie OPŁACONA, to nie pozwalamy na zmiany.
            if (existingPayment.Status == "Paid") return false;

            // NADPISUJEMY starą (porzuconą lub odrzuconą) płatność na nową metodę
            existingPayment.Method = "Offline";
            existingPayment.Status = "Pending";
            existingPayment.Amount = apt.AppointmentType.BasePrice;
            existingPayment.UpdatedAt = DateTime.UtcNow;
        
            await _context.SaveChangesAsync();
            return true;
        }

        // 3. Jeśli to absolutnie pierwsza próba płatności (brak w bazie), tworzymy nową.
        var newPayment = new Payment 
        { 
            AppointmentId = appointmentId, 
            Amount = apt.AppointmentType.BasePrice, 
            Currency = "PLN", 
            Method = "Offline", 
            Status = "Pending", 
            CreatedAt = DateTime.UtcNow, 
            UpdatedAt = DateTime.UtcNow 
        };

        _context.Payments.Add(newPayment);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ConfirmOfflinePaymentAsync(int paymentId, string approvedByUserId, string? comment)
    {
        var payment = await _context.Payments.FirstOrDefaultAsync(p => p.PaymentId == paymentId);
        
        if (payment == null) return false;
        
        // Zabezpieczenie przed podwójnym kliknięciem przycisku przez recepcję.
        if (payment.Status == "Paid") return true;

        payment.Status = "Paid";
        payment.UpdatedAt = DateTime.UtcNow;
        payment.PaidAt = DateTime.UtcNow;

        var approvalLog = new OfflinePaymentApproval 
        { 
            PaymentId = paymentId, 
            ApprovedByUserId = approvedByUserId, 
            Decision = "Approved", 
            DecisionDate = DateTime.UtcNow, 
            Comment = comment 
        };

        _context.OfflinePaymentApprovals.Add(approvalLog);
        await _context.SaveChangesAsync();
        return true;
    }

    // =========================================================================
    // 2. PAYU (ONLINE)
    // =========================================================================
    
    public async Task<string> InitPayuPaymentAsync(int appointmentId)
    {
        var existing = await _context.Payments.FirstOrDefaultAsync(p => p.AppointmentId == appointmentId);
        
        // Nie pozwalamy generować nowego linku, jeśli wizyta jest już opłacona.
        if (existing?.Status == "Paid") 
            throw new Exception("Ta wizyta została już pomyślnie opłacona.");

        var apt = await _context.Appointments
            .Include(a => a.User)
            .Include(a => a.AppointmentType)
            .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

        // Solidne zabezpieczenia przed błędnymi danymi w bazie.
        if (apt?.AppointmentType == null) 
            throw new Exception("Brak danych o cenie dla tego typu wizyty.");
        if (apt.User == null) 
            throw new Exception("Nie znaleziono pacjenta przypisanego do tej wizyty.");

        // Czyścimy starą, porzuconą (Pending/Failed) próbę płatności, by zachować porządek w bazie.
        if (existing != null) 
        {
            _context.Payments.Remove(existing);
        }

        var payment = new Payment 
        { 
            AppointmentId = appointmentId, 
            Amount = apt.AppointmentType.BasePrice, 
            Currency = "PLN", 
            Method = "PayU", 
            Status = "Pending", 
            CreatedAt = DateTime.UtcNow, 
            UpdatedAt = DateTime.UtcNow 
        };
        
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync(); // Musimy zapisać, by baza wygenerowała nowe 'PaymentId'

        // Przekazujemy prośbę do bramki (Delegacja do serwisu PayU)
        var (redirectUri, orderId) = await _payUService.CreateOrderAsync(
            payment.PaymentId,
            payment.Amount,
            $"Wizyta {appointmentId}",
            apt.User.Email!,
            apt.User.FirstName,
            apt.User.LastName
        );
        // Zapamiętujemy ID z PayU na potrzeby późniejszego sprawdzania na Localhost
        _payuOrders[payment.PaymentId] = orderId;
        
        return redirectUri;
    }

    public async Task<string> CheckPayuStatusAsync(int appointmentId)
    {
        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.AppointmentId == appointmentId && p.Method == "PayU");
            
        if (payment == null || !_payuOrders.TryGetValue(payment.PaymentId, out var orderId)) 
            return "UNKNOWN";

        return await _payUService.GetOrderStatusAsync(orderId);
    }

    public async Task<bool> UpdateStatusAsync(int appointmentId, string payuStatus)
    {
        var payment = await _context.Payments
            .Include(p => p.Appointment)
            .FirstOrDefaultAsync(p => p.AppointmentId == appointmentId);
            
        if (payment == null) return false;

        // Jeśli płatność przeszła pomyślnie
        if (payuStatus == "COMPLETED")
        {
            payment.Status = "Paid";
            if (payment.Appointment != null) 
                payment.Appointment.Status = "Confirmed";
        }
        // Jeśli pacjent zamknął okno w banku, albo nie miał środków na koncie
        else if (payuStatus == "CANCELED" || payuStatus == "REJECTED")
        {
            payment.Status = "Failed"; 
            
            // Cofamy wizytę do stanu początkowego, jeśli była przypięta
            if (payment.Appointment != null && payment.Appointment.Status != "Cancelled") 
            {
                payment.Appointment.Status = "Pending";
            }
        }

        payment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        
        // Zwracamy true TYLKO jeśli status końcowy to opłacono
        return payment.Status == "Paid"; 
    }

    // =========================================================================
    // 3. WEBHOOK (POWIADOMIENIA Z PRODUKCJI)
    // =========================================================================
    
    public async Task<bool> ProcessPayUNotificationAsync(PayUNotificationRequest request)
    {
        var status = request.Order.Status;
        
        // Wyciągamy nasze wewnętrzne PaymentId (zbudowane np. jako "123_uuid")
        if (!int.TryParse(request.Order.ExtOrderId.Split('_')[0], out var paymentId)) 
            return false;

        var payment = await _context.Payments
            .Include(p => p.Appointment)
            .FirstOrDefaultAsync(p => p.PaymentId == paymentId);
            
        if (payment == null || payment.Status == "Paid") 
            return true; // Jeśli już kiedyś odebraliśmy ten Webhook, ignorujemy go.

        if (status == "COMPLETED")
        {
            payment.Status = "Paid";
            if (payment.Appointment != null) 
                payment.Appointment.Status = "Confirmed";
        }
        else if (status == "CANCELED" || status == "REJECTED")
        {
            payment.Status = "Failed";
            if (payment.Appointment != null && payment.Appointment.Status != "Cancelled") 
            {
                payment.Appointment.Status = "Pending";
            }
        }

        payment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
}