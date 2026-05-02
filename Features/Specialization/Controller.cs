using Medreserve.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medreserve.Features.Specialization;

[ApiController]
[Route("api/specializations[controller]")]
public class SpecializationController(ISpecializationService _service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<SpecializationDto[]>> GetAllSpecializations()
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SpecializationDto>> GetSpecializationById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null) return NotFound("Nie znalezniono Specializacji o tym ID w Bazie");
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<SpecializationDto>> CreateSpecialization(CreateOrUpdateSpecializationDto dto)
    {
        var result = _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetSpecializationById),new{Id = result.Id}, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<SpecializationDto>> Update(int id, CreateOrUpdateSpecializationDto dto)
    {
        var success = await _service.UpdateAsync(id, dto);
        if (!success) return NotFound();
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<SpecializationDto>> Delete(int id)
    {
        var success = await _service.DeleteAsync(id);
        if (!success) return NotFound();
        return Ok("Nie znalezniono Specializacji o tym ID w Bazie");
    }
}