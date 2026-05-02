using Medreserve.Infrastructure;
using Microsoft.EntityFrameworkCore;
namespace Medreserve.Features.Specialization;

public class SpecializationService(DatabaseContext _context) : ISpecializationService
{
    
    public async Task<IEnumerable<SpecializationDto>> GetAllAsync()
    {
        return await _context.Specializations.Select(s=>new SpecializationDto(s.SpecializationId, s.Name, s.Description)).ToListAsync();
    }

    public async Task<SpecializationDto?> GetByIdAsync(int id)
    {
        var result  = await _context.Specializations.FindAsync(id);
        if (result == null) return null;
        return new SpecializationDto(result.SpecializationId, result.Name, result.Description);
    }

    public async Task<SpecializationDto> CreateAsync(CreateOrUpdateSpecializationDto dto)
    {
        var result = new Specialization
        {
            Name = dto.Name,
            Description = dto.Description
        };
        _context.Specializations.Add(result);
        await _context.SaveChangesAsync();
        return new SpecializationDto(result.SpecializationId, result.Name, result.Description);
    }

    public async Task<bool> UpdateAsync(int id, CreateOrUpdateSpecializationDto dto)
    {
    var result  = await _context.Specializations.FindAsync(id);
    if (result == null) return false;
    result.Name = dto.Name;
    result.Description = dto.Description;
    await _context.SaveChangesAsync();
    return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var result =await _context.Specializations.FindAsync(id);
        if (result == null) return false;
        _context.Specializations.Remove(result);
        await _context.SaveChangesAsync();
        return true;
    }
}