using System.Text.Json.Serialization;

namespace Medreserve.Features.Payment.PayU;

public class PayUOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public string PosId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}

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
