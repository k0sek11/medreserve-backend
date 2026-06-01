using System.Security.Claims;
using System.Text.Json;
using Medreserve.Features.Doctor;
using Medreserve.Features.Notification;
using Medreserve.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medreserve.Features.Appointment;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController(DatabaseContext dbContext) : ControllerBase
{
    [HttpGet("mine")]
    public async Task<ActionResult<IReadOnlyList<AppointmentSummaryDto>>> GetMine(CancellationToken cancellationToken)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
        {
            return Unauthorized();
        }

var appointments = await dbContext.Appointments
    .AsNoTracking()
    .Where(x => x.UserId == currentUserId)
    .Include(x => x.Doctor).ThenInclude(x => x.User)
    .Include(x => x.Doctor).ThenInclude(x => x.DoctorSpecializations).ThenInclude(x => x.Specialization)
    .Include(x => x.AppointmentType)
    .Include(x => x.Payments)
    .OrderByDescending(x => x.CreatedAt)
    .ToListAsync(cancellationToken);
        var result = appointments.Select(MapSummary).ToList();
        return Ok(result);
    }

    [HttpGet("{appointmentId:int}")]
    public async Task<ActionResult<AppointmentDetailDto>> GetById(int appointmentId, CancellationToken cancellationToken)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var appointment = await dbContext.Appointments
            .AsNoTracking()
            .Where(x => x.AppointmentId == appointmentId && x.UserId == currentUserId)
            .Include(x => x.Doctor)
                .ThenInclude(x => x.User)
            .Include(x => x.Doctor)
                .ThenInclude(x => x.DoctorSpecializations)
                    .ThenInclude(x => x.Specialization)
            .Include(x => x.AppointmentType)
            .Include(x => x.Payments)
            .FirstOrDefaultAsync(cancellationToken);

        return appointment is null ? NotFound() : Ok(MapDetail(appointment));
    }

    [HttpPost]
    public async Task<ActionResult<BookAppointmentResultDto>> BookAppointment(
        [FromBody] BookAppointmentRequest request,
        CancellationToken cancellationToken
    )
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var userExists = await dbContext.Users
            .AsNoTracking()
            .AnyAsync(x => x.Id == currentUserId, cancellationToken);

        if (!userExists)
        {
            return Unauthorized();
        }

        var doctor = await dbContext.Doctors
            .AsNoTracking()
            .Include(x => x.DoctorSchedules)
            .Include(x => x.DoctorAppointmentTypes)
                .ThenInclude(x => x.AppointmentType)
            .Include(x => x.Appointments)
                .ThenInclude(x => x.AppointmentType)
            .Include(x => x.DoctorSpecializations)
                .ThenInclude(x => x.Specialization)
            .FirstOrDefaultAsync(x => x.DoctorId == request.DoctorId, cancellationToken);

        if (doctor is null)
        {
            return NotFound(new { message = "Doctor not found." });
        }

        var appointmentType = doctor.DoctorAppointmentTypes
            .Select(x => x.AppointmentType)
            .FirstOrDefault(x => x.AppointmentTypeId == request.AppointmentTypeId);

        if (appointmentType is null)
        {
            return BadRequest(new { message = "Appointment type is not available for this doctor." });
        }

        var clinicMatches = await dbContext.ClinicDoctors
            .AsNoTracking()
            .AnyAsync(
                x => x.DoctorId == request.DoctorId && x.ClinicId == request.ClinicId,
                cancellationToken
            );

        if (!clinicMatches)
        {
            return BadRequest(new { message = "Selected clinic is not assigned to this doctor." });
        }

        try
        {
            var startTime = AppointmentSchedulingHelper.ParseTime(request.StartTime);
            var requestedStart = AppointmentSchedulingHelper.ToDateTime(request.Date, startTime);
            var requestedEnd = requestedStart.AddMinutes(appointmentType.DurationMinutes);
            var targetDayOfWeek = AppointmentSchedulingHelper.NormalizeDayOfWeek((int)request.Date.DayOfWeek);

            var scheduleMatches = doctor.DoctorSchedules.Any(schedule =>
                schedule.IsActive
                && schedule.ClinicId == request.ClinicId
                && AppointmentSchedulingHelper.NormalizeDayOfWeek(schedule.DayOfWeek) == targetDayOfWeek
                && DateOnly.FromDateTime(schedule.ValidFrom) <= request.Date
                && (!schedule.ValidTo.HasValue || DateOnly.FromDateTime(schedule.ValidTo.Value) >= request.Date)
                && startTime >= AppointmentSchedulingHelper.ParseTime(schedule.StartTime)
                && TimeOnly.FromDateTime(requestedEnd) <= AppointmentSchedulingHelper.ParseTime(schedule.EndTime)
            );

            if (!scheduleMatches)
            {
                return BadRequest(new { message = "Selected time is outside of the doctor's schedule." });
            }

            var bookedAppointments = doctor.Appointments
                .Where(x => !IsCancelled(x.Status))
                .Select(x => BuildBookedInterval(x))
                .Where(x => x is not null && x!.Value.Date == request.Date)
                .Select(x => x!.Value)
                .ToList();

            var overlapsExisting = bookedAppointments.Any(booked =>
                AppointmentSchedulingHelper.IsOverlapping(requestedStart, requestedEnd, booked.Start, booked.End)
            );

            if (overlapsExisting)
            {
                return Conflict(new { message = "Selected time is already booked." });
            }

            var timeSlotId = AppointmentSchedulingHelper.BuildTimeSlotId(request.DoctorId, request.Date, startTime);

            var appointment = new Appointment
            {
                UserId = currentUserId,
                DoctorId = request.DoctorId,
                AppointmentTypeId = request.AppointmentTypeId,
                AppointmentTypeDurationMinutes = appointmentType.DurationMinutes,
                TimeSlotId = timeSlotId,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            dbContext.Appointments.Add(appointment);

            await dbContext.SaveChangesAsync(cancellationToken);

            var doctorUserId = await dbContext.Doctors
                .AsNoTracking()
                .Where(x => x.DoctorId == request.DoctorId)
                .Select(x => x.UserId)
                .FirstAsync(cancellationToken);

            var patientName = await dbContext.Users
                .AsNoTracking()
                .Where(x => x.Id == currentUserId)
                .Select(x => x.FirstName + " " + x.LastName)
                .FirstAsync(cancellationToken);

            var notificationPayload = new AppointmentBookingPayload(
                appointment.AppointmentId,
                request.DoctorId,
                doctorUserId,
                string.Empty,
                currentUserId,
                patientName,
                appointmentType.Name,
                request.Date,
                startTime.ToString("HH:mm"),
                TimeOnly.FromDateTime(requestedEnd).ToString("HH:mm")
            );

            var doctorName = await dbContext.Users
                .AsNoTracking()
                .Where(x => x.Id == doctorUserId)
                .Select(x => x.FirstName + " " + x.LastName)
                .FirstAsync(cancellationToken);

            var serializedPayload = JsonSerializer.Serialize(
                notificationPayload with { DoctorName = doctorName }
            );

            dbContext.Notifications.Add(new Notification.Notification
            {
                UserId = doctorUserId,
                AppointmentId = appointment.AppointmentId,
                Type = NotificationKinds.AppointmentBooked,
                Subject = "Nowa wizyta do potwierdzenia",
                Content = serializedPayload,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
            });

            await dbContext.SaveChangesAsync(cancellationToken);

            var doctorSpecialization = doctor.DoctorSpecializations.Select(x => x.Specialization.Name).FirstOrDefault() ?? string.Empty;

            return Ok(
                new BookAppointmentResultDto(
                    appointment.AppointmentId,
                    request.DoctorId,
                    appointmentType.AppointmentTypeId,
                    request.Date,
                    startTime.ToString("HH:mm"),
                    TimeOnly.FromDateTime(requestedEnd).ToString("HH:mm"),
                    appointment.Status,
                    doctorName,
                    doctorSpecialization
                )
            );
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (DbUpdateException)
        {
            return Conflict(new { message = "Selected time is no longer available." });
        }
    }

    private static bool IsCancelled(string status)
    {
        return status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase)
            || status.Equals("Canceled", StringComparison.OrdinalIgnoreCase)
            || status.Equals("Rejected", StringComparison.OrdinalIgnoreCase);
    }

    private static (DateOnly Date, DateTime Start, DateTime End)? BuildBookedInterval(Appointment appointment)
    {
        try
        {
            var (_, date, startTime) = AppointmentSchedulingHelper.DecodeTimeSlotId(appointment.TimeSlotId);
            var start = AppointmentSchedulingHelper.ToDateTime(date, startTime);
            var end = start.AddMinutes(appointment.AppointmentTypeDurationMinutes);
            return (date, start, end);
        }
        catch
        {
            return null;
        }
    }

    private static AppointmentSummaryDto MapSummary(Appointment appointment)
    {
        var (_, date, startTime) = AppointmentSchedulingHelper.DecodeTimeSlotId(appointment.TimeSlotId);
        var endTime = startTime.AddMinutes(appointment.AppointmentTypeDurationMinutes);

        var latestPayment = appointment.Payments.OrderByDescending(p => p.CreatedAt).FirstOrDefault();

        return new AppointmentSummaryDto(
            appointment.AppointmentId,
            appointment.DoctorId,
            $"{appointment.Doctor.User.FirstName} {appointment.Doctor.User.LastName}",
            appointment.Doctor.DoctorSpecializations.Select(x => x.Specialization.Name).FirstOrDefault() ?? string.Empty,
            appointment.AppointmentType?.Name,
            date,
            startTime.ToString("HH:mm"),
            endTime.ToString("HH:mm"),
            appointment.Status,
        
            latestPayment?.PaymentId,
            latestPayment?.Status,
            latestPayment?.Method,
        appointment.AppointmentType?.BasePrice ?? 0
        );
    }

    private static AppointmentDetailDto MapDetail(Appointment appointment)
    {
        var (_, date, startTime) = AppointmentSchedulingHelper.DecodeTimeSlotId(appointment.TimeSlotId);
        var endTime = startTime.AddMinutes(appointment.AppointmentTypeDurationMinutes);
        var latestPayment = appointment.Payments.OrderByDescending(p => p.CreatedAt).FirstOrDefault();

        return new AppointmentDetailDto(
            appointment.AppointmentId,
            appointment.DoctorId,
            $"{appointment.Doctor.User.FirstName} {appointment.Doctor.User.LastName}",
            appointment.Doctor.DoctorSpecializations.Select(x => x.Specialization.Name).FirstOrDefault() ?? string.Empty,
            appointment.AppointmentType?.Name,
            date,
            startTime.ToString("HH:mm"),
            endTime.ToString("HH:mm"),
            appointment.Status,
            appointment.CreatedAt,
            latestPayment?.PaymentId,
                    latestPayment?.Status,
                    latestPayment?.Method
        );
    }
}

public sealed record BookAppointmentRequest(
    int DoctorId,
    int AppointmentTypeId,
    int ClinicId,
    DateOnly Date,
    string StartTime
);

public sealed record BookAppointmentResultDto(
    int AppointmentId,
    int DoctorId,
    int AppointmentTypeId,
    DateOnly Date,
    string StartTime,
    string EndTime,
    string Status,
    string DoctorName,
    string DoctorSpecialization
);
