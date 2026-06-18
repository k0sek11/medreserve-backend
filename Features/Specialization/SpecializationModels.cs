using Medreserve.Features.Doctor;

namespace Medreserve.Features.Specialization;

public class Specialization
{
    public int SpecializationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<DoctorSpecialization> DoctorSpecializations { get; set; } = new List<DoctorSpecialization>();
}
