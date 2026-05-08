using Medreserve.Features.Doctor;
using Medreserve.Features.Geography;

namespace Medreserve.Features.Clinic;

public class Clinic
{
    public int ClinicId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string StreetAddress { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; }
    public int CityId { get; set; }

    public City City { get; set; } = null!;
    public ICollection<ClinicDoctor> ClinicDoctors { get; set; } = new List<ClinicDoctor>();
}
