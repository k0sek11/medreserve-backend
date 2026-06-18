namespace Medreserve.Features.Payment;

public record InitPaymentDto(int AppointmentId);

public record ConfirmOfflinePaymentDto(string? Comment);
