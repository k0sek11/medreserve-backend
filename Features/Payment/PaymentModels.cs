using Medreserve.Features.Appointment;
using Medreserve.Features.Users;

namespace Medreserve.Features.Payment;

public class Payment
{
    public int PaymentId { get; set; }
    public int AppointmentId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "PLN";
    public string Method { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? PaidAt { get; set; }

    public Appointment.Appointment Appointment { get; set; } = null!;
    public OfflinePaymentApproval? OfflinePaymentApproval { get; set; }
}

public class OfflinePaymentApproval
{
    public int ApprovalId { get; set; }
    public int PaymentId { get; set; }
    public string ApprovedByUserId { get; set; } = string.Empty;
    public string Decision { get; set; } = string.Empty;
    public DateTime DecisionDate { get; set; }
    public string? Comment { get; set; }

    public Payment Payment { get; set; } = null!;
    public Medreserve.Features.Users.User ApprovedByUser { get; set; } = null!;
}