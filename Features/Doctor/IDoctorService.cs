namespace Medreserve.Features.Doctor;

public interface IDoctorService
{
    Task<bool> CreateProfileAsync(string userId, CreateDoctorProfileDto request);
    Task<DoctorProfileDto?> GetMyProfileAsync(string userId, CancellationToken cancellationToken);
    Task<bool> UpdateMyProfileAsync(string userId, UpdateDoctorProfileDto request, CancellationToken cancellationToken);
    Task<DoctorAppointmentTypeDto?> CreateMyAppointmentTypeAsync(string userId, CreateDoctorAppointmentTypeDto request, CancellationToken cancellationToken);
    Task<bool> DeleteMyAppointmentTypeAsync(string userId, int appointmentTypeId, CancellationToken cancellationToken);
    Task<IReadOnlyList<DoctorScheduleDto>?> GetMySchedulesAsync(string userId, CancellationToken cancellationToken);
    Task<DoctorScheduleDto?> UpsertMyScheduleAsync(string userId, UpsertDoctorScheduleDto request, CancellationToken cancellationToken);
    Task<bool> DeleteMyScheduleAsync(string userId, int scheduleId, CancellationToken cancellationToken);
    Task<DoctorPublicProfileDto?> GetPublicProfileAsync(int doctorId, CancellationToken cancellationToken);
    Task<DoctorAvailabilityDto?> GetAvailabilityAsync(int doctorId, DateOnly date, int appointmentTypeId, int? clinicId, CancellationToken cancellationToken);
    Task<DoctorAvailabilityCalendarDto?> GetAvailabilityCalendarAsync(int doctorId, int year, int month, int appointmentTypeId, int? clinicId, CancellationToken cancellationToken);
    Task<IReadOnlyList<DoctorDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<DoctorDto?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<PagedResultDto<DoctorSearchItemDto>> SearchDoctorsAsync(DoctorSearchQueryDto query, CancellationToken cancellationToken);
}
