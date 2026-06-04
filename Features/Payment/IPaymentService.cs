using Medreserve.Features.Payment.PayU;

namespace Medreserve.Features.Payment;

/// <summary>
/// Kontrakt dla serwisu płatności. Definiuje, jakie operacje można wykonać w systemie, 
/// całkowicie ukrywając przed kontrolerem szczegóły implementacji (np. bazę danych czy API PayU).
/// </summary>
public interface IPaymentService
{
    // =========================================================================
    // 1. PŁATNOŚCI GOTÓWKOWE (W PLACÓWCE)
    // =========================================================================
    
    /// <summary>
    /// Tworzy w bazie tzw. zamiar płatności gotówkowej (Status: Pending).
    /// </summary>
    Task<bool> CreateOfflinePaymentIntentAsync(int appointmentId);
    
    /// <summary>
    /// Używane przez lekarza/recepcję. Zatwierdza fizyczny odbiór gotówki od pacjenta (Status: Paid).
    /// </summary>
    Task<bool> ConfirmOfflinePaymentAsync(int paymentId, string approvedByUserId, string? comment);
    
    
    // =========================================================================
    // 2. PŁATNOŚCI ONLINE (PAYU)
    // =========================================================================
    
    /// <summary>
    /// Inicjuje połączenie z PayU i zwraca link (RedirectUri), pod który należy przekierować pacjenta.
    /// </summary>
    Task<string> InitPayuPaymentAsync(int appointmentId);
    
    /// <summary>
    /// Krok 1 z SRP: Tylko odpytuje serwery PayU o status konkretnego zamówienia.
    /// Nie dotyka naszej bazy danych. Zwraca np. "COMPLETED", "CANCELED" lub "UNKNOWN".
    /// </summary>
    Task<string> CheckPayuStatusAsync(int appointmentId);
    
    /// <summary>
    /// Krok 2 z SRP: Na podstawie statusu pobranego z banku aktualizuje naszą lokalną bazę danych.
    /// Radzi sobie z sukcesami (Paid) oraz odrzuceniami (Failed).
    /// </summary>
    Task<bool> UpdateStatusAsync(int appointmentId, string payuStatus);
    
    
    // =========================================================================
    // 3. OBSŁUGA PRODUKCYJNA (WEBHOOKI)
    // =========================================================================
    
    /// <summary>
    /// Odbiera automatyczne, asynchroniczne powiadomienie od serwerów PayU, że transakcja dobiegła końca.
    /// </summary>
    Task<bool> ProcessPayUNotificationAsync(PayUNotificationRequest request);
}