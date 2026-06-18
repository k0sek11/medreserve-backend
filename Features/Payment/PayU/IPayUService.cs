namespace Medreserve.Features.Payment.PayU;

public interface IPayUService
{
    Task<(string RedirectUri, string OrderId)> CreateOrderAsync(int paymentId, decimal amount, string description, string patientEmail, string patientFirstName, string patientLastName);
    Task<string> GetOrderStatusAsync(string orderId);
}
