using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Medreserve.Features.Users;
using Medreserve.Infrastructure;

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
            foreach (var specId in request.SpecializationIds)
            {
                doctor.DoctorSpecializations.Add(new DoctorSpecialization 
                { 
                    SpecializationId = specId 
                });
            }
        }
        
        _dbContext.Set<Doctor>().Add(doctor);
        await _dbContext.SaveChangesAsync();

        // 5. Aktywacja konta i rola
        // =========================================================================
        // TODO: Wersja produkcyjna - Zostawiamy IsActive = false!
        // Admin po weryfikacji licencji w osobnym endpointcie ustawi to na true.
        // =========================================================================
        
        user.IsActive = true; // Tymczasowe na czas testów
        await _userManager.UpdateAsync(user);
        
        await _userManager.AddToRoleAsync(user, "Doctor");

        return true;
    }
}