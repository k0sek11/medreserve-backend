namespace Medreserve.Features.Patient;

public record CreatePatientProfileDto(
    string? Pesel, 
    DateTime? DateOfBirth, 
    string? Address
);