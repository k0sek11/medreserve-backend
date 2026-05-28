using Medreserve.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Medreserve.Features.Doctor;
using Medreserve.Features.Notification;
using Medreserve.Features.Specialization;
using NotificationEntity = Medreserve.Features.Notification.Notification;

namespace Medreserve.Features.Clinic;

public class ClinicService (DatabaseContext _context) : IClinicService
{
    public async Task<IReadOnlyList<Dto.ClinicDto>> GetAllClinicsAsync(CancellationToken cancellationToken)
    {
        var clinics = await _context
            .Clinics
            .AsNoTracking()
            .OrderBy(x => x.ClinicId)
            .Select(x => new Dto.ClinicDto(
                x.ClinicId,
                x.Name,
                x.Description,
                x.StreetAddress,
                x.OpeningHours,
                x.MapLocation,
                x.PhoneNumber,
                x.Email,
                x.IsActive
            ))
            .ToListAsync(cancellationToken);

        return  clinics;
        
    }

    public async Task<Dto.ClinicDto?> GetClinicByIdAsync(int id, CancellationToken cancellationToken)
    {
        var clinic = await _context
            .Clinics
            .AsNoTracking()
            .Where(x => x.ClinicId == id)
            .Select(x => new Dto.ClinicDto(
                x.ClinicId,
                x.Name,
                x.Description,
                x.StreetAddress,
                x.OpeningHours,
                x.MapLocation,
                x.PhoneNumber,
                x.Email,
                x.IsActive
            ))
            .FirstOrDefaultAsync(cancellationToken);
        return clinic;
    }

    public async Task<Dto.ClinicDto> CreateClinicAsync(Dto.CreateClinicRequest request, string currentUserId, CancellationToken cancellationToken)
    {
        var doctor = await _context.Doctors.FirstOrDefaultAsync(x => x.UserId == currentUserId, cancellationToken);
        if (doctor is null)
        {
            throw new InvalidOperationException("Tylko lekarz może zarejestrować przychodnię.");
        }

        var cityExists = await _context.Cities.AnyAsync(x => x.CityId == request.CityId, cancellationToken);
        if (!cityExists)
        {
            throw new InvalidOperationException("Nie znaleziono miasta.");
        }

        var openingHours = string.IsNullOrWhiteSpace(request.OpeningHours) ? null : request.OpeningHours.Trim();

        var clinic = new Clinic
        {
            Name = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            StreetAddress = request.StreetAddress.Trim(),
            OpeningHours = openingHours,
            MapLocation = string.IsNullOrWhiteSpace(request.MapLocation) ? null : request.MapLocation.Trim(),
            CityId = request.CityId,
            PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim(),
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
            IsActive = true
        };

        clinic.ClinicDoctors.Add(new ClinicDoctor
        {
            DoctorId = doctor.DoctorId,
            IsOwner = true
        });

        _context.Clinics.Add(clinic);
        await _context.SaveChangesAsync(cancellationToken);


        var result = new Dto.ClinicDto(
            clinic.ClinicId,
            clinic.Name,
            clinic.Description,
            clinic.StreetAddress,
            clinic.OpeningHours,
            clinic.MapLocation,
            clinic.PhoneNumber,
            clinic.Email,
            clinic.IsActive
        );

        return result;
    }

