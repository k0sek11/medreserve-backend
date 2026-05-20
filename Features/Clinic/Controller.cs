using System.Security.Claims;
using System.Text.Json;
using Medreserve.Features.Notification;
using Medreserve.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotificationEntity = Medreserve.Features.Notification.Notification;

namespace Medreserve.Features.Clinic;

[ApiController]
[Route("api/clinics")]
[Authorize]
public class ClinicsController(DatabaseContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ClinicDto>>> GetAll(CancellationToken cancellationToken)
    {
        var clinics = await dbContext
            .Clinics
            .AsNoTracking()
            .OrderBy(x => x.ClinicId)
            .Select(x => new ClinicDto(
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

        return Ok(clinics);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClinicDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var clinic = await dbContext
            .Clinics
            .AsNoTracking()
            .Where(x => x.ClinicId == id)
            .Select(x => new ClinicDto(
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

        return clinic is null ? NotFound() : Ok(clinic);
    }

    [HttpPost]
    public async Task<ActionResult<ClinicDto>> Create(CreateClinicRequest request, CancellationToken cancellationToken)
    {
        var clinic = new Clinic
        {
            Name = request.Name,
            Description = request.Description,
            StreetAddress = request.StreetAddress,
            OpeningHours = request.OpeningHours,
            MapLocation = request.MapLocation,
            CityId = request.CityId,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            IsActive = request.IsActive
        };

        dbContext.Clinics.Add(clinic);
        await dbContext.SaveChangesAsync(cancellationToken);

        var result = new ClinicDto(
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

        return CreatedAtAction(nameof(GetById), new { id = clinic.ClinicId }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ClinicDto>> Update(
        int id,
        UpdateClinicRequest request,
        CancellationToken cancellationToken
    )
    {
        var clinic = await dbContext.Clinics.FirstOrDefaultAsync(x => x.ClinicId == id, cancellationToken);
        if (clinic is null)
        {
            return NotFound();
        }

        clinic.StreetAddress = request.StreetAddress;
        clinic.Description = request.Description;
        clinic.OpeningHours = request.OpeningHours;
        clinic.MapLocation = request.MapLocation;
        clinic.CityId = request.CityId;
        clinic.PhoneNumber = request.PhoneNumber;
        clinic.Email = request.Email;
        clinic.IsActive = request.IsActive;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(
            new ClinicDto(
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

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var clinic = await dbContext.Clinics.FirstOrDefaultAsync(x => x.ClinicId == id, cancellationToken);
        if (clinic is null)
        {
            return NotFound();
        }

        dbContext.Clinics.Remove(clinic);
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpGet("cities")]
    public async Task<ActionResult<IReadOnlyList<CityDto>>> GetCities(CancellationToken cancellationToken)
    {
        var cities = await dbContext
            .Cities
            .AsNoTracking()
            .OrderBy(x => x.Voivodeship)
            .ThenBy(x => x.District)
            .ThenBy(x => x.Name)
            .Select(x => new CityDto(x.CityId, x.Name, x.District, x.Voivodeship))
            .ToListAsync(cancellationToken);

        return Ok(cities);
    }

    [HttpGet("specializations")]
    public async Task<ActionResult<IReadOnlyList<SpecializationDto>>> GetSpecializations(CancellationToken cancellationToken)
    {
        var specializations = await dbContext
            .Specializations
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new SpecializationDto(x.SpecializationId, x.Name, x.Description))
            .ToListAsync(cancellationToken);

        return Ok(specializations);
    }

    [HttpGet("cities/{cityId:int}/specializations")]
    public async Task<ActionResult<IReadOnlyList<SpecializationDto>>> GetSpecializationsByCity(
        int cityId,
        CancellationToken cancellationToken
    )
    {
        var cityExists = await dbContext
            .Cities
            .AsNoTracking()
            .AnyAsync(x => x.CityId == cityId, cancellationToken);

        if (!cityExists)
        {
            return NotFound("City not found");
        }

        var specializations = await dbContext.Doctors
            .AsNoTracking()
            .Where(d => d.ClinicDoctors.Any(cd => cd.Clinic.CityId == cityId))
            .SelectMany(d => d.DoctorSpecializations)
            .Select(x => x.Specialization)
            .Distinct()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return Ok(specializations.Select(x => new SpecializationDto(
            x.SpecializationId,
            x.Name,
            x.Description
        )));
    }

    [HttpGet("cities/by-specialization/{specializationId:int}")]
    public async Task<ActionResult<IReadOnlyList<CityDto>>> GetCitiesBySpecialization(
        int specializationId,
        CancellationToken cancellationToken
    )
    {
        var specializationExists = await dbContext
            .Specializations
            .AsNoTracking()
            .AnyAsync(x => x.SpecializationId == specializationId, cancellationToken);

        if (!specializationExists)
        {
            return NotFound("Specialization not found");
        }

        var cities = await dbContext.Doctors
            .AsNoTracking()
            .Where(d => d.DoctorSpecializations.Any(ds => ds.SpecializationId == specializationId))
            .SelectMany(d => d.ClinicDoctors.Select(cd => cd.Clinic.City))
            .Distinct()
            .OrderBy(x => x.Voivodeship)
            .ThenBy(x => x.District)
            .ThenBy(x => x.Name)
            .Select(x => new CityDto(x.CityId, x.Name, x.District, x.Voivodeship))
            .ToListAsync(cancellationToken);

        return Ok(cities);
    }

    [HttpGet("by-city/{cityId:int}")]
    public async Task<ActionResult<IReadOnlyList<ClinicDto>>> GetClinicsByCity(
        int cityId,
        CancellationToken cancellationToken
    )
    {
        var clinics = await dbContext
            .Clinics
            .AsNoTracking()
            .Where(x => x.CityId == cityId && x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new ClinicDto(
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

        return Ok(clinics);
    }

    [HttpGet("search")]
    public async Task<ActionResult<PagedResultDto<ClinicListItemDto>>> Search(
        [FromQuery] ClinicSearchQuery query,
        CancellationToken cancellationToken
    )
    {
        var clinicsQuery = dbContext
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
        var specializationsByClinic = await dbContext
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

        var clinics = pagedRows.Select(x => new ClinicListItemDto(
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

        return Ok(new PagedResultDto<ClinicListItemDto>(clinics.ToList(), page, pageSize, totalCount, totalPages));
    }

    [HttpGet("mine")]
    public async Task<ActionResult<IReadOnlyList<ClinicListItemDto>>> GetMyClinics(
        CancellationToken cancellationToken
    )
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var doctorId = await dbContext
            .Doctors
            .AsNoTracking()
            .Where(x => x.UserId == currentUserId)
            .Select(x => (int?)x.DoctorId)
            .FirstOrDefaultAsync(cancellationToken);

        if (doctorId is null)
        {
            return NotFound();
        }

        var clinicRows = await dbContext
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
        var specializationsByClinic = await dbContext
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

        var clinics = clinicRows.Select(x => new ClinicListItemDto(
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

        return Ok(clinics.ToList());
    }

    [HttpGet("{id:int}/details")]
    public async Task<ActionResult<ClinicDetailDto>> GetDetails(int id, CancellationToken cancellationToken)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var currentDoctorId = currentUserId is null
            ? null
            : await dbContext.Doctors
                .AsNoTracking()
                .Where(x => x.UserId == currentUserId)
                .Select(x => (int?)x.DoctorId)
                .FirstOrDefaultAsync(cancellationToken);

        var clinic = await dbContext
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
            return NotFound();
        }

        var doctors = await dbContext.ClinicDoctors
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
            .Select(x => new ClinicDoctorSummaryDto(
                x.DoctorId,
                x.FullName,
                x.PrimarySpecialization,
                x.IsOwner
            ))
            .ToListAsync(cancellationToken);

        var specializations = await dbContext.ClinicDoctors
            .AsNoTracking()
            .Where(x => x.ClinicId == id)
            .SelectMany(x => x.Doctor.DoctorSpecializations.Select(ds => ds.Specialization.Name))
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

        var isCurrentUserMember = currentDoctorId.HasValue && await dbContext.ClinicDoctors.AnyAsync(
            x => x.ClinicId == id && x.DoctorId == currentDoctorId.Value,
            cancellationToken
        );

        var isCurrentUserOwner = currentDoctorId.HasValue && await dbContext.ClinicDoctors.AnyAsync(
            x => x.ClinicId == id && x.DoctorId == currentDoctorId.Value && x.IsOwner,
            cancellationToken
        );

        return Ok(new ClinicDetailDto(
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

    [HttpPost("{id:int}/join-request")]
    public async Task<IActionResult> RequestJoin(int id, [FromBody] CreateClinicJoinRequestDto request, CancellationToken cancellationToken)
    {
        if (!request.ConfirmDoctor)
        {
            return BadRequest(new { message = "Potwierdź checkbox, aby wysłać prośbę." });
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var currentDoctor = await dbContext.Doctors
            .AsNoTracking()
            .Where(x => x.UserId == currentUserId)
            .Select(x => new { x.DoctorId, x.UserId })
            .FirstOrDefaultAsync(cancellationToken);

        if (currentDoctor is null)
        {
            return BadRequest(new { message = "Tylko lekarz może wysłać prośbę o dołączenie." });
        }

        var clinic = await dbContext.Clinics.AsNoTracking().FirstOrDefaultAsync(x => x.ClinicId == id, cancellationToken);
        if (clinic is null)
        {
            return NotFound();
        }

        var alreadyMember = await dbContext.ClinicDoctors.AnyAsync(
            x => x.ClinicId == id && x.DoctorId == currentDoctor.DoctorId,
            cancellationToken
        );

        if (alreadyMember)
        {
            return Conflict(new { message = "Jesteś już przypisany do tej przychodni." });
        }

        var pendingRequests = await dbContext.Notifications
            .AsNoTracking()
            .Where(x => x.Type == NotificationKinds.ClinicJoinRequest && x.Status == "Pending")
            .ToListAsync(cancellationToken);

        var duplicateExists = pendingRequests.Any(notification =>
        {
            var payload = DeserializeJoinRequestPayload(notification.Content);
            return payload is not null
                   && payload.ClinicId == id
                   && payload.RequesterDoctorId == currentDoctor.DoctorId;
        });

        if (duplicateExists)
        {
            return Conflict(new { message = "Masz już aktywną prośbę o dołączenie do tej przychodni." });
        }

        var owners = await dbContext.ClinicDoctors
            .AsNoTracking()
            .Where(x => x.ClinicId == id && x.IsOwner)
            .Select(x => new { x.Doctor.UserId })
            .ToListAsync(cancellationToken);

        if (owners.Count == 0)
        {
            return Conflict(new { message = "Ta przychodnia nie ma jeszcze właściciela." });
        }

        var requester = await dbContext.Users
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

        dbContext.Notifications.AddRange(notifications);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new { message = "Prośba została wysłana." });
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

public sealed record ClinicDto(
    int ClinicId,
    string Name,
    string? Description,
    string StreetAddress,
    string? OpeningHours,
    string? MapLocation,
    string? PhoneNumber,
    string? Email,
    bool IsActive
);

public sealed record CreateClinicRequest(
    string Name,
    string? Description,
    string StreetAddress,
    string? OpeningHours,
    string? MapLocation,
    int CityId,
    string? PhoneNumber,
    string? Email,
    bool IsActive
);

public sealed record UpdateClinicRequest(
    string Name,
    string? Description,
    string StreetAddress,
    string? OpeningHours,
    string? MapLocation,
    int CityId,
    string? PhoneNumber,
    string? Email,
    bool IsActive
);

public sealed record CityDto(
    int CityId,
    string Name,
    string District,
    string Voivodeship
);

public sealed record SpecializationDto(
    int SpecializationId,
    string Name,
    string? Description
);

public sealed record PagedResultDto<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount, int TotalPages);

public sealed record ClinicSearchQuery(
    string? Name,
    string? Location,
    int? CityId,
    int? SpecializationId,
    string? Sort,
    int Page = 1,
    int PageSize = 6
);

public sealed record ClinicListItemDto(
    int ClinicId,
    string Name,
    string StreetAddress,
    string City,
    int DoctorCount,
    IReadOnlyList<string> Specializations,
    bool IsActive,
    bool IsOwner
);

public sealed record ClinicDetailDto(
    int ClinicId,
    string Name,
    string? Description,
    string StreetAddress,
    string? OpeningHours,
    string? MapLocation,
    string? PhoneNumber,
    string? Email,
    int CityId,
    string City,
    string District,
    string Voivodeship,
    bool IsActive,
    int DoctorCount,
    IReadOnlyList<string> Specializations,
    IReadOnlyList<ClinicDoctorSummaryDto> Doctors,
    bool IsCurrentUserMember,
    bool IsCurrentUserOwner
);

public sealed record ClinicDoctorSummaryDto(
    int DoctorId,
    string FullName,
    string PrimarySpecialization,
    bool IsOwner
);

public sealed record CreateClinicJoinRequestDto(
    bool ConfirmDoctor,
    string? Message
);
