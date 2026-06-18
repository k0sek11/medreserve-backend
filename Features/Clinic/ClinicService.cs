using System.Text.Json;
using Medreserve.Features.Doctor;
using Medreserve.Features.Notification;
using Medreserve.Infrastructure;
using Microsoft.EntityFrameworkCore;
using NotificationEntity = Medreserve.Features.Notification.Notification;

namespace Medreserve.Features.Clinic;

public class ClinicService(DatabaseContext _context) : IClinicService
{
    public async Task<object> GetAsync(Dto.ClinicListQuery query, CancellationToken ct)
    {
        return query.View?.Trim().ToLowerInvariant() switch
        {
            "specializations" => await ListSpecializationsAsync(query.Location, ct),
            _ => await ListClinicsAsync(query, ct),
        };
    }

    public async Task<Dto.ClinicDetailDto?> GetByIdAsync(int id, string? currentUserId, CancellationToken ct)
    {
        var currentDoctorId = currentUserId is null
            ? null
            : await ResolveDoctorIdAsync(currentUserId, ct);

        var clinic = await _context.Clinics
            .AsNoTracking()
            .Where(x => x.ClinicId == id)
            .Select(x => new
            {
                x.ClinicId,
                x.Name,
                x.Description,
                x.StreetAddress,
                x.OpeningHours,
                x.Latitude,
                x.Longitude,
                x.PhoneNumber,
                x.Email,
                x.City,
                x.IsActive,
            })
            .FirstOrDefaultAsync(ct);

        if (clinic is null) return null;

        var doctors = await _context.ClinicDoctors
            .AsNoTracking()
            .Where(x => x.ClinicId == id)
            .Select(x => new { x.DoctorId, FullName = x.Doctor.User.FirstName + " " + x.Doctor.User.LastName, PrimarySpecialization = x.Doctor.DoctorSpecializations.OrderBy(ds => ds.Specialization.Name).Select(ds => ds.Specialization.Name).FirstOrDefault() ?? "Lekarz", x.IsOwner })
            .OrderByDescending(x => x.IsOwner).ThenBy(x => x.FullName)
            .Select(x => new Dto.ClinicDoctorSummaryDto(x.DoctorId, x.FullName, x.PrimarySpecialization, x.IsOwner))
            .ToListAsync(ct);

        var specializations = await _context.ClinicDoctors
            .AsNoTracking()
            .Where(x => x.ClinicId == id)
            .SelectMany(x => x.Doctor.DoctorSpecializations.Select(ds => ds.Specialization.Name))
            .Distinct().OrderBy(x => x).ToListAsync(ct);

        var isMember = currentDoctorId.HasValue && await _context.ClinicDoctors
            .AnyAsync(x => x.ClinicId == id && x.DoctorId == currentDoctorId.Value, ct);

        var isOwner = currentDoctorId.HasValue && await _context.ClinicDoctors
            .AnyAsync(x => x.ClinicId == id && x.DoctorId == currentDoctorId.Value && x.IsOwner, ct);

        return new Dto.ClinicDetailDto(
            clinic.ClinicId, clinic.Name, clinic.Description, clinic.StreetAddress,
            clinic.OpeningHours, clinic.Latitude, clinic.Longitude,
            clinic.PhoneNumber, clinic.Email, clinic.City,
            clinic.IsActive, doctors.Count, specializations, doctors, isMember, isOwner);
    }

    public async Task<Dto.ClinicDto> CreateAsync(Dto.CreateClinicRequest request, string currentUserId, CancellationToken ct)
    {
        var doctor = await _context.Doctors.FirstOrDefaultAsync(x => x.UserId == currentUserId, ct)
            ?? throw new InvalidOperationException("Tylko lekarz może zarejestrować przychodnię.");

        var clinic = new Clinic
        {
            Name = request.Name.Trim(),
            Description = TrimToNull(request.Description),
            StreetAddress = request.StreetAddress.Trim(),
            OpeningHours = TrimToNull(request.OpeningHours),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            City = request.City.Trim(),
            PhoneNumber = TrimToNull(request.PhoneNumber),
            Email = TrimToNull(request.Email),
            IsActive = true,
        };
        clinic.ClinicDoctors.Add(new ClinicDoctor { DoctorId = doctor.DoctorId, IsOwner = true });

        _context.Clinics.Add(clinic);
        await _context.SaveChangesAsync(ct);

        return new Dto.ClinicDto(clinic.ClinicId, clinic.Name, clinic.Description, clinic.StreetAddress,
            clinic.OpeningHours, clinic.Latitude, clinic.Longitude,
            clinic.PhoneNumber, clinic.Email, clinic.IsActive);
    }

