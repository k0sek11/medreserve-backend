using System.Collections.Concurrent;
using Medreserve.Features.Appointment;
using Medreserve.Features.Payment.PayU;
using Medreserve.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Medreserve.Features.Payment;

using PaymentEntity = Medreserve.Features.Payment.Payment;

public class PaymentService(DatabaseContext _context, IPayUService _payUService) : IPaymentService
{
    private static readonly ConcurrentDictionary<int, string> _payuOrders = new();


    public async Task<bool> CreateOfflinePaymentIntentAsync(int appointmentId)
    {
        var apt = await _context.Appointments
            .Include(a => a.AppointmentType)
            .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

        if (apt?.AppointmentType == null) return false;

        var existingPayment = await _context.Payments
            .FirstOrDefaultAsync(p => p.AppointmentId == appointmentId);

        if (existingPayment != null)
        {
            if (existingPayment.Status == "Paid") return false;

            existingPayment.Method = "Offline";
            existingPayment.Status = "Pending";
            existingPayment.Amount = apt.AppointmentType.BasePrice;
            existingPayment.UpdatedAt = DateTime.UtcNow;

            apt.Status = AppointmentStatus.AwaitingOnSitePayment;

            await _context.SaveChangesAsync();
            return true;
        }

        var newPayment = new PaymentEntity
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

        apt.Status = AppointmentStatus.AwaitingOnSitePayment;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ConfirmOfflinePaymentAsync(int paymentId, string approvedByUserId, string? comment)
    {
        var payment = await _context.Payments
            .Include(p => p.Appointment)
            .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

        if (payment == null) return false;
        if (payment.Status == "Paid") return true;

        if (payment.Appointment != null)
        {
            var now = DateTime.UtcNow;
            var localStart = payment.Appointment.GetStartDateTime();
            
            var polishTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Warsaw");
            
            var startUtc = TimeZoneInfo.ConvertTimeToUtc(localStart, polishTimeZone);

            if (startUtc > now)
            {
                throw new InvalidOperationException("Nie można potwierdzić płatności przed rozpoczęciem wizyty.");
            }
        }

        

        payment.Status = "Paid";
        payment.UpdatedAt = DateTime.UtcNow;
        payment.PaidAt = DateTime.UtcNow;

        if (payment.Appointment != null)
        {
            payment.Appointment.Status = AppointmentStatus.Confirmed;
        }

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

    public async Task<string> InitPayuPaymentAsync(int appointmentId)
    {
        var existing = await _context.Payments.FirstOrDefaultAsync(p => p.AppointmentId == appointmentId);

        if (existing?.Status == "Paid")
            throw new Exception("Ta wizyta została już pomyślnie opłacona.");

        var apt = await _context.Appointments
            .Include(a => a.User)
            .Include(a => a.AppointmentType)
            .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

        if (apt?.AppointmentType == null)
            throw new Exception("Brak danych o cenie dla tego typu wizyty.");
        if (apt.User == null)
            throw new Exception("Nie znaleziono pacjenta przypisanego do tej wizyty.");

        if (existing != null)
        {
            _context.Payments.Remove(existing);
        }

        var payment = new PaymentEntity
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
        await _context.SaveChangesAsync();

        var (redirectUri, orderId) = await _payUService.CreateOrderAsync(
            payment.PaymentId,
            payment.Amount,
            $"Wizyta {appointmentId}",
            apt.User.Email!,
            apt.User.FirstName,
            apt.User.LastName
        );

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

        if (payuStatus == "COMPLETED")
        {
            payment.Status = "Paid";
            if (payment.Appointment != null)
                payment.Appointment.Status = AppointmentStatus.Confirmed;
        }
        else if (payuStatus == "CANCELED" || payuStatus == "REJECTED")
        {
            payment.Status = "Failed";

            if (payment.Appointment != null && payment.Appointment.Status != AppointmentStatus.Cancelled)
            {
                payment.Appointment.Status = AppointmentStatus.PendingConfirmation;
            }
        }

        payment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return payment.Status == "Paid";
    }

    public async Task<bool> ProcessPayUNotificationAsync(PayUNotificationRequest request)
    {
        var status = request.Order.Status;

        if (!int.TryParse(request.Order.ExtOrderId.Split('_')[0], out var paymentId))
            return false;

        var payment = await _context.Payments
            .Include(p => p.Appointment)
            .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

        if (payment == null || payment.Status == "Paid")
            return true;

        if (status == "COMPLETED")
        {
            payment.Status = "Paid";
            if (payment.Appointment != null)
                payment.Appointment.Status = AppointmentStatus.Confirmed;
        }
        else if (status == "CANCELED" || status == "REJECTED")
        {
            payment.Status = "Failed";
            if (payment.Appointment != null && payment.Appointment.Status != AppointmentStatus.Cancelled)
            {
                payment.Appointment.Status = AppointmentStatus.PendingConfirmation;
            }
        }

        payment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
}