    public async Task<bool> DeleteClinicAsync(int id, CancellationToken cancellationToken)
    {
        var clinic = await _context.Clinics.FirstOrDefaultAsync(x => x.ClinicId == id, cancellationToken);
        if (clinic is null)
        {
            return false;
        }

        _context.Clinics.Remove(clinic);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<Dto.ClinicDto?> UpdateClinicAsync(int id,Dto.UpdateClinicRequest request, CancellationToken cancellationToken)
    {
        
            var clinic = await _context.Clinics.FirstOrDefaultAsync(x => x.ClinicId == id, cancellationToken);
            if (clinic is null)
            {
                return null;
            }

            clinic.StreetAddress = request.StreetAddress;
            clinic.Description = request.Description;
            clinic.OpeningHours = request.OpeningHours;
            clinic.MapLocation = request.MapLocation;
            clinic.CityId = request.CityId;
            clinic.PhoneNumber = request.PhoneNumber;
            clinic.Email = request.Email;
            clinic.IsActive = request.IsActive;

            await _context.SaveChangesAsync(cancellationToken);

            return (
                new Dto.ClinicDto(
                    clinic.ClinicId,
                    clinic.Name,
                    clinic.Description,
                    clinic.StreetAddress,
                    clinic.OpeningHours,
                    clinic.MapLocation,
                    clinic.PhoneNumber,
                    clinic.Email,
                    clinic.IsActive
                )
            );
    }

    public async Task<IReadOnlyList<Dto.CityDto>> GetAllCitiesAsync(CancellationToken cancellationToken)
    {
        var cities = await _context
            .Cities
            .AsNoTracking()
            .OrderBy(x => x.Voivodeship)
            .ThenBy(x => x.District)
            .ThenBy(x => x.Name)
            .Select(x => new Dto.CityDto(x.CityId, x.Name, x.District, x.Voivodeship))
            .ToListAsync(cancellationToken);

        return cities;
    }

    public async Task<IReadOnlyList<Dto.ClinicSpecializationDto>> GetSpecializationAsync(CancellationToken cancellationToken)
    {
        var specializations = await _context
            .Specializations
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new Dto.ClinicSpecializationDto(x.SpecializationId, x.Name, x.Description))
            .ToListAsync(cancellationToken);

        return specializations;
    }

    public async Task<IReadOnlyList<Dto.ClinicSpecializationDto>?> GetSpecializationByCityAsync(int cityId, CancellationToken cancellationToken)
    {
        
        var cityExists = await _context
            .Cities
            .AsNoTracking()
            .AnyAsync(x => x.CityId == cityId, cancellationToken);

        if (!cityExists)
        {
            return null;
        }

        var specializations = await _context.Doctors
            .AsNoTracking()
            .Where(d => d.ClinicDoctors.Any(cd => cd.Clinic.CityId == cityId))
            .SelectMany(d => d.DoctorSpecializations)
            .Select(x => x.Specialization)
            .Distinct()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return specializations.Select(x => new Dto.ClinicSpecializationDto(
                x.SpecializationId,
                x.Name,
                x.Description))
            .ToList();
    }

    public async Task<IReadOnlyList<Dto.CityDto>?> GetCitiesBySpecializationAsync(int specializationId, CancellationToken cancellationToken)
    {
       
        var specializationExists = await _context
            .Specializations
            .AsNoTracking()
            .AnyAsync(x => x.SpecializationId == specializationId, cancellationToken);

        if (!specializationExists)
        {
            return null;
        }

        var cities = await _context.Doctors
            .AsNoTracking()
            .Where(d => d.DoctorSpecializations.Any(ds => ds.SpecializationId == specializationId))
            .SelectMany(d => d.ClinicDoctors.Select(cd => cd.Clinic.City))
            .Distinct()
            .OrderBy(x => x.Voivodeship)
            .ThenBy(x => x.District)
            .ThenBy(x => x.Name)
            .Select(x => new Dto.CityDto(x.CityId, x.Name, x.District, x.Voivodeship))
            .ToListAsync(cancellationToken);

        return cities;
    }
    

    public async Task<IReadOnlyList<Dto.ClinicDto>> GetClinicsByCityAsync(int cityId, CancellationToken cancellationToken)
    {
        var clinics = await _context
            .Clinics
            .AsNoTracking()
            .Where(x => x.CityId == cityId && x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new Dto.ClinicDto(
                x.ClinicId,
                x.Name,
                x.Description,
                x.StreetAddress,
                x.OpeningHours,
                x.MapLocation,
                x.PhoneNumber,
                x.Email,
                x.IsActive
            ))
            .ToListAsync(cancellationToken);

        return clinics;
    }

    public async  Task<Dto.PagedResultDto<Dto.ClinicListItemDto>> SearchAsync(Dto.ClinicSearchQuery query, CancellationToken cancellationToken)
    {
       var clinicsQuery = _context
            .Clinics
            .AsNoTracking()
            .Where(x => x.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Name))
        {
            var name = query.Name.Trim();
            clinicsQuery = clinicsQuery.Where(x =>
                EF.Functions.ILike(x.Name, $"%{name}%")
                || EF.Functions.ILike(x.StreetAddress, $"%{name}%")
                || EF.Functions.ILike(x.City.Name, $"%{name}%")
                || EF.Functions.ILike(x.City.District, $"%{name}%")
            );
        }

