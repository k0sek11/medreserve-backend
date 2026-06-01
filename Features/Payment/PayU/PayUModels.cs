using System.Text.Json.Serialization;

namespace Medreserve.Features.Payment.PayU;

public class PayUOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public string PosId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string SecondKey { get; set; } = string.Empty;
}

public class PayUAuthResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;
    
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}

public class PayUOrderCreateRequest
{
    public string notifyUrl { get; set; } = string.Empty; 
    public string continueUrl { get; set; } = string.Empty;
    public string customerIp { get; set; } = string.Empty;
    public string merchantPosId { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public string currencyCode { get; set; } = "PLN";
    public string totalAmount { get; set; } = string.Empty; 
    public string extOrderId { get; set; } = string.Empty; 
    
    public PayUBuyer buyer { get; set; } = new();
    public List<PayUProduct> products { get; set; } = new();
}

public class PayUBuyer
{
    public string email { get; set; } = string.Empty;
    public string phone { get; set; } = string.Empty;
    public string firstName { get; set; } = string.Empty;
    public string lastName { get; set; } = string.Empty;
    public string language { get; set; } = "pl";
}

public class PayUProduct
{
    public string name { get; set; } = string.Empty;
    public string unitPrice { get; set; } = string.Empty;
    public string quantity { get; set; } = "1";
}

public class PayUOrderCreateResponse
{
    public PayUStatus status { get; set; } = new();
    public string redirectUri { get; set; } = string.Empty;
    public string orderId { get; set; } = string.Empty; 
    public string continueUrl { get; set; } = string.Empty; 
}

public class PayUStatus
{
    public string statusCode { get; set; } = string.Empty;
}

public class PayUNotificationRequest
{
    public PayUNotificationOrder Order { get; set; } = new();
}

public class PayUNotificationOrder
{
    public string ExtOrderId { get; set; } = string.Empty; 
    public string Status { get; set; } = string.Empty;     
}

public class PayUOrderDetailsResponse
{
    [JsonPropertyName("orders")]
    public List<PayUOrderDetailsItem> Orders { get; set; } = new();
}

public class PayUOrderDetailsItem
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty; 
}