    public async Task<Dto.ClinicDto?> UpdateAsync(int id, Dto.UpdateClinicRequest request, CancellationToken ct)
    {
        var clinic = await _context.Clinics.FirstOrDefaultAsync(x => x.ClinicId == id, ct);
        if (clinic is null) return null;

        if (request.Name is not null) clinic.Name = request.Name;
        if (request.StreetAddress is not null) clinic.StreetAddress = request.StreetAddress;
        if (request.Description is not null) clinic.Description = TrimToNull(request.Description);
        if (request.OpeningHours is not null) clinic.OpeningHours = TrimToNull(request.OpeningHours);
        if (request.Latitude is not null) clinic.Latitude = request.Latitude;
        if (request.Longitude is not null) clinic.Longitude = request.Longitude;
        if (request.City is not null) clinic.City = request.City.Trim();
        if (request.PhoneNumber is not null) clinic.PhoneNumber = TrimToNull(request.PhoneNumber);
        if (request.Email is not null) clinic.Email = TrimToNull(request.Email);
        if (request.IsActive.HasValue) clinic.IsActive = request.IsActive.Value;

        await _context.SaveChangesAsync(ct);

        return new Dto.ClinicDto(clinic.ClinicId, clinic.Name, clinic.Description, clinic.StreetAddress,
            clinic.OpeningHours, clinic.Latitude, clinic.Longitude,
            clinic.PhoneNumber, clinic.Email, clinic.IsActive);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var clinic = await _context.Clinics.FirstOrDefaultAsync(x => x.ClinicId == id, ct);
        if (clinic is null) return false;
        _context.Clinics.Remove(clinic);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IReadOnlyList<Dto.ClinicListItemDto>> GetMineAsync(string currentUserId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(currentUserId)) return [];

        var doctorId = await ResolveDoctorIdAsync(currentUserId, ct);
        if (doctorId is null) return [];

        var clinicRows = await _context.ClinicDoctors
            .AsNoTracking()
            .Where(cd => cd.DoctorId == doctorId.Value)
            .Select(cd => new { cd.ClinicId, cd.Clinic.Name, cd.Clinic.StreetAddress, cd.Clinic.City, cd.Clinic.IsActive, cd.IsOwner, DoctorCount = cd.Clinic.ClinicDoctors.Select(x => x.DoctorId).Distinct().Count() })
            .OrderByDescending(x => x.IsOwner).ThenBy(x => x.Name)
            .ToListAsync(ct);

        var specializationLookup = await BuildSpecializationLookupAsync(clinicRows.Select(x => x.ClinicId).ToArray(), ct);

        return clinicRows.Select(x => new Dto.ClinicListItemDto(
            x.ClinicId, x.Name, x.StreetAddress, x.City, x.DoctorCount,
            specializationLookup.GetValueOrDefault(x.ClinicId, []), x.IsActive, x.IsOwner)).ToList();
    }

    public async Task<string?> RequestJoinAsync(int clinicId, Dto.CreateClinicJoinRequestDto request, string currentUserId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(currentUserId)) return "Brak autoryzacji.";

        var currentDoctor = await _context.Doctors.AsNoTracking()
            .Where(x => x.UserId == currentUserId)
            .Select(x => new { x.DoctorId, x.UserId })
            .FirstOrDefaultAsync(ct);
        if (currentDoctor is null) return "Tylko lekarz może wysłać prośbę o dołączenie.";

        var clinic = await _context.Clinics.AsNoTracking().FirstOrDefaultAsync(x => x.ClinicId == clinicId, ct);
        if (clinic is null) return "Nie znaleziono placówki.";

        var alreadyMember = await _context.ClinicDoctors.AnyAsync(x => x.ClinicId == clinicId && x.DoctorId == currentDoctor.DoctorId, ct);
        if (alreadyMember) return "Jesteś już przypisany do tej przychodni.";

        var pendingRequests = await _context.Notifications.AsNoTracking()
            .Where(x => x.Type == NotificationKinds.ClinicJoinRequest && x.Status == "Pending").ToListAsync(ct);

        if (pendingRequests.Any(n => IsDuplicateJoinRequest(n.Content, clinicId, currentDoctor.DoctorId)))
            return "Masz już aktywną prośbę o dołączenie do tej przychodni.";

        var owners = await _context.ClinicDoctors.AsNoTracking()
            .Where(x => x.ClinicId == clinicId && x.IsOwner)
            .Select(x => new { x.Doctor.UserId }).ToListAsync(ct);
        if (owners.Count == 0) return "Ta przychodnia nie ma jeszcze właściciela.";

        var requester = await _context.Users.AsNoTracking()
            .Where(x => x.Id == currentUserId)
            .Select(x => new { x.FirstName, x.LastName }).FirstAsync(ct);

        var payload = new ClinicJoinRequestPayload(
            Guid.NewGuid().ToString("N"), clinic.ClinicId, clinic.Name,
            currentDoctor.DoctorId, currentUserId,
            $"{requester.FirstName} {requester.LastName}",
            string.IsNullOrWhiteSpace(request.Message) ? null : request.Message.Trim());

        var notifications = owners.Select(owner => new NotificationEntity
        {
            UserId = owner.UserId,
            Type = NotificationKinds.ClinicJoinRequest,
            Subject = $"Prośba o dołączenie do {clinic.Name}",
            Content = JsonSerializer.Serialize(payload),
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
        });