        if (!string.IsNullOrWhiteSpace(query.Location))
        {
            var location = query.Location.Trim();
            clinicsQuery = clinicsQuery.Where(x =>
                EF.Functions.ILike(x.StreetAddress, $"%{location}%")
                || EF.Functions.ILike(x.City.Name, $"%{location}%")
                || EF.Functions.ILike(x.City.District, $"%{location}%")
            );
        }

        if (query.CityId.HasValue)
        {
            clinicsQuery = clinicsQuery.Where(x => x.CityId == query.CityId.Value);
        }

        if (query.SpecializationId.HasValue)
        {
            clinicsQuery = clinicsQuery.Where(x =>
                x.ClinicDoctors.Any(cd =>
                    cd.Doctor.DoctorSpecializations.Any(ds => ds.SpecializationId == query.SpecializationId.Value)
                )
            );
        }

        var clinicRows = await clinicsQuery
            .Select(x => new
            {
                x.ClinicId,
                x.Name,
                x.StreetAddress,
                City = x.City.Name,
                x.IsActive,
                DoctorCount = x.ClinicDoctors.Select(cd => cd.DoctorId).Distinct().Count(),
            })
            .ToListAsync(cancellationToken);

        var sortedRows = query.Sort?.Trim().ToLowerInvariant() switch
        {
            "namedesc" => clinicRows.OrderByDescending(x => x.Name).ThenBy(x => x.City).ToList(),
            "cityasc" => clinicRows.OrderBy(x => x.City).ThenBy(x => x.Name).ToList(),
            "citydesc" => clinicRows.OrderByDescending(x => x.City).ThenBy(x => x.Name).ToList(),
            "doctorcountdesc" => clinicRows.OrderByDescending(x => x.DoctorCount).ThenBy(x => x.Name).ToList(),
            _ => clinicRows.OrderBy(x => x.Name).ThenBy(x => x.City).ToList(),
        };

        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize switch
        {
            < 1 => 6,
            > 24 => 24,
            _ => query.PageSize,
        };

        var totalCount = sortedRows.Count;
        var totalPages = totalCount == 0 ? 1 : (int)Math.Ceiling(totalCount / (double)pageSize);
        var pagedRows = sortedRows
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var clinicIds = pagedRows.Select(x => x.ClinicId).ToArray();
        var specializationsByClinic = await _context
            .ClinicDoctors
            .AsNoTracking()
            .Where(cd => clinicIds.Contains(cd.ClinicId))
            .SelectMany(cd => cd.Doctor.DoctorSpecializations.Select(ds => new
            {
                cd.ClinicId,
                Specialization = ds.Specialization.Name,
            }))
            .Distinct()
            .ToListAsync(cancellationToken);

        var specializationLookup = specializationsByClinic
            .GroupBy(x => x.ClinicId)
            .ToDictionary(
                group => group.Key,
                group => group.Select(x => x.Specialization).OrderBy(x => x).ToArray()
            );

        var clinics = pagedRows.Select(x => new Dto.ClinicListItemDto(
            x.ClinicId,
            x.Name,
            x.StreetAddress,
            x.City,
            x.DoctorCount,
            specializationLookup.TryGetValue(x.ClinicId, out var specializations)
                ? specializations
                : Array.Empty<string>(),
            x.IsActive,
            false
        ));

