using Medreserve.Infrastructure; 
using Medreserve.Features.Payment.PayU;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace Medreserve.Features.Payment;

public class PaymentService(DatabaseContext _context, IPayUService _payUService) : IPaymentService
{
    private static readonly ConcurrentDictionary<int, string> _payuOrderMapping = new();


    public async Task<bool> ConfirmOfflinePaymentAsync(int paymentId, string approvedByUserId, string? comment)
    {
        var payment = await _context.Payments
            .Include(p => p.OfflinePaymentApproval) 
            .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

        if (payment == null) return false;
        if (payment.Status == "Paid") return true; 

        payment.Status = "Paid";
        payment.UpdatedAt = DateTime.UtcNow;
        payment.PaidAt = DateTime.UtcNow;

        var approval = new OfflinePaymentApproval
        {
            PaymentId = paymentId,
            ApprovedByUserId = approvedByUserId,
            Decision = "Approved",
            DecisionDate = DateTime.UtcNow,
            Comment = comment
        };

        _context.OfflinePaymentApprovals.Add(approval);
        await _context.SaveChangesAsync();

        return true;
    }


    public async Task<bool> CreateOfflinePaymentIntentAsync(int appointmentId)
    {
        var existingPayment = await _context.Payments
            .FirstOrDefaultAsync(p => p.AppointmentId == appointmentId);

        if (existingPayment != null) return false; 

        var appointment = await _context.Appointments
            .Include(a => a.AppointmentType)
            .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

        if (appointment == null || appointment.AppointmentType == null)
            throw new Exception("Nie znaleziono wizyty lub typu wizyty (brak ceny).");

        var price = appointment.AppointmentType.BasePrice; 
        
        var payment = new Payment
        {
            AppointmentId = appointmentId,
            Amount = price, 
            Currency = "PLN",
            Method = "Offline",
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        return true;        
    }


    public async Task<string> InitPayuPaymentAsync(int appointmentId)
    {
        var existingPayment = await _context.Payments
            .FirstOrDefaultAsync(p => p.AppointmentId == appointmentId);
    
        if (existingPayment != null && existingPayment.Status == "Paid")
            throw new Exception("Ta wizyta została już pomyślnie opłacona.");
    
        var appointment = await _context.Appointments
            .Include(a => a.User)
            .Include(a => a.AppointmentType)
            .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);
    
        if (appointment == null || appointment.AppointmentType == null)
            throw new Exception("Nie znaleziono wizyty lub typu wizyty (brak ceny).");
    
        var price = appointment.AppointmentType.BasePrice; 
    
        if (existingPayment != null)
        {
            _context.Payments.Remove(existingPayment);
            await _context.SaveChangesAsync();
        }

        var payment = new Payment
        {
            AppointmentId = appointmentId,
            Amount = price,
            Currency = "PLN",
            Method = "PayU",
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();
    
        var description = $"Wizyta: {appointment.AppointmentType.Name} - ID: {appointmentId}";
        
        var (redirectUri, payuOrderId) = await _payUService.CreateOrderAsync(
            paymentId: payment.PaymentId,
            amount: price,
            description: description,
            patientEmail: appointment.User.Email!,
            patientFirstName: appointment.User.FirstName,
            patientLastName: appointment.User.LastName
        );
    
        _payuOrderMapping[payment.PaymentId] = payuOrderId;

        return redirectUri;        
    }


    public async Task<bool> CheckAndUpdatePayUStatusAsync(int appointmentId)
    {
        var payment = await _context.Payments
            .Include(p => p.Appointment)
            .FirstOrDefaultAsync(p => p.AppointmentId == appointmentId && p.Status == "Pending" && p.Method == "PayU");

        if (payment == null) return false;

        if (_payuOrderMapping.TryGetValue(payment.PaymentId, out var orderId))
        {
            var payuStatus = await _payUService.GetOrderStatusAsync(orderId);

            if (payuStatus == "COMPLETED")
            {
                payment.Status = "Paid";
                payment.UpdatedAt = DateTime.UtcNow;

                if (payment.Appointment != null)
                {
                    payment.Appointment.Status = "Confirmed";
                    payment.Appointment.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                return true;
            }
        }
        return false;
    }

 
    public async Task<bool> ProcessPayUNotificationAsync(PayUNotificationRequest request)
    {
        if (request.Order.Status != "COMPLETED")
        {
            return false;
        }
    
        var parts = request.Order.ExtOrderId.Split('_');
        if (parts.Length == 0 || !int.TryParse(parts[0], out var paymentId))
        {
            return false;
        }
    
        var payment = await _context.Payments
            .Include(p => p.Appointment)
            .FirstOrDefaultAsync(p => p.PaymentId == paymentId);
    
        if (payment == null) return false;
        if (payment.Status == "Paid") return true;
    
        payment.Status = "Paid";
        payment.UpdatedAt = DateTime.UtcNow;
    
        if (payment.Appointment != null && payment.Appointment.Status == "Pending")
        {
            payment.Appointment.Status = "Confirmed";
            payment.Appointment.UpdatedAt = DateTime.UtcNow;
        }
    
        await _context.SaveChangesAsync();
        return true;
    }
}