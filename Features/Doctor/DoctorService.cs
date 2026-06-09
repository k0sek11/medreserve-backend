using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Medreserve.Features.Appointment;
using Medreserve.Features.AppointmentType;
using Medreserve.Features.Clinic;
using Medreserve.Features.Users;
using Medreserve.Infrastructure;
using AppointmentTypeEntity = Medreserve.Features.AppointmentType.AppointmentType;

namespace Medreserve.Features.Doctor;

public class DoctorService : IDoctorService
{
    private readonly DatabaseContext _dbContext;
    private readonly UserManager<User> _userManager;

    public DoctorService(DatabaseContext dbContext, UserManager<User> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<bool> CreateProfileAsync(string userId, CreateDoctorProfileDto request)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        var profileExists = await _dbContext.Set<Doctor>().AnyAsync(d => d.UserId == userId);
        if (profileExists) return false;

        var doctor = new Doctor
        {
            UserId = userId,
            LicenseNumber = request.LicenseNumber,
            Bio = request.Bio
        };

        if (request.SpecializationIds != null && request.SpecializationIds.Any())
        {
            var specializationIds = request.SpecializationIds.Distinct().ToArray();
            var validSpecializationIds = await _dbContext.Specializations
                .Where(x => specializationIds.Contains(x.SpecializationId))
                .Select(x => x.SpecializationId)
                .ToListAsync();

            if (validSpecializationIds.Count != specializationIds.Length)
            {
                throw new ArgumentException("Jedna lub więcej specjalizacji nie istnieje.");
            }

            foreach (var specId in specializationIds)
            {
                doctor.DoctorSpecializations.Add(new DoctorSpecialization
                {
                    SpecializationId = specId
                });
            }
        }

        _dbContext.Set<Doctor>().Add(doctor);
        await _dbContext.SaveChangesAsync();

        user.IsActive = true;
        await _userManager.UpdateAsync(user);

        await _userManager.AddToRoleAsync(user, "Doctor");

        return true;
    }

    public async Task<DoctorProfileDto?> GetMyProfileAsync(string userId, CancellationToken cancellationToken)
    {
        var doctor = await LoadDoctorDetailsAsync(userId: userId, doctorId: null, cancellationToken);
        return doctor is null ? null : MapProfile(doctor);
    }

