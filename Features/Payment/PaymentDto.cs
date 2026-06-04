namespace Medreserve.Features.Payment;

// Rekordy to nowoczesny, skrócony zapis klas w C#, idealny do przesyłania prostych danych z frontendu.
public record InitPaymentDto(int AppointmentId);

public record ConfirmOfflinePaymentDto(string? Comment);