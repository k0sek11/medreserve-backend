using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Medreserve.Features.Users;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly UserManager<User> _userManager;

    public UsersController(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet("me")]
    public async Task<ActionResult<PatientProfileDto>> GetMyProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return NotFound();

        return Ok(new PatientProfileDto(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            user.PhoneNumber ?? "",
            user.BirthDate,
            user.Gender,
            user.IsActive
        ));
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdatePatientProfileDto request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return NotFound();

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        if (request.PhoneNumber is not null) user.PhoneNumber = request.PhoneNumber.Trim();
        if (request.BirthDate is not null) user.BirthDate = request.BirthDate;
        if (request.Gender is not null) user.Gender = request.Gender.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded ? Ok(new { message = "Profile updated successfully." }) : BadRequest(new { message = "Failed to update profile." });
    }
}