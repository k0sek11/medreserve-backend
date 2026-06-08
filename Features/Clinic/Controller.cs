using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Medreserve.Features.Clinic;

[ApiController]
[Route("api/clinics")]
[Authorize]
public class ClinicsController(IClinicService service) : ControllerBase
{
    private string? GetCurrentUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier);

    // ────────────────── GET /api/clinics (consolidated) ───────────────────

    [HttpGet]
    public async Task<IActionResult> GetClinics(
        [FromQuery] Dto.ClinicListQuery query,
        CancellationToken cancellationToken)
    {
        var result = await service.GetAsync(query, cancellationToken);
        return Ok(result);
    }

    // ──────────────────── GET /api/clinics/{id} (merged) ──────────────────

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Dto.ClinicDetailDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        var result = await service.GetByIdAsync(id, currentUserId, cancellationToken);
        if (result is null) return NotFound();
        return Ok(result);
    }

    // ───────────────────────────── POST /api/clinics ──────────────────────

    [HttpPost]
    public async Task<ActionResult<Dto.ClinicDto>> Create(
        [FromBody] Dto.CreateClinicRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(currentUserId)) return Unauthorized();

        try
        {
            var result = await service.CreateAsync(request, currentUserId, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.ClinicId }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ───────────────────────── PUT /api/clinics/{id} ──────────────────────

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Dto.ClinicDto>> Update(
        int id,
        [FromBody] Dto.UpdateClinicRequest request,
        CancellationToken cancellationToken)
    {
        var result = await service.UpdateAsync(id, request, cancellationToken);
        if (result is null) return NotFound();
        return Ok(result);
    }

    // ─────────────────────── DELETE /api/clinics/{id} ─────────────────────

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await service.DeleteAsync(id, cancellationToken);
        if (!deleted) return NotFound();
        return Ok(new { message = "Poprawnie usunieto klinike" });
    }

    // ────────────────────── GET /api/clinics/mine ─────────────────────────

    [HttpGet("mine")]
    public async Task<ActionResult<IReadOnlyList<Dto.ClinicListItemDto>>> GetMine(
        CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(currentUserId)) return Unauthorized();

        var result = await service.GetMineAsync(currentUserId, cancellationToken);
        return Ok(result);
    }

    // ──────────────── POST /api/clinics/{id}/join-request ─────────────────

    [HttpPost("{id:int}/join-request")]
    public async Task<IActionResult> RequestJoin(
        int id,
        [FromBody] Dto.CreateClinicJoinRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!request.ConfirmDoctor)
            return BadRequest(new { message = "Potwierdź checkbox, aby wysłać prośbę." });

        var currentUserId = GetCurrentUserId();
        if (currentUserId is null) return Unauthorized();

        var errorMessage = await service.RequestJoinAsync(id, request, currentUserId, cancellationToken);

        if (errorMessage is null) return Ok(new { message = "Prośba została wysłana." });
        if (errorMessage == "Nie znaleziono placówki.") return NotFound(new { message = errorMessage });
        return BadRequest(new { message = errorMessage });
    }
}