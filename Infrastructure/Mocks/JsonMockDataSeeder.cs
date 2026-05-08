using System.Text.Json;
using Medreserve.Features.AppointmentType;
using Medreserve.Features.Clinic;
using Medreserve.Features.Doctor;
using Medreserve.Features.Geography;
using Medreserve.Features.Specialization;
using Medreserve.Features.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Medreserve.Infrastructure.Mocks;

public sealed class JsonMockDataSeeder(
    DatabaseContext dbContext,
    ILogger<JsonMockDataSeeder> logger,
    UserManager<User> userManager
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
        var cities = await ReadListAsync<CityMock>(mocksDir, "cities.json", cancellationToken);
        var clinics = await ReadListAsync<ClinicMock>(mocksDir, "clinics.json", cancellationToken);
        var specializations = await ReadListAsync<SpecializationMock>(mocksDir, "specializations.json", cancellationToken);
        var appointmentTypes = await ReadListAsync<AppointmentTypeMock>(mocksDir, "appointment_types.json", cancellationToken);
        var doctors = await ReadListAsync<DoctorMock>(mocksDir, "doctors.json", cancellationToken);
        var clinicDoctors = await ReadListAsync<ClinicDoctorMock>(mocksDir, "clinic_doctors.json", cancellationToken);
        var doctorSpecializations = await ReadListAsync<DoctorSpecializationMock>(mocksDir, "doctor_specializations.json", cancellationToken);
        var doctorAppointmentTypes = await ReadListAsync<DoctorAppointmentTypeMock>(mocksDir, "doctor_appointment_types.json", cancellationToken);
        var doctorSchedules = await ReadListAsync<DoctorScheduleMock>(mocksDir, "doctor_schedules.json", cancellationToken);

        if (reset)
        {
            await ClearMockedTablesAsync(cancellationToken);
        }

        await UpsertUsersAsync(users, cancellationToken);

        dbContext.Cities.AddRange(cities.Select(x => new City
        {
            CityId = x.CityId,
            Name = x.Name,
            District = x.District,
            Voivodeship = x.Voivodeship,
        }));

        dbContext.Specializations.AddRange(specializations.Select(x => new Features.Specialization.Specialization
        {
            SpecializationId = x.SpecializationId,
            Name = x.Name,
            Description = x.Description,
        }));

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
            CityId = x.CityId,
        }));

        dbContext.Doctors.AddRange(doctors.Select(x => new Doctor
        {
            DoctorId = x.DoctorId,
            UserId = x.UserId,
            LicenseNumber = x.LicenseNumber,
            Bio = x.Bio,
        }));

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
            DayOfWeek = x.DayOfWeek,
            StartTime = x.StartTime,
            EndTime = x.EndTime,
            ValidFrom = x.ValidFrom,
            ValidTo = x.ValidTo,
            IsActive = x.IsActive,
        }));

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Mock seeding complete. cities={Cities}, clinics={Clinics}, specs={Specs}, doctors={Doctors}, users={Users}",
            cities.Count,
            clinics.Count,
            specializations.Count,
            doctors.Count,
            users.Count
        );
    }

    private async Task ClearMockedTablesAsync(CancellationToken cancellationToken)
    {
        await dbContext.DoctorSchedules.ExecuteDeleteAsync(cancellationToken);
        await dbContext.DoctorAppointmentTypes.ExecuteDeleteAsync(cancellationToken);
        await dbContext.DoctorSpecializations.ExecuteDeleteAsync(cancellationToken);
        await dbContext.ClinicDoctors.ExecuteDeleteAsync(cancellationToken);
        await dbContext.Doctors.ExecuteDeleteAsync(cancellationToken);
        await dbContext.Clinics.ExecuteDeleteAsync(cancellationToken);
        await dbContext.AppointmentTypes.ExecuteDeleteAsync(cancellationToken);
        await dbContext.Specializations.ExecuteDeleteAsync(cancellationToken);
        await dbContext.Cities.ExecuteDeleteAsync(cancellationToken);
    }

    private async Task UpsertUsersAsync(IEnumerable<UserMock> users, CancellationToken cancellationToken)
    {
        const string DefaultPassword = "medreserve";
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
                existing.UpdatedAt = DateTime.UtcNow;
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
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            dbContext.Users.Add(newUser);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        // Set passwords for all users
        var allUsers = await dbContext.Users.Where(x => userIds.Contains(x.Id)).ToListAsync(cancellationToken);
        foreach (var user in allUsers)
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            await userManager.ResetPasswordAsync(user, token, DefaultPassword);
        }
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
        string? PhoneNumber
    );

    private sealed record CityMock(int CityId, string Name, string District, string Voivodeship);

    private sealed record ClinicMock(
        int ClinicId,
        string Name,
        string StreetAddress,
        string? PhoneNumber,
        string? Email,
        bool IsActive,
        int CityId
    );

    private sealed record SpecializationMock(int SpecializationId, string Name, string? Description);

    private sealed record AppointmentTypeMock(
        int AppointmentTypeId,
        string Name,
        string? Description,
        decimal BasePrice,
        int DurationMinutes
    );

    private sealed record DoctorMock(int DoctorId, string UserId, string LicenseNumber, string? Bio);

    private sealed record ClinicDoctorMock(int ClinicId, int DoctorId, bool IsOwner);

    private sealed record DoctorSpecializationMock(int DoctorId, int SpecializationId);

    private sealed record DoctorAppointmentTypeMock(int DoctorId, int AppointmentTypeId);

    private sealed record DoctorScheduleMock(
        int ScheduleId,
        int DoctorId,
        int DayOfWeek,
        string StartTime,
        string EndTime,
        DateTime ValidFrom,
        DateTime? ValidTo,
        bool IsActive
    );
}
