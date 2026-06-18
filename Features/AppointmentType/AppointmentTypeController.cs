using Microsoft.AspNetCore.Mvc;

namespace Medreserve.Features.AppointmentType;

[ApiController]
[Route("api/appointment-types")]
public class AppointmentTypesController : ControllerBase
{
    private readonly IAppointmentTypeService _service;

    public AppointmentTypesController(IAppointmentTypeService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AppointmentTypeDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _service.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AppointmentTypeDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<AppointmentTypeDto>> Create(
        CreateAppointmentTypeRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.AppointmentTypeId }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AppointmentTypeDto>> Update(
        int id,
        UpdateAppointmentTypeRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _service.UpdateAsync(id, request, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
