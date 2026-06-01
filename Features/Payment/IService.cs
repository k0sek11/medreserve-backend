namespace Medreserve.Features.Payment;

using Medreserve.Features.Payment.PayU;

    public interface IPaymentService
    {
        Task<bool> ConfirmOfflinePaymentAsync(int paymentId, string approvedByUserId, string? comment);
        Task<bool> CreateOfflinePaymentIntentAsync(int appointmentId);
        Task<string> InitPayuPaymentAsync(int appointmentId);
        
        Task<bool> CheckAndUpdatePayUStatusAsync(int appointmentId);
        Task<bool> ProcessPayUNotificationAsync(PayUNotificationRequest request);
    }
