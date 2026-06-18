namespace Medreserve.Features.Specialization;

public interface ISpecializationService
{
    Task<IEnumerable<SpecializationDto>> GetAllAsync();
    Task<SpecializationDto?> GetByIdAsync(int id);
    Task<SpecializationDto> CreateAsync(CreateOrUpdateSpecializationDto dto);
    Task<bool> UpdateAsync(int id, CreateOrUpdateSpecializationDto dto);
    Task<bool> DeleteAsync(int id);
}
