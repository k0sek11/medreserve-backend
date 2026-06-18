using System.Text.Json;
using Medreserve.Features.Appointment;
using Medreserve.Features.AppointmentType;
using Medreserve.Features.Clinic;
using Medreserve.Features.Doctor;
using Medreserve.Features.Notification;
using Medreserve.Features.Payment;
using Medreserve.Features.Specialization;
using Medreserve.Features.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Medreserve.Infrastructure.Mocks;

public sealed class JsonMockDataSeeder(
    DatabaseContext dbContext,
    ILogger<JsonMockDataSeeder> logger,
    UserManager<User> userManager,
    IPasswordHasher<User> passwordHasher
)
    : IMockDataSeeder
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public async Task SeedAsync(bool reset, CancellationToken cancellationToken = default)
    {
        var mocksDir = ResolveMocksDirectory();
        logger.LogInformation("Mock seeding from {MocksDir} (reset: {Reset})", mocksDir, reset);

        var users = await ReadListAsync<UserMock>(mocksDir, "users.json", cancellationToken);
        var clinics = await ReadListAsync<ClinicMock>(mocksDir, "clinics.json", cancellationToken);
        var specializations = await ReadListAsync<SpecializationMock>(mocksDir, "specializations.json", cancellationToken);
        var appointmentTypes = await ReadListAsync<AppointmentTypeMock>(mocksDir, "appointment_types.json", cancellationToken);
        var doctors = await ReadListAsync<DoctorMock>(mocksDir, "doctors.json", cancellationToken);
        var clinicDoctors = await ReadListAsync<ClinicDoctorMock>(mocksDir, "clinic_doctors.json", cancellationToken);
        var doctorSpecializations = await ReadListAsync<DoctorSpecializationMock>(mocksDir, "doctor_specializations.json", cancellationToken);
        var doctorAppointmentTypes = await ReadListAsync<DoctorAppointmentTypeMock>(mocksDir, "doctor_appointment_types.json", cancellationToken);
        var doctorSchedules = await ReadListAsync<DoctorScheduleMock>(mocksDir, "doctor_schedules.json", cancellationToken);
        var appointments = await ReadListAsync<AppointmentMock>(mocksDir, "appointments.json", cancellationToken);
        var payments = await ReadListAsync<PaymentMock>(mocksDir, "payments.json", cancellationToken);
        var notifications = await ReadListAsync<NotificationMock>(mocksDir, "notifications.json", cancellationToken);
        var offlineApprovals = await ReadListAsync<OfflinePaymentApprovalMock>(mocksDir, "offline_payment_approvals.json", cancellationToken);

        if (reset)
        {
            await ClearMockedTablesAsync(cancellationToken);
        }

        await UpsertUsersAsync(users, cancellationToken);
        await AssignRolesAsync(users, cancellationToken);

        var existingSpecIds = await dbContext.Specializations
.Select(x => x.SpecializationId)
.ToListAsync(cancellationToken);
        var existingSpecIdSet = new HashSet<int>(existingSpecIds);
        var newSpecs = specializations
            .Where(x => !existingSpecIdSet.Contains(x.SpecializationId))
            .Select(x => new Specialization
            {
                SpecializationId = x.SpecializationId,
                Name = x.Name,
                Description = x.Description,
            })
            .ToList();
        if (newSpecs.Count > 0)
            dbContext.Specializations.AddRange(newSpecs);

        dbContext.AppointmentTypes.AddRange(appointmentTypes.Select(x => new AppointmentType
        {
            AppointmentTypeId = x.AppointmentTypeId,
            Name = x.Name,
            Description = x.Description,
            BasePrice = x.BasePrice,
            DurationMinutes = x.DurationMinutes,
        }));

        dbContext.Clinics.AddRange(clinics.Select(x => new Clinic
        {
            ClinicId = x.ClinicId,
            Name = x.Name,
            StreetAddress = x.StreetAddress,
            PhoneNumber = x.PhoneNumber,
            Email = x.Email,
            IsActive = x.IsActive,
            City = x.City,
            Description = x.Description,
            OpeningHours = x.OpeningHours,
            Latitude = x.Latitude,
            Longitude = x.Longitude,
        }));

        dbContext.Doctors.AddRange(doctors.Select(x => new Doctor
        {
            DoctorId = x.DoctorId,
            UserId = x.UserId,
            LicenseNumber = x.LicenseNumber,
            Bio = x.Bio,
            ProfileImageUrl = x.ProfileImageUrl,
        }));

        await dbContext.SaveChangesAsync(cancellationToken);

        dbContext.ClinicDoctors.AddRange(clinicDoctors.Select(x => new ClinicDoctor
        {
            ClinicId = x.ClinicId,
            DoctorId = x.DoctorId,
            IsOwner = x.IsOwner,
        }));

        dbContext.DoctorSpecializations.AddRange(doctorSpecializations.Select(x => new DoctorSpecialization
        {
            DoctorId = x.DoctorId,
            SpecializationId = x.SpecializationId,
        }));

        dbContext.DoctorAppointmentTypes.AddRange(doctorAppointmentTypes.Select(x => new DoctorAppointmentType
        {
            DoctorId = x.DoctorId,
            AppointmentTypeId = x.AppointmentTypeId,
        }));

        dbContext.DoctorSchedules.AddRange(doctorSchedules.Select(x => new DoctorSchedule
        {
            ScheduleId = x.ScheduleId,
            DoctorId = x.DoctorId,
            ClinicId = x.ClinicId,
            DayOfWeek = x.DayOfWeek,
            StartTime = x.StartTime,
            EndTime = x.EndTime,
            ValidFrom = x.ValidFrom,
            ValidTo = x.ValidTo,
            IsActive = x.IsActive,
        }));

        dbContext.Appointments.AddRange(appointments.Select(x => new Appointment
        {
            AppointmentId = x.AppointmentId,
            UserId = x.UserId,
            DoctorId = x.DoctorId,
            AppointmentDate = x.AppointmentDate,
            StartTime = x.StartTime,
            AppointmentTypeId = x.AppointmentTypeId,
            AppointmentTypeDurationMinutes = x.AppointmentTypeDurationMinutes,
            Status = x.Status,
            DoctorNotes = x.DoctorNotes,
            CancellationReason = x.CancellationReason,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt,
            ConfirmedAt = x.ConfirmedAt,
            CompletedAt = x.CompletedAt,
            CancelledAt = x.CancelledAt,
        }));

        dbContext.Payments.AddRange(payments.Select(x => new Payment
        {
            PaymentId = x.PaymentId,
            AppointmentId = x.AppointmentId,
            Amount = x.Amount,
            Currency = x.Currency,
            Method = x.Method,
            Status = x.Status,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt,
            PaidAt = x.PaidAt,
        }));

        dbContext.Notifications.AddRange(notifications.Select(x => new Notification
        {
            NotificationId = x.NotificationId,
            UserId = x.UserId,
            AppointmentId = x.AppointmentId,
            Type = x.Type,
            Subject = x.Subject,
            Content = x.Content,
            Status = x.Status,
            CreatedAt = x.CreatedAt,
            SentAt = x.SentAt,
            FailureReason = x.FailureReason,
        }));

        dbContext.OfflinePaymentApprovals.AddRange(offlineApprovals.Select(x => new OfflinePaymentApproval
        {
            ApprovalId = x.ApprovalId,
            PaymentId = x.PaymentId,
            ApprovedByUserId = x.ApprovedByUserId,
            Decision = x.Decision,
            DecisionDate = x.DecisionDate,
            Comment = x.Comment,
        }));

        await dbContext.SaveChangesAsync(cancellationToken);

        await SyncIdentitySequenceAsync("specializations", "specialization_id", cancellationToken);
        await SyncIdentitySequenceAsync("appointment_types", "appointment_type_id", cancellationToken);
        await SyncIdentitySequenceAsync("clinics", "clinic_id", cancellationToken);
        await SyncIdentitySequenceAsync("doctors", "doctor_id", cancellationToken);
        await SyncIdentitySequenceAsync("doctor_schedules", "schedule_id", cancellationToken);
        await SyncIdentitySequenceAsync("appointments", "appointment_id", cancellationToken);
        await SyncIdentitySequenceAsync("payments", "payment_id", cancellationToken);
        await SyncIdentitySequenceAsync("notifications", "notification_id", cancellationToken);
        await SyncIdentitySequenceAsync("offline_payment_approvals", "approval_id", cancellationToken);

        logger.LogInformation(
            "Mock seeding complete. users={Users}, clinics={Clinics}, specs={Specs}, doctors={Doctors}, "
            + "appointments={Appointments}, payments={Payments}, notifications={Notifications}",
            users.Count,
            clinics.Count,
            specializations.Count,
            doctors.Count,
            appointments.Count,
            payments.Count,
            notifications.Count
        );
    }

    private async Task ClearMockedTablesAsync(CancellationToken cancellationToken)
    {
        await dbContext.OfflinePaymentApprovals.ExecuteDeleteAsync(cancellationToken);
        await dbContext.Payments.ExecuteDeleteAsync(cancellationToken);
        await dbContext.Notifications.ExecuteDeleteAsync(cancellationToken);
        await dbContext.Appointments.ExecuteDeleteAsync(cancellationToken);
        await dbContext.DoctorSchedules.ExecuteDeleteAsync(cancellationToken);
        await dbContext.DoctorAppointmentTypes.ExecuteDeleteAsync(cancellationToken);
        await dbContext.DoctorSpecializations.ExecuteDeleteAsync(cancellationToken);
        await dbContext.ClinicDoctors.ExecuteDeleteAsync(cancellationToken);
        await dbContext.Doctors.ExecuteDeleteAsync(cancellationToken);
        await dbContext.Clinics.ExecuteDeleteAsync(cancellationToken);
        await dbContext.AppointmentTypes.ExecuteDeleteAsync(cancellationToken);
        await dbContext.Specializations.ExecuteDeleteAsync(cancellationToken);
    }

    private async Task UpsertUsersAsync(IEnumerable<UserMock> users, CancellationToken cancellationToken)
    {
        const string DefaultPassword = "Q1w2e3!";
        var userIds = users.Select(x => x.Id).Distinct().ToArray();
        if (userIds.Length == 0)
        {
            return;
        }

        var existingUsers = await dbContext
            .Users
            .Where(x => userIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        foreach (var user in users)
        {
            if (existingUsers.TryGetValue(user.Id, out var existing))
            {
                existing.Email = user.Email;
                existing.UserName = user.Email;
                existing.NormalizedEmail = user.Email.ToUpperInvariant();
                existing.NormalizedUserName = user.Email.ToUpperInvariant();
                existing.FirstName = user.FirstName;
                existing.LastName = user.LastName;
                existing.IsActive = user.IsActive;
                existing.PhoneNumber = user.PhoneNumber;
                existing.BirthDate = user.BirthDate;
                existing.Gender = user.Gender;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.PasswordHash = passwordHasher.HashPassword(existing, DefaultPassword);
                existing.SecurityStamp = Guid.NewGuid().ToString("N");
                continue;
            }

            var newUser = new User
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.Email,
                NormalizedEmail = user.Email.ToUpperInvariant(),
                NormalizedUserName = user.Email.ToUpperInvariant(),
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString("N"),
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive,
                PhoneNumber = user.PhoneNumber,
                BirthDate = user.BirthDate,
                Gender = user.Gender,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            newUser.PasswordHash = passwordHasher.HashPassword(newUser, DefaultPassword);
            dbContext.Users.Add(newUser);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task AssignRolesAsync(IEnumerable<UserMock> users, CancellationToken cancellationToken)
    {
        foreach (var user in users)
        {
            if (string.IsNullOrWhiteSpace(user.Role)) continue;

            var identityUser = await userManager.FindByIdAsync(user.Id);
            if (identityUser is null) continue;

            var currentRoles = await userManager.GetRolesAsync(identityUser);
            if (currentRoles.Count > 0) continue;

            await userManager.AddToRoleAsync(identityUser, user.Role);
        }
    }

    private async Task SyncIdentitySequenceAsync(string tableName, string columnName, CancellationToken cancellationToken)
    {
        var sql = $"SELECT setval(pg_get_serial_sequence('{tableName}', '{columnName}'), COALESCE((SELECT MAX({columnName}) FROM {tableName}), 0) + 1, false);";
        await dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    private static string ResolveMocksDirectory()
    {
        var candidates = new[]
        {
            Path.Combine(Directory.GetCurrentDirectory(), "mocks"),
            Path.Combine(AppContext.BaseDirectory, "mocks"),
        };

        foreach (var candidate in candidates)
        {
            if (Directory.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new DirectoryNotFoundException("Could not find mocks directory. Expected ./mocks next to api project.");
    }

    private static async Task<List<T>> ReadListAsync<T>(
        string mocksDir,
        string fileName,
        CancellationToken cancellationToken
    )
    {
        var filePath = Path.Combine(mocksDir, fileName);
        if (!File.Exists(filePath))
        {
            return [];
        }

        var json = await File.ReadAllTextAsync(filePath, cancellationToken);
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        var data = JsonSerializer.Deserialize<List<T>>(json, JsonOptions);
        return data ?? [];
    }

    private sealed record UserMock(
        string Id,
        string Email,
        string FirstName,
        string LastName,
        bool IsActive,
        string? PhoneNumber,
        DateOnly? BirthDate,
        string? Gender,
        string? Role
    );

    private sealed record ClinicMock(
        int ClinicId,
        string Name,
        string StreetAddress,
        string? PhoneNumber,
        string? Email,
        bool IsActive,
        string City,
        string? Description,
        string? OpeningHours,
        double? Latitude,
        double? Longitude
    );

    private sealed record SpecializationMock(int SpecializationId, string Name, string? Description);

    private sealed record AppointmentTypeMock(
        int AppointmentTypeId,
        string Name,
        string? Description,
        decimal BasePrice,
        int DurationMinutes
    );

    private sealed record DoctorMock(int DoctorId, string UserId, string LicenseNumber, string? Bio, string? ProfileImageUrl);

    private sealed record ClinicDoctorMock(int ClinicId, int DoctorId, bool IsOwner);

    private sealed record DoctorSpecializationMock(int DoctorId, int SpecializationId);

    private sealed record DoctorAppointmentTypeMock(int DoctorId, int AppointmentTypeId);

    private sealed record DoctorScheduleMock(
        int ScheduleId,
        int DoctorId,
        int? ClinicId,
        int DayOfWeek,
        string StartTime,
        string EndTime,
        DateTime ValidFrom,
        DateTime? ValidTo,
        bool IsActive
    );

    private sealed record AppointmentMock(
        int AppointmentId,
        string UserId,
        int DoctorId,
        DateOnly AppointmentDate,
        TimeOnly StartTime,
        int? AppointmentTypeId,
        int AppointmentTypeDurationMinutes,
        string Status,
        string? DoctorNotes,
        string? CancellationReason,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        DateTime? ConfirmedAt,
        DateTime? CompletedAt,
        DateTime? CancelledAt
    );

    private sealed record PaymentMock(
        int PaymentId,
        int AppointmentId,
        decimal Amount,
        string Currency,
        string Method,
        string Status,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        DateTime? PaidAt
    );

    private sealed record NotificationMock(
        int NotificationId,
        string UserId,
        int? AppointmentId,
        string Type,
        string Subject,
        string Content,
        string Status,
        DateTime CreatedAt,
        DateTime? SentAt,
        string? FailureReason
    );

    private sealed record OfflinePaymentApprovalMock(
        int ApprovalId,
        int PaymentId,
        string ApprovedByUserId,
        string Decision,
        DateTime DecisionDate,
        string? Comment
    );
}
