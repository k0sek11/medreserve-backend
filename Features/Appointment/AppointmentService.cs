using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Medreserve.Infrastructure;
using Medreserve.Features.Notification;

namespace Medreserve.Features.Appointment;

public class AppointmentService(DatabaseContext dbContext) : IAppointmentService
{
    private readonly DatabaseContext _dbContext = dbContext;

    public async Task<BookAppointmentResultDto> BookAppointmentAsync(string userId, BookAppointmentRequest request, CancellationToken cancellationToken)
    {
        await EnsureUserExistsAsync(userId, cancellationToken);
        var doctor = await GetDoctorAsync(request.DoctorId, cancellationToken);
        var appointmentType = GetAppointmentTypeFromDoctor(doctor, request.AppointmentTypeId);

        await ValidateClinicAssignmentAsync(request.DoctorId, request.ClinicId, cancellationToken);

        var startTime = AppointmentSchedulingHelper.ParseTime(request.StartTime);
        var requestedStart = AppointmentSchedulingHelper.ToDateTime(request.Date, startTime);
        var requestedEnd = requestedStart.AddMinutes(appointmentType.DurationMinutes);

        ValidateSchedule(doctor, request.ClinicId, request.Date, startTime, requestedEnd);
        ValidateOverlaps(doctor, request.Date, requestedStart, requestedEnd);

        var appointment = CreateAppointmentEntity(
            userId,
            request.DoctorId,
            appointmentType.AppointmentTypeId,
            appointmentType.DurationMinutes,
            request.Date,
            startTime
        );

        _dbContext.Appointments.Add(appointment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await CreateBookingNotificationAsync(appointment, doctor, appointmentType, requestedEnd, cancellationToken);

        return MapToResult(appointment, doctor, appointmentType, request.Date, startTime, requestedEnd);
    }

    public async Task ConfirmAppointmentAsync(string userId, int appointmentId, bool isOnline, CancellationToken cancellationToken)
    {
        var appointment = await GetAppointmentEntityAsync(appointmentId, cancellationToken);

        EnsureUserIsDoctorForAppointment(appointment, userId);

        if (appointment.Status != AppointmentStatus.PendingConfirmation)
            throw new ArgumentException("Only pending appointments can be confirmed.");

        appointment.Status = isOnline ? AppointmentStatus.AwaitingPayment : AppointmentStatus.Confirmed;
        appointment.ConfirmedAt = DateTime.UtcNow;
        appointment.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task CancelAppointmentAsync(string userId, int appointmentId, CancellationToken cancellationToken)
    {
        var appointment = await GetAppointmentEntityAsync(appointmentId, cancellationToken);

        EnsureUserCanModifyAppointment(appointment, userId);

        if (!CanBeCancelled(appointment.Status))
            throw new ArgumentException("Cannot cancel a confirmed or completed appointment.");

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.CancelledAt = DateTime.UtcNow;
        appointment.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task CompleteAppointmentAsync(string userId, int appointmentId, string? comment, CancellationToken cancellationToken)
    {
        var appointment = await GetAppointmentEntityAsync(appointmentId, cancellationToken);

        EnsureUserIsDoctorForAppointment(appointment, userId);

        if (appointment.Status != AppointmentStatus.Confirmed)
            throw new ArgumentException("Only confirmed appointments can be marked as completed.");

        appointment.Status = AppointmentStatus.Completed;
        appointment.DoctorNotes = comment;
        appointment.CompletedAt = DateTime.UtcNow;
        appointment.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AppointmentSummaryDto>> GetMyAppointmentsAsync(string userId, CancellationToken cancellationToken)
    {
        var appointments = await _dbContext.Appointments
            .AsNoTracking()
            .Where(x => x.UserId == userId || x.Doctor.UserId == userId)
            .Include(x => x.Doctor).ThenInclude(x => x.User)
            .Include(x => x.Doctor).ThenInclude(x => x.DoctorSpecializations).ThenInclude(x => x.Specialization)
            .Include(x => x.AppointmentType)
            .Include(x => x.Payments)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return appointments.Select(MapSummary).ToList();
    }

    public async Task<AppointmentDetailDto?> GetAppointmentByIdAsync(string userId, int appointmentId, CancellationToken cancellationToken)
    {
        var appointment = await _dbContext.Appointments
            .AsNoTracking()
            .Where(x => x.AppointmentId == appointmentId && (x.UserId == userId || x.Doctor.UserId == userId))
            .Include(x => x.Doctor).ThenInclude(x => x.User)
            .Include(x => x.Doctor).ThenInclude(x => x.DoctorSpecializations).ThenInclude(x => x.Specialization)
            .Include(x => x.AppointmentType)
            .Include(x => x.Payments)
            .FirstOrDefaultAsync(cancellationToken);

        return appointment is null ? null : MapDetail(appointment);
    }

    // ───────────────────────────── Private Helpers ─────────────────────────────

    private async Task EnsureUserExistsAsync(string userId, CancellationToken ct)
    {
        var exists = await _dbContext.Users.AsNoTracking().AnyAsync(u => u.Id == userId, ct);
        if (!exists) throw new UnauthorizedAccessException("User not found.");
    }

    private async Task<Doctor.Doctor> GetDoctorAsync(int doctorId, CancellationToken ct)
    {
        var doctor = await _dbContext.Doctors
            .Include(x => x.User)
            .Include(x => x.DoctorSchedules)
            .Include(x => x.DoctorAppointmentTypes).ThenInclude(x => x.AppointmentType)
            .Include(x => x.Appointments)
            .Include(x => x.DoctorSpecializations).ThenInclude(x => x.Specialization)
            .FirstOrDefaultAsync(d => d.DoctorId == doctorId, ct);

        if (doctor is null) throw new ArgumentException("Doctor not found.");
        return doctor;
    }

    private static AppointmentType.AppointmentType GetAppointmentTypeFromDoctor(Doctor.Doctor doctor, int typeId)
    {
        var type = doctor.DoctorAppointmentTypes
            .Select(x => x.AppointmentType)
            .FirstOrDefault(x => x.AppointmentTypeId == typeId);

        if (type is null) throw new ArgumentException("Appointment type is not available for this doctor.");
        return type;
    }

    private async Task ValidateClinicAssignmentAsync(int doctorId, int clinicId, CancellationToken ct)
    {
        var matches = await _dbContext.ClinicDoctors
            .AsNoTracking()
            .AnyAsync(x => x.DoctorId == doctorId && x.ClinicId == clinicId, ct);

        if (!matches) throw new ArgumentException("Selected clinic is not assigned to this doctor.");
    }

    private static void ValidateSchedule(Doctor.Doctor doctor, int clinicId, DateOnly requestDate, TimeOnly startTime, DateTime requestedEnd)
    {
        var targetDayOfWeek = AppointmentSchedulingHelper.NormalizeDayOfWeek((int)requestDate.DayOfWeek);

        var scheduleMatches = doctor.DoctorSchedules.Any(schedule =>
            schedule.IsActive
            && schedule.ClinicId == clinicId
            && AppointmentSchedulingHelper.NormalizeDayOfWeek(schedule.DayOfWeek) == targetDayOfWeek
            && DateOnly.FromDateTime(schedule.ValidFrom) <= requestDate
            && (!schedule.ValidTo.HasValue || DateOnly.FromDateTime(schedule.ValidTo.Value) >= requestDate)
            && startTime >= AppointmentSchedulingHelper.ParseTime(schedule.StartTime)
            && TimeOnly.FromDateTime(requestedEnd) <= AppointmentSchedulingHelper.ParseTime(schedule.EndTime)
        );

        if (!scheduleMatches) throw new ArgumentException("Selected time is outside of the doctor's schedule.");
    }

    private static void ValidateOverlaps(Doctor.Doctor doctor, DateOnly requestDate, DateTime requestedStart, DateTime requestedEnd)
    {
        var bookedAppointments = doctor.Appointments
            .Where(x => !IsCancelled(x.Status))
            .Where(x => x.AppointmentDate == requestDate)
            .Select(x => (Start: x.GetStartDateTime(), End: x.GetEndDateTime()))
            .ToList();

        var overlapsExisting = bookedAppointments.Any(booked =>
            AppointmentSchedulingHelper.IsOverlapping(requestedStart, requestedEnd, booked.Start, booked.End));

        if (overlapsExisting) throw new ArgumentException("Selected time is already booked.");
    }

    private static Appointment CreateAppointmentEntity(string userId, int doctorId, int appointmentTypeId, int durationMinutes, DateOnly date, TimeOnly startTime)
    {
        return new Appointment
        {
            UserId = userId,
            DoctorId = doctorId,
            AppointmentTypeId = appointmentTypeId,
            AppointmentTypeDurationMinutes = durationMinutes,
            AppointmentDate = date,
            StartTime = startTime,
            Status = AppointmentStatus.PendingConfirmation,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private async Task CreateBookingNotificationAsync(Appointment appointment, Doctor.Doctor doctor, AppointmentType.AppointmentType type, DateTime requestedEnd, CancellationToken ct)
    {
        var patientName = await _dbContext.Users
            .AsNoTracking()
            .Where(x => x.Id == appointment.UserId)
            .Select(x => x.FirstName + " " + x.LastName)
            .FirstAsync(ct);

        var notificationPayload = new AppointmentBookingPayload(
            appointment.AppointmentId,
            appointment.DoctorId,
            doctor.UserId,
            $"{doctor.User.FirstName} {doctor.User.LastName}",
            appointment.UserId,
            patientName,
            type.Name,
            appointment.AppointmentDate,
            appointment.StartTime.ToString("HH:mm"),
            TimeOnly.FromDateTime(requestedEnd).ToString("HH:mm")
        );

        var serializedPayload = JsonSerializer.Serialize(notificationPayload);

        _dbContext.Notifications.Add(new Notification.Notification
        {
            UserId = doctor.UserId,
            AppointmentId = appointment.AppointmentId,
            Type = NotificationKinds.AppointmentBooked,
            Subject = "Nowa wizyta do potwierdzenia",
            Content = serializedPayload,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
        });

        await _dbContext.SaveChangesAsync(ct);
    }

    private async Task<Appointment> GetAppointmentEntityAsync(int appointmentId, CancellationToken ct)
    {
        var appointment = await _dbContext.Appointments
            .Include(x => x.Doctor)
            .FirstOrDefaultAsync(x => x.AppointmentId == appointmentId, ct);

        if (appointment is null) throw new ArgumentException("Appointment not found.");
        return appointment;
    }

    private static void EnsureUserCanModifyAppointment(Appointment appointment, string userId)
    {
        if (appointment.UserId != userId && appointment.Doctor.UserId != userId)
            throw new UnauthorizedAccessException("You do not have permission to modify this appointment.");
    }

    private static void EnsureUserIsDoctorForAppointment(Appointment appointment, string userId)
    {
        if (appointment.Doctor.UserId != userId)
            throw new UnauthorizedAccessException("Only the assigned doctor can perform this action.");
    }

    private static bool IsCancelled(string status)
    {
        return string.Equals(status, AppointmentStatus.Cancelled, StringComparison.OrdinalIgnoreCase);
    }

    private static bool CanBeCancelled(string status)
    {
        return !string.Equals(status, AppointmentStatus.Confirmed, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(status, AppointmentStatus.Completed, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(status, AppointmentStatus.Unpaid, StringComparison.OrdinalIgnoreCase);
    }

    private static BookAppointmentResultDto MapToResult(Appointment app, Doctor.Doctor doc, AppointmentType.AppointmentType type, DateOnly date, TimeOnly startTime, DateTime requestedEnd)
    {
        var specialization = doc.DoctorSpecializations.Select(x => x.Specialization.Name).FirstOrDefault() ?? string.Empty;

        return new BookAppointmentResultDto(
            app.AppointmentId,
            doc.DoctorId,
            type.AppointmentTypeId,
            date,
            startTime.ToString("HH:mm"),
            TimeOnly.FromDateTime(requestedEnd).ToString("HH:mm"),
            app.Status,
            $"{doc.User.FirstName} {doc.User.LastName}",
            specialization
        );
    }

    private static AppointmentSummaryDto MapSummary(Appointment appointment)
    {
        var endTime = appointment.StartTime.AddMinutes(appointment.AppointmentTypeDurationMinutes);
        var latestPayment = appointment.Payments.OrderByDescending(p => p.CreatedAt).FirstOrDefault();

        return new AppointmentSummaryDto(
            appointment.AppointmentId,
            appointment.DoctorId,
            $"{appointment.Doctor.User.FirstName} {appointment.Doctor.User.LastName}",
            appointment.Doctor.DoctorSpecializations.Select(x => x.Specialization.Name).FirstOrDefault() ?? string.Empty,
            appointment.AppointmentType?.Name,
            appointment.AppointmentDate,
            appointment.StartTime.ToString("HH:mm"),
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
        var endTime = appointment.StartTime.AddMinutes(appointment.AppointmentTypeDurationMinutes);
        var latestPayment = appointment.Payments.OrderByDescending(p => p.CreatedAt).FirstOrDefault();

        return new AppointmentDetailDto(
            appointment.AppointmentId,
            appointment.DoctorId,
            $"{appointment.Doctor.User.FirstName} {appointment.Doctor.User.LastName}",
            appointment.Doctor.DoctorSpecializations.Select(x => x.Specialization.Name).FirstOrDefault() ?? string.Empty,
            appointment.AppointmentType?.Name,
            appointment.AppointmentDate,
            appointment.StartTime.ToString("HH:mm"),
            endTime.ToString("HH:mm"),
            appointment.Status,
            appointment.CreatedAt,
            latestPayment?.PaymentId,
            latestPayment?.Status,
            latestPayment?.Method,
            appointment.DoctorNotes
        );
    }
}