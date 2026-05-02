using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Medreserve.Features.Patient;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PatientController : ControllerBase
{
    private readonly IPatientService _patientService;

    public PatientController(IPatientService patientService)
    {
        _patientService = patientService;
    }

    [HttpPost("profile")]
    public async Task<IActionResult> CreateProfile([FromBody] CreatePatientProfileDto request)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (currentUserId == null) 
            return Unauthorized();

        var success = await _patientService.CreateProfileAsync(currentUserId, request);

        if (!success) 
        {
            return BadRequest(new { message = "Failed to create profile." });
        }

        return Ok(new { message = "Patient profile created successfully!" });
    }
}