using Medreserve.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medreserve.Features.Specialization;

[ApiController]
[Route("api/specializations")]
public class SpecializationsController(DatabaseContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SpecializationDto>>> GetAll(CancellationToken cancellationToken)
    {
        var specializations = await dbContext
            .Specializations
            .AsNoTracking()
            .OrderBy(x => x.SpecializationId)
            .Select(x => new SpecializationDto(x.SpecializationId, x.Name, x.Description))
            .ToListAsync(cancellationToken);

        return Ok(specializations);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SpecializationDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var specialization = await dbContext
            .Specializations
            .AsNoTracking()
            .Where(x => x.SpecializationId == id)
            .Select(x => new SpecializationDto(x.SpecializationId, x.Name, x.Description))
            .FirstOrDefaultAsync(cancellationToken);

        return specialization is null ? NotFound() : Ok(specialization);
    }

    [HttpPost]
    public async Task<ActionResult<SpecializationDto>> Create(
        CreateSpecializationRequest request,
        CancellationToken cancellationToken
    )
    {
        var specialization = new Specialization
        {
            Name = request.Name,
            Description = request.Description
        };

        dbContext.Specializations.Add(specialization);
        await dbContext.SaveChangesAsync(cancellationToken);

        var result = new SpecializationDto(
            specialization.SpecializationId,
            specialization.Name,
            specialization.Description
        );

        return CreatedAtAction(nameof(GetById), new { id = specialization.SpecializationId }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<SpecializationDto>> Update(
        int id,
        UpdateSpecializationRequest request,
        CancellationToken cancellationToken
    )
    {
        var specialization = await dbContext.Specializations.FirstOrDefaultAsync(
            x => x.SpecializationId == id,
            cancellationToken
        );
        if (specialization is null)
        {
            return NotFound();
        }

        specialization.Name = request.Name;
        specialization.Description = request.Description;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(
            new SpecializationDto(
                specialization.SpecializationId,
                specialization.Name,
                specialization.Description
            )
        );
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var specialization = await dbContext.Specializations.FirstOrDefaultAsync(
            x => x.SpecializationId == id,
            cancellationToken
        );
        if (specialization is null)
        {
            return NotFound();
        }

        dbContext.Specializations.Remove(specialization);
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}

public sealed record SpecializationDto(int SpecializationId, string Name, string? Description);

public sealed record CreateSpecializationRequest(string Name, string? Description);

public sealed record UpdateSpecializationRequest(string Name, string? Description);
