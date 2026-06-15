using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Medreserve.Features.Images;

[ApiController]
[Route("api/images")]
[Authorize]
public class ImagesController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;

    public ImagesController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    [HttpGet("profiles/{fileName}")]
    public IActionResult GetProfileImage(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return BadRequest();

        var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var filePath = Path.Combine(webRoot, "images", "profiles", fileName);

        if (!System.IO.File.Exists(filePath))
            return NotFound();

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var contentType = extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };

        return PhysicalFile(filePath, contentType);
    }
}