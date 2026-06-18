using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Medreserve.Features.Images;

[ApiController]
[Route("api/images")]
[Authorize]
public class ImagesController : ControllerBase
{
    private readonly IImagesService _imagesService;

    public ImagesController(IImagesService imagesService)
    {
        _imagesService = imagesService;
    }

    [HttpGet("profiles/{fileName}")]
    public IActionResult GetProfileImage(string fileName)
    {
        var info = _imagesService.ResolveProfileImage(fileName);
        return info is null ? NotFound() : PhysicalFile(info.FilePath, info.ContentType);
    }

    [HttpPost("profiles")]
    [Authorize]
    public async Task<IActionResult> UploadProfilePhoto(IFormFile file, CancellationToken cancellationToken)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
            return Unauthorized();

        if (file is null || file.Length == 0)
            return BadRequest(new { message = "No file provided." });

        try
        {
            var url = await _imagesService.UploadProfilePhotoAsync(currentUserId, file, cancellationToken);
            return Ok(new { profileImageUrl = url });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }
}