    public async Task<bool> UpdateMyProfileAsync(string userId, UpdateDoctorProfileDto request, CancellationToken cancellationToken)
    {
        var doctor = await _dbContext.Doctors
            .Include(x => x.DoctorSpecializations)
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        if (doctor is null)
        {
            return false;
        }

        doctor.Bio = request.Bio;

        if (request.SpecializationIds is not null)
        {
            var specializationIds = request.SpecializationIds.Distinct().ToArray();
            var validSpecializationIds = await _dbContext.Specializations
                .Where(x => specializationIds.Contains(x.SpecializationId))
                .Select(x => x.SpecializationId)
                .ToListAsync(cancellationToken);

            if (validSpecializationIds.Count != specializationIds.Length)
            {
                throw new ArgumentException("Jedna lub więcej specjalizacji nie istnieje.");
            }

            doctor.DoctorSpecializations.Clear();

            foreach (var specializationId in specializationIds)
            {
                doctor.DoctorSpecializations.Add(new DoctorSpecialization
                {
                    DoctorId = doctor.DoctorId,
                    SpecializationId = specializationId
                });
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<DoctorAppointmentTypeDto?> CreateMyAppointmentTypeAsync(
        string userId,
        CreateDoctorAppointmentTypeDto request,
        CancellationToken cancellationToken
    )
    {
        var doctor = await _dbContext.Doctors
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        if (doctor is null) return null;

        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Appointment type name is required.");

        if (request.BasePrice < 0)
            throw new ArgumentException("Appointment type price must be zero or greater.");

        if (request.DurationMinutes <= 0)
            throw new ArgumentException("Appointment type duration must be greater than zero.");

        var duplicateExists = await _dbContext.DoctorAppointmentTypes
            .AsNoTracking()
            .AnyAsync(x => x.DoctorId == doctor.DoctorId && x.AppointmentType.Name == name, cancellationToken);

        if (duplicateExists)
            throw new InvalidOperationException("Masz już typ wizyty o takiej nazwie.");

        var appointmentType = new AppointmentTypeEntity
        {
            Name = name,
            Description = null,
            BasePrice = request.BasePrice,
            DurationMinutes = request.DurationMinutes,
        };

        _dbContext.AppointmentTypes.Add(appointmentType);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _dbContext.DoctorAppointmentTypes.Add(new DoctorAppointmentType
        {
            DoctorId = doctor.DoctorId,
            AppointmentTypeId = appointmentType.AppointmentTypeId,
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapAppointmentType(appointmentType);
    }

    public async Task<bool> DeleteMyAppointmentTypeAsync(string userId, int appointmentTypeId, CancellationToken cancellationToken)
    {
        var doctor = await _dbContext.Doctors
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        if (doctor is null) return false;

        var doctorAppointmentType = await _dbContext.DoctorAppointmentTypes
            .Include(x => x.AppointmentType)
            .FirstOrDefaultAsync(x => x.DoctorId == doctor.DoctorId && x.AppointmentTypeId == appointmentTypeId, cancellationToken);

        if (doctorAppointmentType is null || doctorAppointmentType.AppointmentType is null)
            return false;

        var affectedAppointments = await _dbContext.Appointments
            .Where(x => x.AppointmentTypeId == appointmentTypeId)
            .ToListAsync(cancellationToken);

        foreach (var appointment in affectedAppointments)
            appointment.AppointmentTypeId = null;

        _dbContext.DoctorAppointmentTypes.Remove(doctorAppointmentType);
        _dbContext.AppointmentTypes.Remove(doctorAppointmentType.AppointmentType);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<DoctorScheduleDto>?> GetMySchedulesAsync(string userId, CancellationToken cancellationToken)
    {
        var doctor = await LoadDoctorDetailsAsync(userId: userId, doctorId: null, cancellationToken);
        return doctor is null ? null : doctor.DoctorSchedules.Select(MapSchedule).ToList();
    }

    public async Task<DoctorScheduleDto?> UpsertMyScheduleAsync(string userId, UpsertDoctorScheduleDto request, CancellationToken cancellationToken)
    {
        var doctor = await _dbContext.Doctors.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        if (doctor is null) return null;

        var clinic = await _dbContext.Clinics
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ClinicId == request.ClinicId, cancellationToken);
        if (clinic is null)
            throw new InvalidOperationException("Przychodnia nie istnieje.");

        var membershipExists = await _dbContext.ClinicDoctors.AnyAsync(
            x => x.ClinicId == request.ClinicId && x.DoctorId == doctor.DoctorId, cancellationToken);

        if (!membershipExists)
            throw new InvalidOperationException("Możesz tworzyć grafik tylko dla przychodni, do których należysz.");

        var normalizedDayOfWeek = AppointmentSchedulingHelper.NormalizeDayOfWeek(request.DayOfWeek);
        var startTime = AppointmentSchedulingHelper.ParseTime(request.StartTime);
        var endTime = AppointmentSchedulingHelper.ParseTime(request.EndTime);

        if (endTime <= startTime)
            throw new ArgumentException("Schedule end time must be after start time.");

        if (request.ValidTo.HasValue && request.ValidTo.Value < request.ValidFrom)
            throw new ArgumentException("Schedule validity end date must be on or after the start date.");

        DoctorSchedule schedule;
        if (request.ScheduleId.HasValue)
        {
            schedule = await _dbContext.DoctorSchedules.FirstOrDefaultAsync(
                x => x.ScheduleId == request.ScheduleId.Value && x.DoctorId == doctor.DoctorId, cancellationToken)
                ?? throw new InvalidOperationException("Schedule not found.");
        }
        else
        {
            schedule = new DoctorSchedule { DoctorId = doctor.DoctorId };
            _dbContext.DoctorSchedules.Add(schedule);
        }

        schedule.DayOfWeek = normalizedDayOfWeek;
        schedule.StartTime = startTime.ToString("HH:mm");
        schedule.EndTime = endTime.ToString("HH:mm");
        schedule.ClinicId = request.ClinicId;
        schedule.ValidFrom = DateTime.SpecifyKind(request.ValidFrom.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        schedule.ValidTo = request.ValidTo is null
            ? null
            : DateTime.SpecifyKind(request.ValidTo.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        schedule.IsActive = true;

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _dbContext.Entry(schedule).Reference(x => x.Clinic).LoadAsync(cancellationToken);
        return MapSchedule(schedule);
    }

    public async Task<bool> DeleteMyScheduleAsync(string userId, int scheduleId, CancellationToken cancellationToken)
    {
        var doctor = await _dbContext.Doctors.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        if (doctor is null) return false;

        var schedule = await _dbContext.DoctorSchedules.FirstOrDefaultAsync(
            x => x.ScheduleId == scheduleId && x.DoctorId == doctor.DoctorId, cancellationToken);

        if (schedule is null) return false;

        _dbContext.DoctorSchedules.Remove(schedule);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<DoctorPublicProfileDto?> GetPublicProfileAsync(int doctorId, CancellationToken cancellationToken)
    {
        var doctor = await LoadDoctorDetailsAsync(userId: null, doctorId: doctorId, cancellationToken);
        return doctor is null ? null : MapPublicProfile(doctor);
    }

    public async Task<DoctorAvailabilityDto?> GetAvailabilityAsync(
        int doctorId, DateOnly date, int appointmentTypeId, int? clinicId, CancellationToken cancellationToken)
    {
        var doctor = await LoadAvailabilityDoctorAsync(doctorId, clinicId, cancellationToken);
        if (doctor is null) return null;

        var appointmentType = doctor.DoctorAppointmentTypes
            .Select(x => x.AppointmentType)
            .FirstOrDefault(x => x.AppointmentTypeId == appointmentTypeId);
        if (appointmentType is null) return null;

        var bookedIntervals = doctor.Appointments
            .Where(x => !IsCancelled(x.Status) && x.AppointmentDate == date)
            .Select(x => (Start: x.GetStartDateTime(), End: x.GetEndDateTime()))
            .ToList();

        var orderedSlots = BuildAvailableSlots(date, appointmentType, clinicId, doctor.DoctorSchedules, bookedIntervals);

        return new DoctorAvailabilityDto(
            doctorId, date, appointmentTypeId, clinicId,
            appointmentType.Name, appointmentType.DurationMinutes, orderedSlots);
    }

    public async Task<DoctorAvailabilityCalendarDto?> GetAvailabilityCalendarAsync(
        int doctorId, int year, int month, int appointmentTypeId, int? clinicId, CancellationToken cancellationToken)
    {
        var doctor = await LoadAvailabilityDoctorAsync(doctorId, clinicId, cancellationToken);
        if (doctor is null) return null;

        var appointmentType = doctor.DoctorAppointmentTypes
            .Select(x => x.AppointmentType)
            .FirstOrDefault(x => x.AppointmentTypeId == appointmentTypeId);
        if (appointmentType is null) return null;

        var availableDates = new List<DateOnly>();
        var daysInMonth = DateTime.DaysInMonth(year, month);
        var bookedIntervals = doctor.Appointments
            .Where(x => !IsCancelled(x.Status))
            .Select(x => (x.AppointmentDate, Start: x.GetStartDateTime(), End: x.GetEndDateTime()))
            .ToList();

        for (var day = 1; day <= daysInMonth; day++)
        {
            var date = new DateOnly(year, month, day);
            var dayBooked = bookedIntervals
                .Where(x => x.AppointmentDate == date)
                .Select(x => (x.Start, x.End))
                .ToList();

            var slots = BuildAvailableSlots(date, appointmentType, clinicId, doctor.DoctorSchedules, dayBooked);
            if (slots.Count > 0) availableDates.Add(date);
        }

        return new DoctorAvailabilityCalendarDto(doctorId, year, month, appointmentTypeId, clinicId, availableDates);
    }

    // ──────────────────────────── Private Helpers ────────────────────────────

    private async Task<Doctor?> LoadAvailabilityDoctorAsync(int doctorId, int? clinicId, CancellationToken ct)
    {
        var query = _dbContext.Doctors.AsNoTracking()
            .Include(x => x.DoctorSchedules)
            .Include(x => x.DoctorAppointmentTypes).ThenInclude(x => x.AppointmentType)
            .Include(x => x.Appointments).ThenInclude(x => x.AppointmentType)
            .Include(x => x.ClinicDoctors).ThenInclude(x => x.Clinic)
            .AsQueryable();

        if (clinicId.HasValue)
            query = query.Where(x => x.ClinicDoctors.Any(cd => cd.ClinicId == clinicId.Value));

        return await query.FirstOrDefaultAsync(x => x.DoctorId == doctorId, ct);
    }

    private static IReadOnlyList<DoctorAvailabilitySlotDto> BuildAvailableSlots(
        DateOnly date,
        AppointmentTypeEntity appointmentType,
        int? clinicId,
        IEnumerable<DoctorSchedule> schedules,
        IReadOnlyList<(DateTime Start, DateTime End)> bookedIntervals)
    {
        var targetDayOfWeek = AppointmentSchedulingHelper.NormalizeDayOfWeek((int)date.DayOfWeek);
        var slots = new List<DoctorAvailabilitySlotDto>();

        foreach (var schedule in schedules)
        {
            if (!schedule.IsActive) continue;
            if (clinicId.HasValue && schedule.ClinicId != clinicId.Value) continue;

            var scheduleDayOfWeek = AppointmentSchedulingHelper.NormalizeDayOfWeek(schedule.DayOfWeek);
            if (scheduleDayOfWeek != targetDayOfWeek) continue;

            var validFrom = DateOnly.FromDateTime(schedule.ValidFrom);
            var validTo = schedule.ValidTo.HasValue ? DateOnly.FromDateTime(schedule.ValidTo.Value) : (DateOnly?)null;
            if (validFrom > date || (validTo.HasValue && validTo.Value < date)) continue;

            var scheduleStart = AppointmentSchedulingHelper.ParseTime(schedule.StartTime);
            var scheduleEnd = AppointmentSchedulingHelper.ParseTime(schedule.EndTime);
            var currentStart = scheduleStart;

            while (currentStart.AddMinutes(appointmentType.DurationMinutes) <= scheduleEnd)
            {
                var currentEnd = currentStart.AddMinutes(appointmentType.DurationMinutes);
                var startDateTime = AppointmentSchedulingHelper.ToDateTime(date, currentStart);
                var endDateTime = AppointmentSchedulingHelper.ToDateTime(date, currentEnd);

                var overlapsBookedSlot = bookedIntervals.Any(booked =>
                    AppointmentSchedulingHelper.IsOverlapping(startDateTime, endDateTime, booked.Start, booked.End));

                slots.Add(new DoctorAvailabilitySlotDto(
                    startDateTime.ToString("s"),
                    endDateTime.ToString("s"),
                    overlapsBookedSlot));

                currentStart = currentStart.AddMinutes(AppointmentSchedulingHelper.SlotStepMinutes);
            }
        }

        return slots.OrderBy(x => x.StartAt, StringComparer.Ordinal).ToList();
    }

    private async Task<Doctor?> LoadDoctorDetailsAsync(string? userId, int? doctorId, CancellationToken cancellationToken)
    {
        var query = _dbContext.Doctors
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.DoctorSchedules).ThenInclude(x => x.Clinic)
            .Include(x => x.DoctorSpecializations).ThenInclude(x => x.Specialization)
            .Include(x => x.DoctorAppointmentTypes).ThenInclude(x => x.AppointmentType)
            .Include(x => x.ClinicDoctors).ThenInclude(x => x.Clinic).ThenInclude(x => x.City)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(userId))
            query = query.Where(x => x.UserId == userId);

        if (doctorId.HasValue)
            query = query.Where(x => x.DoctorId == doctorId.Value);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    private static DoctorProfileDto MapProfile(Doctor doctor)
    {
        var clinic = doctor.ClinicDoctors
            .OrderByDescending(x => x.IsOwner)
            .ThenBy(x => x.ClinicId)
            .FirstOrDefault();

        return new DoctorProfileDto(
            doctor.DoctorId,
            $"{doctor.User.FirstName} {doctor.User.LastName}",
            doctor.LicenseNumber,
            doctor.Bio,
            doctor.User.PhoneNumber,
            clinic?.Clinic.City.Name,
            clinic?.Clinic.StreetAddress,
            null,
            doctor.DoctorSpecializations.Select(x => x.Specialization.Name).Distinct().OrderBy(x => x).ToList(),
            doctor.DoctorAppointmentTypes.Select(x => MapAppointmentType(x.AppointmentType)).OrderBy(x => x.BasePrice).ThenBy(x => x.Name).ToList(),
            doctor.DoctorSchedules.OrderBy(x => x.DayOfWeek).ThenBy(x => x.StartTime).Select(MapSchedule).ToList(),
            doctor.ClinicDoctors.OrderByDescending(x => x.IsOwner).ThenBy(x => x.ClinicId).Select(MapClinic).ToList());
    }

    private static DoctorPublicProfileDto MapPublicProfile(Doctor doctor)
    {
        var clinic = doctor.ClinicDoctors
            .OrderByDescending(x => x.IsOwner)
            .ThenBy(x => x.ClinicId)
            .FirstOrDefault();

        return new DoctorPublicProfileDto(
            doctor.DoctorId,
            $"{doctor.User.FirstName} {doctor.User.LastName}",
            doctor.LicenseNumber,
            doctor.Bio,
            doctor.User.PhoneNumber,
            clinic?.Clinic.City.Name,
            clinic?.Clinic.StreetAddress,
            null,
            doctor.DoctorSpecializations.Select(x => x.Specialization.Name).Distinct().OrderBy(x => x).ToList(),
            doctor.DoctorAppointmentTypes.Select(x => MapAppointmentType(x.AppointmentType)).OrderBy(x => x.BasePrice).ThenBy(x => x.Name).ToList(),
            doctor.ClinicDoctors.OrderByDescending(x => x.IsOwner).ThenBy(x => x.ClinicId).Select(MapClinic).ToList());
    }

    private static DoctorClinicDto MapClinic(ClinicDoctor clinicDoctor) =>
        new(clinicDoctor.ClinicId, clinicDoctor.Clinic.Name, clinicDoctor.Clinic.City.Name, clinicDoctor.Clinic.StreetAddress);

    private static DoctorScheduleDto MapSchedule(DoctorSchedule schedule) =>
        new(schedule.ScheduleId, schedule.ClinicId, schedule.Clinic?.Name,
            AppointmentSchedulingHelper.NormalizeDayOfWeek(schedule.DayOfWeek),
            schedule.StartTime, schedule.EndTime,
            DateOnly.FromDateTime(schedule.ValidFrom),
            schedule.ValidTo.HasValue ? DateOnly.FromDateTime(schedule.ValidTo.Value) : null,
            schedule.IsActive);

    private static DoctorAppointmentTypeDto MapAppointmentType(AppointmentTypeEntity appointmentType) =>
        new(appointmentType.AppointmentTypeId, appointmentType.Name, appointmentType.Description,
            appointmentType.BasePrice, appointmentType.DurationMinutes);

    private static bool IsCancelled(string status) =>
        string.Equals(status, AppointmentStatus.Cancelled, StringComparison.OrdinalIgnoreCase);
}