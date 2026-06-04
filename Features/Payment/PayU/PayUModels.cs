using System.Text.Json.Serialization;

namespace Medreserve.Features.Payment.PayU;

// 1. Konfiguracja z appsettings.json
public class PayUOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public string PosId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}

// 2. Modele do odbioru powiadomień (Webhook) używane w PaymentsController
public class PayUNotificationRequest
{
    [JsonPropertyName("order")]
    public PayUNotificationOrder Order { get; set; } = new();
}

public class PayUNotificationOrder
{
    [JsonPropertyName("extOrderId")]
    public string ExtOrderId { get; set; } = string.Empty; 
    
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;     
}