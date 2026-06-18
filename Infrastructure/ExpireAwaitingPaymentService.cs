using Medreserve.Features.Appointment;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Medreserve.Infrastructure;

public sealed class ExpireAwaitingPaymentService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExpireAwaitingPaymentService> _logger;
    private static readonly TimeSpan ScanInterval = TimeSpan.FromMinutes(1);

    public ExpireAwaitingPaymentService(
        IServiceProvider serviceProvider,
        ILogger<ExpireAwaitingPaymentService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ExpireAwaitingPaymentService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessExpiredAwaitingPaymentAppointmentsAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error while expiring AwaitingPayment appointments");
            }

            await Task.Delay(ScanInterval, stoppingToken);
        }
    }

    private async Task ProcessExpiredAwaitingPaymentAppointmentsAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

        var now = DateTime.UtcNow;

        var expiredAppointments = await dbContext.Appointments
            .Where(a => a.Status == AppointmentStatus.AwaitingPayment)
            .ToListAsync(ct);

        var toExpire = new List<Appointment>();

        foreach (var appointment in expiredAppointments)
        {
            var appointmentStart = appointment.GetStartDateTime();
            if (appointmentStart < now)
            {
                toExpire.Add(appointment);
            }
        }

        if (toExpire.Count == 0) return;

        foreach (var appointment in toExpire)
        {
            appointment.Status = AppointmentStatus.Unpaid;
            appointment.UpdatedAt = now;
        }

        await dbContext.SaveChangesAsync(ct);
        _logger.LogInformation("Expired {Count} AwaitingPayment → Unpaid appointments", toExpire.Count);
    }
}
