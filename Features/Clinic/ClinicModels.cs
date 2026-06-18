using Medreserve.Features.Doctor;

namespace Medreserve.Features.Clinic;

public class Clinic
{
    public int ClinicId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string StreetAddress { get; set; } = string.Empty;
    public string? OpeningHours { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; }
    public string City { get; set; } = string.Empty;

    public ICollection<ClinicDoctor> ClinicDoctors { get; set; } = new List<ClinicDoctor>();
}
