namespace Medreserve.Features.Specialization;

public record SpecializationDto(int Id, string Name, string Description);
public record CreateOrUpdateSpecializationDto(string Name, string? Description);