        _context.Notifications.AddRange(notifications);
        await _context.SaveChangesAsync(ct);
        return null;
    }

    private async Task<IReadOnlyList<Dto.ClinicSpecializationDto>> ListSpecializationsAsync(string? location, CancellationToken ct)
    {
        IQueryable<Specialization.Specialization> query = _context.Specializations.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(location))
        {
            var loc = location.Trim();
            query = query.Where(s => s.DoctorSpecializations.Any(ds =>
                ds.Doctor.ClinicDoctors.Any(cd =>
                    EF.Functions.ILike(cd.Clinic.City, $"%{loc}%") ||
                    EF.Functions.ILike(cd.Clinic.StreetAddress, $"%{loc}%"))));
        }

        return await query
            .OrderBy(s => s.Name)
            .Select(s => new Dto.ClinicSpecializationDto(s.SpecializationId, s.Name, s.Description))
            .ToListAsync(ct);
    }

    private async Task<Dto.PagedResultDto<Dto.ClinicListItemDto>> ListClinicsAsync(Dto.ClinicListQuery query, CancellationToken ct)
    {
        IQueryable<Clinic> clinicsQuery = _context.Clinics.AsNoTracking().Where(x => x.IsActive);

        if (!string.IsNullOrWhiteSpace(query.Name))
        {
            var name = query.Name.Trim();
            clinicsQuery = clinicsQuery.Where(x =>
                EF.Functions.ILike(x.Name, $"%{name}%") ||
                EF.Functions.ILike(x.StreetAddress, $"%{name}%") ||
                EF.Functions.ILike(x.City, $"%{name}%"));
        }

        if (!string.IsNullOrWhiteSpace(query.Location))
        {
            var location = query.Location.Trim();
            clinicsQuery = clinicsQuery.Where(x =>
                EF.Functions.ILike(x.StreetAddress, $"%{location}%") ||
                EF.Functions.ILike(x.City, $"%{location}%"));
        }

        if (query.SpecializationId.HasValue)
            clinicsQuery = clinicsQuery.Where(x =>
                x.ClinicDoctors.Any(cd =>
                    cd.Doctor.DoctorSpecializations.Any(ds => ds.SpecializationId == query.SpecializationId.Value)));

        var clinicRows = await clinicsQuery
            .Select(x => new { x.ClinicId, x.Name, x.StreetAddress, x.City, x.IsActive, DoctorCount = x.ClinicDoctors.Select(cd => cd.DoctorId).Distinct().Count() })
            .ToListAsync(ct);

        var sortedRows = query.Sort?.Trim().ToLowerInvariant() switch
        {
            "namedesc" => clinicRows.OrderByDescending(x => x.Name).ThenBy(x => x.City).ToList(),
            "cityasc" => clinicRows.OrderBy(x => x.City).ThenBy(x => x.Name).ToList(),
            "citydesc" => clinicRows.OrderByDescending(x => x.City).ThenBy(x => x.Name).ToList(),
            "doctorcountdesc" => clinicRows.OrderByDescending(x => x.DoctorCount).ThenBy(x => x.Name).ToList(),
            _ => clinicRows.OrderBy(x => x.Name).ThenBy(x => x.City).ToList(),
        };

        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize switch { < 1 => 6, > 24 => 24, _ => query.PageSize };
        var totalCount = sortedRows.Count;
        var totalPages = totalCount == 0 ? 1 : (int)Math.Ceiling(totalCount / (double)pageSize);
        var pagedRows = sortedRows.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        var specializationLookup = await BuildSpecializationLookupAsync(pagedRows.Select(x => x.ClinicId).ToArray(), ct);

        var items = pagedRows.Select(x => new Dto.ClinicListItemDto(
            x.ClinicId, x.Name, x.StreetAddress, x.City, x.DoctorCount,
            specializationLookup.GetValueOrDefault(x.ClinicId, []), x.IsActive, false)).ToList();

        return new Dto.PagedResultDto<Dto.ClinicListItemDto>(items, page, pageSize, totalCount, totalPages);
    }

    private async Task<Dictionary<int, string[]>> BuildSpecializationLookupAsync(int[] clinicIds, CancellationToken ct)
    {
        if (clinicIds.Length == 0) return [];

        var rows = await _context.ClinicDoctors
            .AsNoTracking()
            .Where(cd => clinicIds.Contains(cd.ClinicId))
            .SelectMany(cd => cd.Doctor.DoctorSpecializations.Select(ds => new { cd.ClinicId, Specialization = ds.Specialization.Name }))
            .Distinct().ToListAsync(ct);

        return rows.GroupBy(x => x.ClinicId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Specialization).OrderBy(x => x).ToArray());
    }

    private async Task<int?> ResolveDoctorIdAsync(string userId, CancellationToken ct)
    {
        return await _context.Doctors.AsNoTracking()
            .Where(x => x.UserId == userId).Select(x => (int?)x.DoctorId).FirstOrDefaultAsync(ct);
    }

    private static bool IsDuplicateJoinRequest(string content, int clinicId, int doctorId)
    {
        try
        {
            var payload = JsonSerializer.Deserialize<ClinicJoinRequestPayload>(content);
            return payload is not null && payload.ClinicId == clinicId && payload.RequesterDoctorId == doctorId;
        }
        catch (JsonException) { return false; }
    }

    private static string? TrimToNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
