namespace Medreserve.Features.Clinic;

public interface IClinicService
{
    Task<object> GetAsync(Dto.ClinicListQuery query, CancellationToken cancellationToken);
    Task<Dto.ClinicDetailDto?> GetByIdAsync(int id, string? currentUserId, CancellationToken cancellationToken);

    Task<Dto.ClinicDto> CreateAsync(Dto.CreateClinicRequest request, string currentUserId, CancellationToken cancellationToken);

    Task<Dto.ClinicDto?> UpdateAsync(int id, Dto.UpdateClinicRequest request, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Dto.ClinicListItemDto>> GetMineAsync(string currentUserId, CancellationToken cancellationToken);

    Task<string?> RequestJoinAsync(int clinicId, Dto.CreateClinicJoinRequestDto request, string currentUserId, CancellationToken cancellationToken);
}
