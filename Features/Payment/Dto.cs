namespace Medreserve.Features.Payment.DTOs
{
    public class ConfirmOfflinePaymentDto
    {
        public string? Comment { get; set; }
    }
    public class InitPaymentDto
    {
        public int AppointmentId { get; set; }
    }

}