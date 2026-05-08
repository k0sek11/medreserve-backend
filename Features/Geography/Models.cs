namespace Medreserve.Features.Geography;

public class City
{
    public int CityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Voivodeship { get; set; } = string.Empty;

    public ICollection<Clinic.Clinic> Clinics { get; set; } = new List<Clinic.Clinic>();
}