        return (new Dto.PagedResultDto<Dto.ClinicListItemDto>(clinics.ToList(), page, pageSize, totalCount, totalPages));
   
    }

    public async Task<IReadOnlyList<Dto.ClinicListItemDto>> GetMyClinicsAsync(string currentUserId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return [];
        }
        var doctorId = await _context
            .Doctors
            .AsNoTracking()
            .Where(x => x.UserId == currentUserId)
            .Select(x => (int?)x.DoctorId)
            .FirstOrDefaultAsync(cancellationToken);

        if (doctorId is null)
        {
            return [];
        }

        var clinicRows = await _context
            .ClinicDoctors
            .AsNoTracking()
            .Where(cd => cd.DoctorId == doctorId.Value)
            .Select(cd => new
            {
                cd.ClinicId,
                cd.Clinic.Name,
                cd.Clinic.StreetAddress,
                City = cd.Clinic.City.Name,
                cd.Clinic.IsActive,
                cd.IsOwner,
                DoctorCount = cd.Clinic.ClinicDoctors.Select(item => item.DoctorId).Distinct().Count(),
            })
            .OrderByDescending(x => x.IsOwner)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var clinicIds = clinicRows.Select(x => x.ClinicId).ToArray();
        var specializationsByClinic = await _context
            .ClinicDoctors
            .AsNoTracking()
            .Where(cd => clinicIds.Contains(cd.ClinicId))
            .SelectMany(cd => cd.Doctor.DoctorSpecializations.Select(ds => new
            {
                cd.ClinicId,
                Specialization = ds.Specialization.Name,
            }))
            .Distinct()
            .ToListAsync(cancellationToken);

        var specializationLookup = specializationsByClinic
            .GroupBy(x => x.ClinicId)
            .ToDictionary(
                group => group.Key,
                group => group.Select(x => x.Specialization).OrderBy(x => x).ToArray()
            );

        var clinics = clinicRows.Select(x => new Dto.ClinicListItemDto(
            x.ClinicId,
            x.Name,
            x.StreetAddress,
            x.City,
            x.DoctorCount,
            specializationLookup.TryGetValue(x.ClinicId, out var specializations)
                ? specializations
                : Array.Empty<string>(),
            x.IsActive,
            x.IsOwner
        ));

        return (clinics.ToList());
    }

    public async Task<Dto.ClinicDetailDto?> GetClinicDetailsAsync(int id, string? currentUserId, CancellationToken cancellationToken)
    {
        var currentDoctorId = currentUserId is null
            ? null
            : await _context.Doctors
                .AsNoTracking()
                .Where(x => x.UserId == currentUserId)
                .Select(x => (int?)x.DoctorId)
                .FirstOrDefaultAsync(cancellationToken);

        var clinic = await _context
            .Clinics
            .AsNoTracking()
            .Where(x => x.ClinicId == id)
            .Select(x => new
            {
                x.ClinicId,
                x.Name,
                x.Description,
                x.StreetAddress,
                x.OpeningHours,
                x.MapLocation,
                x.PhoneNumber,
                x.Email,
                x.CityId,
                City = x.City.Name,
                District = x.City.District,
                Voivodeship = x.City.Voivodeship,
                x.IsActive,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (clinic is null)
        {
            return null;
        }

        var doctors = await _context.ClinicDoctors
            .AsNoTracking()
            .Where(x => x.ClinicId == id)
            .Select(x => new
            {
                x.DoctorId,
                FullName = x.Doctor.User.FirstName + " " + x.Doctor.User.LastName,
                PrimarySpecialization = x.Doctor.DoctorSpecializations
                    .Select(ds => ds.Specialization.Name)
                    .OrderBy(name => name)
                    .FirstOrDefault() ?? "Lekarz",
                x.IsOwner,
            })
            .OrderByDescending(x => x.IsOwner)
            .ThenBy(x => x.FullName)
            .Select(x => new Dto.ClinicDoctorSummaryDto(
                x.DoctorId,
                x.FullName,
                x.PrimarySpecialization,
                x.IsOwner
            ))
            .ToListAsync(cancellationToken);

        var specializations = await _context.ClinicDoctors
            .AsNoTracking()
            .Where(x => x.ClinicId == id)
            .SelectMany(x => x.Doctor.DoctorSpecializations.Select(ds => ds.Specialization.Name))
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

        var isCurrentUserMember = currentDoctorId.HasValue && await _context.ClinicDoctors.AnyAsync(
            x => x.ClinicId == id && x.DoctorId == currentDoctorId.Value,
            cancellationToken
        );

        var isCurrentUserOwner = currentDoctorId.HasValue && await _context.ClinicDoctors.AnyAsync(
            x => x.ClinicId == id && x.DoctorId == currentDoctorId.Value && x.IsOwner,
            cancellationToken
        );

        return (new Dto.ClinicDetailDto(
            clinic.ClinicId,
            clinic.Name,
            clinic.Description,
            clinic.StreetAddress,
            clinic.OpeningHours,
            clinic.MapLocation,
            clinic.PhoneNumber,
            clinic.Email,
            clinic.CityId,
            clinic.City,
            clinic.District,
            clinic.Voivodeship,
            clinic.IsActive,
            doctors.Count,
            specializations,
            doctors,
            isCurrentUserMember,
            isCurrentUserOwner
        ));
    }

    public async Task<string?> ReqestJoinAsync(int clinicId, Dto.CreateClinicJoinRequestDto request, string currentUserId,
        CancellationToken cancellationToken)
    {
         if (string.IsNullOrWhiteSpace(currentUserId))
    {
        return "Brak autoryzacji.";
    }

    var currentDoctor = await _context.Doctors
        .AsNoTracking()
        .Where(x => x.UserId == currentUserId)
        .Select(x => new { x.DoctorId, x.UserId })
        .FirstOrDefaultAsync(cancellationToken);

    if (currentDoctor is null)
    {
        return "Tylko lekarz może wysłać prośbę o dołączenie.";
    }

    var clinic = await _context.Clinics
        .AsNoTracking()
        .FirstOrDefaultAsync(x => x.ClinicId == clinicId, cancellationToken);
        
    if (clinic is null)
    {
        return "Nie znaleziono placówki.";
    }

    var alreadyMember = await _context.ClinicDoctors.AnyAsync(
        x => x.ClinicId == clinicId && x.DoctorId == currentDoctor.DoctorId,
        cancellationToken
    );

    if (alreadyMember)
    {
        return "Jesteś już przypisany do tej przychodni.";
    }

    var pendingRequests = await _context.Notifications
        .AsNoTracking()
        .Where(x => x.Type == NotificationKinds.ClinicJoinRequest && x.Status == "Pending")
        .ToListAsync(cancellationToken);

    var duplicateExists = pendingRequests.Any(notification =>
    {
        var payload = DeserializeJoinRequestPayload(notification.Content);
        return payload is not null
               && payload.ClinicId == clinicId
               && payload.RequesterDoctorId == currentDoctor.DoctorId;
    });

    if (duplicateExists)
    {
        return "Masz już aktywną prośbę o dołączenie do tej przychodni.";
    }

    var owners = await _context.ClinicDoctors
        .AsNoTracking()
        .Where(x => x.ClinicId == clinicId && x.IsOwner)
        .Select(x => new { x.Doctor.UserId })
        .ToListAsync(cancellationToken);

    if (owners.Count == 0)
    {
        return "Ta przychodnia nie ma jeszcze właściciela.";
    }

    var requester = await _context.Users
        .AsNoTracking()
        .Where(x => x.Id == currentUserId)
        .Select(x => new { x.FirstName, x.LastName })
        .FirstAsync(cancellationToken);

    var payload = new ClinicJoinRequestPayload(
        Guid.NewGuid().ToString("N"),
        clinic.ClinicId,
        clinic.Name,
        currentDoctor.DoctorId,
        currentUserId,
        $"{requester.FirstName} {requester.LastName}",
        string.IsNullOrWhiteSpace(request.Message) ? null : request.Message.Trim()
    );

    var payloadJson = JsonSerializer.Serialize(payload);
    
    var notifications = owners.Select(owner => new NotificationEntity
    {
        UserId = owner.UserId,
        Type = NotificationKinds.ClinicJoinRequest,
        Subject = $"Prośba o dołączenie do {clinic.Name}",
        Content = payloadJson,
        Status = "Pending",
        CreatedAt = DateTime.UtcNow,
    });

    _context.Notifications.AddRange(notifications);
    await _context.SaveChangesAsync(cancellationToken);

    return null;
    }



private static ClinicJoinRequestPayload? DeserializeJoinRequestPayload(string content)
{
    try
    {
        return JsonSerializer.Deserialize<ClinicJoinRequestPayload>(content);
    }
    catch (JsonException)
    {
        return null;
    }
}
}