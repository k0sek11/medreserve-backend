using System.Security.Claims;
using System.Text.Json;
using Medreserve.Features.Notification;
using Medreserve.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotificationEntity = Medreserve.Features.Notification.Notification;

namespace Medreserve.Features.Clinic;

[ApiController]
[Route("api/clinics")]
[Authorize]
public class ClinicsController(IClinicService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Dto.ClinicDto>>> GetAll(CancellationToken cancellationToken)
    {
       var clinic = await service.GetAllClinicsAsync(cancellationToken);
       return Ok(clinic);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Dto.ClinicDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var clinic = await service.GetClinicByIdAsync(id, cancellationToken);
        if (clinic == null)
            return NotFound();
        return Ok(clinic);
    }

    [HttpPost]
    public async Task<ActionResult<Dto.ClinicDto>> Create(Dto.CreateClinicRequest request, CancellationToken cancellationToken)
    {
       var result = await service.CreateClinicAsync(request, cancellationToken);
       return CreatedAtAction(nameof(GetById), new { id = result.ClinicId }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Dto.ClinicDto>> Update(int id, Dto.UpdateClinicRequest request, CancellationToken cancellationToken)
    {
        var clinic = await service.UpdateClinicAsync(id, request, cancellationToken);
        if (clinic is null)
        {
            return NotFound();
        }
        return Ok(clinic);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var clinic = await service.DeleteClinicAsync(id, cancellationToken);
        if (clinic is false)
        {
            return NotFound();
        }
        
        return Ok("Poprawnie usunieto klinike");
    }

    [HttpGet("cities")]
    public async Task<ActionResult<IReadOnlyList<Dto.CityDto>>> GetCities(CancellationToken cancellationToken)
    {
        var cities = await service.GetAllCitiesAsync(cancellationToken);

        return Ok(cities);
    }

    [HttpGet("specializations")]
    public async Task<ActionResult<IReadOnlyList<Dto.ClinicSpecializationDto>>> GetSpecializations(CancellationToken cancellationToken)
    {
        var specializations = await service.GetSpecializationAsync(cancellationToken);

        return Ok(specializations);
    }

    [HttpGet("cities/{cityId:int}/specializations")]
    public async Task<ActionResult<IReadOnlyList<Dto.ClinicSpecializationDto>>> GetSpecializationsByCity(int cityId, CancellationToken cancellationToken)
    {
       var result = await service.GetSpecializationByCityAsync(cityId, cancellationToken);
       if(result is null)
           return NotFound();
       return Ok(result);
    }

    [HttpGet("cities/by-specialization/{specializationId:int}")]
    public async Task<ActionResult<IReadOnlyList<Dto.CityDto>>> GetCitiesBySpecialization(int specializationId, CancellationToken cancellationToken)
    {
        var result = await service.GetCitiesBySpecializationAsync(specializationId, cancellationToken);
        if (result is null)
            return NotFound();
        return Ok(result);
    }

    [HttpGet("by-city/{cityId:int}")]
    public async Task<ActionResult<IReadOnlyList<Dto.ClinicDto>>> GetClinicsByCity(int cityId, CancellationToken cancellationToken)
    {
      var result = await service.GetClinicsByCityAsync(cityId, cancellationToken);
      if (result is null)
          return NotFound();
      return Ok(result);
    }

    [HttpGet("search")]
    public async Task<ActionResult<Dto.PagedResultDto<Dto.ClinicListItemDto>>> Search([FromQuery] Dto.ClinicSearchQuery query, CancellationToken cancellationToken)
    {
        var result = await service.SearchAsync(query, cancellationToken);
        if (result is null)
            return NotFound();
        return Ok(result);
    }

    [HttpGet("mine")]
    public async Task<ActionResult<IReadOnlyList<Dto.ClinicListItemDto>>> GetMyClinics(CancellationToken cancellationToken)
    {
       var result = await service.GetMyClinicsAsync(currentUserId: User.FindFirstValue(ClaimTypes.NameIdentifier),cancellationToken);
       if (result is null)
           return NotFound();
       return Ok(result);
       
    }

    [HttpGet("{id:int}/details")]
    public async Task<ActionResult<Dto.ClinicDetailDto>> GetDetails(int id, CancellationToken cancellationToken)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await service.GetClinicDetailsAsync(id, currentUserId, cancellationToken);
        if (result is null)
            return NotFound();
        return Ok(result);
    }

    [HttpPost("{id:int}/join-request")]
    public async Task<IActionResult> RequestJoin(int id, [FromBody] Dto.CreateClinicJoinRequestDto request, CancellationToken cancellationToken)
    {
        if (!request.ConfirmDoctor)
        {
            return BadRequest(new { message = "Potwierdź checkbox, aby wysłać prośbę." });
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var errorMessage = await service.ReqestJoinAsync(id, request, currentUserId, cancellationToken);
    
        if (errorMessage is null)
        {
            return Ok(new { message = "Prośba została wysłana." });
        }

        if (errorMessage == "Nie znaleziono placówki.")
        {
            return NotFound(new { message = errorMessage });
        }

        return BadRequest(new { message = errorMessage });
    }
}
