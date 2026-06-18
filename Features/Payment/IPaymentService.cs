using Medreserve.Features.Payment.PayU;

namespace Medreserve.Features.Payment;
public interface IPaymentService
{
                Task<bool> CreateOfflinePaymentIntentAsync(int appointmentId);
    Task<bool> ConfirmOfflinePaymentAsync(int paymentId, string approvedByUserId, string? comment);
    
    
                Task<string> InitPayuPaymentAsync(int appointmentId);
    Task<string> CheckPayuStatusAsync(int appointmentId);
    Task<bool> UpdateStatusAsync(int appointmentId, string payuStatus);
    
    
                Task<bool> ProcessPayUNotificationAsync(PayUNotificationRequest request);
}
