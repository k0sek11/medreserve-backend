namespace Medreserve.Features.Clinic;
using Medreserve.Features.Clinic;
public interface IClinicService
{
    Task<IReadOnlyList<Dto.ClinicDto>> GetAllClinicsAsync(CancellationToken cancellationToken);
    Task<Dto.ClinicDto> GetClinicByIdAsync(int id, CancellationToken cancellationToken);
    Task<Dto.ClinicDto> CreateClinicAsync(Dto.CreateClinicRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteClinicAsync(int id, CancellationToken cancellationToken);
    Task<Dto.ClinicDto?> UpdateClinicAsync(int id, Dto.UpdateClinicRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<Dto.CityDto>> GetAllCitiesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<Dto.ClinicSpecializationDto>> GetSpecializationAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<Dto.ClinicSpecializationDto>?> GetSpecializationByCityAsync(int cityId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Dto.CityDto>> GetCitiesBySpecializationAsync(int specializationId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Dto.ClinicDto>> GetClinicsByCityAsync(int cityId, CancellationToken cancellationToken);

    Task<Dto.PagedResultDto<Dto.ClinicListItemDto>> SearchAsync(Dto.ClinicSearchQuery query, CancellationToken cancellationToken);

    Task<IReadOnlyList<Dto.ClinicListItemDto?>>
        GetMyClinicsAsync(string currentUserId, CancellationToken cancellationToken);

    Task<Dto.ClinicDetailDto?> GetClinicDetailsAsync(int id, string? currentUserId, CancellationToken cancellationToken);

    Task<string?> ReqestJoinAsync(int clinicId, Dto.CreateClinicJoinRequestDto request, string currentUserId,
        CancellationToken cancellationToken);
};