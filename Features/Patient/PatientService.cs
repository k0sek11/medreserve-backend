using Microsoft.AspNetCore.Identity;
using Medreserve.Features.Users;
using Medreserve.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Medreserve.Features.Patient;

public class PatientService : IPatientService
{
    private readonly DatabaseContext _dbContext;
    private readonly UserManager<User> _userManager;

    public PatientService(DatabaseContext dbContext, UserManager<User> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<bool> CreateProfileAsync(string userId, CreatePatientProfileDto request)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;
        
        var profileExists = await _dbContext.Set<Patient>().AnyAsync(p => p.UserId == userId);
        if (profileExists) return false; 
        
        var patient = new Patient
        {
            UserId = userId,
            Pesel = request.Pesel,
            DateOfBirth = request.DateOfBirth,
            Address = request.Address
        };
        
        _dbContext.Set<Patient>().Add(patient);
        await _dbContext.SaveChangesAsync();
        
        user.IsActive = true;
        await _userManager.UpdateAsync(user);
        
        await _userManager.AddToRoleAsync(user, "Patient");

        return true;
    }
}