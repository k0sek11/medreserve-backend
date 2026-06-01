using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Medreserve.Features.Payment.PayU;

public class PayUService : IPayUService
{
    private readonly HttpClient _httpClient;
    private readonly PayUOptions _payUOptions;

    public PayUService(HttpClient httpClient, IOptions<PayUOptions> options)
    {
        _httpClient = httpClient;
        _payUOptions = options.Value;
    }

    private async Task<string> GetAccessTokenAsync()
    {
        var dict = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", _payUOptions.ClientId },
            { "client_secret", _payUOptions.ClientSecret }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, $"{_payUOptions.BaseUrl}/pl/standard/user/oauth/authorize")
        {
            Content = new FormUrlEncodedContent(dict)
        };

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<PayUAuthResponse>(content);
        
        return authResponse?.AccessToken ?? throw new Exception("Nie udało się pobrać tokenu dostępu z systemu PayU.");
    }

    public async Task<(string RedirectUri, string OrderId)> CreateOrderAsync(
        int paymentId, 
        decimal amount, 
        string description, 
        string patientEmail, 
        string patientFirstName, 
        string patientLastName)
    {
        var token = await GetAccessTokenAsync();

        var totalAmountGrosze = (int)(amount * 100);

        var orderRequest = new PayUOrderCreateRequest
        {
            notifyUrl = "https://twoja-aplikacja.pl/api/payments/payu-notify", 
            continueUrl = "http://localhost:5000/moje-wizyty",
            customerIp = "127.0.0.1",
            merchantPosId = _payUOptions.PosId,
            description = description,
            currencyCode = "PLN",
            totalAmount = totalAmountGrosze.ToString(),
            extOrderId = $"{paymentId}_{Guid.NewGuid():N}", 
            buyer = new PayUBuyer
            {
                email = patientEmail,
                firstName = patientFirstName,
                lastName = patientLastName,
                language = "pl"
            },
            products = new List<PayUProduct>
            {
                new PayUProduct
                {
                    name = description,
                    unitPrice = totalAmountGrosze.ToString(),
                    quantity = "1"
                }
            }
        };

        var jsonRequest = JsonSerializer.Serialize(orderRequest);
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_payUOptions.BaseUrl}/api/v2_1/orders")
        {
            Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json")
        };
        
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.Redirect)
        {
            throw new Exception($"Błąd komunikacji z PayU (Status: {response.StatusCode}): {content}");
        }

        var orderResponse = JsonSerializer.Deserialize<PayUOrderCreateResponse>(content);
        
        return (
            orderResponse?.redirectUri ?? throw new Exception("PayU nie zwróciło adresu przekierowania (redirectUri)."),
            orderResponse?.orderId ?? throw new Exception("PayU nie zwróciło identyfikatora zamówienia (orderId).")
        );
    }

    public async Task<string> GetOrderStatusAsync(string orderId)
    {
        var token = await GetAccessTokenAsync();
        
        var request = new HttpRequestMessage(HttpMethod.Get, $"{_payUOptions.BaseUrl}/api/v2_1/orders/{orderId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode) 
        {
            return "UNKNOWN";
        }

        var details = JsonSerializer.Deserialize<PayUOrderDetailsResponse>(content);
        
        return details?.Orders?.FirstOrDefault()?.Status ?? "UNKNOWN";
    }
}