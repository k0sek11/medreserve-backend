using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Medreserve.Features.Payment.PayU;

public class PayUService(HttpClient _httpClient, IOptions<PayUOptions> _options) : IPayUService
{
    private readonly PayUOptions _payu = _options.Value;


    private async Task<string> GetTokenAsync()
    {
        var dict = new Dictionary<string, string> 
        { 
            { "grant_type", "client_credentials" }, 
            { "client_id", _payu.ClientId }, 
            { "client_secret", _payu.ClientSecret } 
        };
        
        var req = new HttpRequestMessage(HttpMethod.Post, $"{_payu.BaseUrl}/pl/standard/user/oauth/authorize") 
        { 
            Content = new FormUrlEncodedContent(dict) 
        };
        
        var res = await _httpClient.SendAsync(req);
        
        if (!res.IsSuccessStatusCode)
        {
            var errorBody = await res.Content.ReadAsStringAsync();
            throw new HttpRequestException($"PayU odrzuciło prośbę o token. Status: {res.StatusCode}, Detale: {errorBody}");
        }

        using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
        
        return doc.RootElement.GetProperty("access_token").GetString() 
               ?? throw new Exception("Odpowiedź PayU nie zawierała tokenu.");
    }

    public async Task<(string RedirectUri, string OrderId)> CreateOrderAsync(int paymentId, decimal amount, string desc, string email, string firstName, string lastName)
    {
        var token = await GetTokenAsync();
        var totalAmountGrosze = ((int)(amount * 100)).ToString();
        
        var body = new 
        {
            notifyUrl = "https://twoja-aplikacja.pl/api/payments/payu-notify", 
            continueUrl = "http://localhost:5000/moje-wizyty", 
            customerIp = "127.0.0.1", 
            merchantPosId = _payu.PosId, 
            description = desc, 
            currencyCode = "PLN", 
            totalAmount = totalAmountGrosze, 
            extOrderId = $"{paymentId}_{Guid.NewGuid():N}", // Zabezpieczenie PayU przed duplikatem ID
            buyer = new 
            { 
                email = email, 
                firstName = firstName, 
                lastName = lastName, 
                language = "pl" 
            },
            products = new[] 
            { 
                new 
                { 
                    name = desc, 
                    unitPrice = totalAmountGrosze, 
                    quantity = "1" 
                } 
            }
        };

        var req = new HttpRequestMessage(HttpMethod.Post, $"{_payu.BaseUrl}/api/v2_1/orders") 
        { 
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json") 
        };
        
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var res = await _httpClient.SendAsync(req);

        if (!res.IsSuccessStatusCode && res.StatusCode != System.Net.HttpStatusCode.Redirect)
        {
            var errorBody = await res.Content.ReadAsStringAsync();
            throw new HttpRequestException($"PayU odrzuciło zamówienie. Status: {res.StatusCode}, Detale: {errorBody}");
        }

        using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
        
        var redirectUri = doc.RootElement.GetProperty("redirectUri").GetString();
        var orderId = doc.RootElement.GetProperty("orderId").GetString();

        if (string.IsNullOrEmpty(redirectUri) || string.IsNullOrEmpty(orderId))
            throw new Exception("Brak krytycznych danych (redirectUri lub orderId) w odpowiedzi od PayU.");

        return (redirectUri, orderId);
    }

    public async Task<string> GetOrderStatusAsync(string orderId)
    {
        var token = await GetTokenAsync();
        var req = new HttpRequestMessage(HttpMethod.Get, $"{_payu.BaseUrl}/api/v2_1/orders/{orderId}");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        var res = await _httpClient.SendAsync(req);
        
        if (!res.IsSuccessStatusCode) return "UNKNOWN";

        try
        {
            using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
            var ordersArray = doc.RootElement.GetProperty("orders");
            
            if (ordersArray.GetArrayLength() > 0)
            {
                return ordersArray[0].GetProperty("status").GetString() ?? "UNKNOWN";
            }
        }
        catch (KeyNotFoundException)
        {
            return "UNKNOWN";
        }

        return "UNKNOWN";
    }
}