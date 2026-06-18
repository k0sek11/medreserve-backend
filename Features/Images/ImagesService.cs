using Medreserve.Features.Doctor;
using Medreserve.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Medreserve.Features.Images;

public sealed record ProfileImageInfo(string FilePath, string ContentType);

public class ImagesService : IImagesService
{
    private readonly IWebHostEnvironment _environment;
    private readonly DatabaseContext _dbContext;
    private const string ProfileImagesFolder = "images/profiles";
    private const string ProfileImagesUrlPrefix = "/api/images/profiles/";
    private const long MaxFileSize = 5 * 1024 * 1024;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    public ImagesService(IWebHostEnvironment environment, DatabaseContext dbContext)
    {
        _environment = environment;
        _dbContext = dbContext;
    }

    public async Task<string> UploadProfilePhotoAsync(string userId, IFormFile file, CancellationToken cancellationToken = default)
    {
        var doctor = await _dbContext.Set<Doctor.Doctor>()
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        if (doctor is null)
            throw new InvalidOperationException("Doctor profile not found.");

        var oldUrl = doctor.ProfileImageUrl;
        var newUrl = await SaveFileAsync(file, cancellationToken);

        doctor.ProfileImageUrl = newUrl;
        await _dbContext.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(oldUrl))
            DeleteFile(oldUrl);

        return newUrl;
    }

    public ProfileImageInfo? ResolveProfileImage(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return null;

        var filePath = Path.Combine(GetWebRoot(), ProfileImagesFolder, fileName);

        if (!File.Exists(filePath))
            return null;

        var contentType = Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };

        return new ProfileImageInfo(filePath, contentType);
    }

    private async Task<string> SaveFileAsync(IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length == 0)
            throw new ArgumentException("File is empty.");

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension))
            throw new ArgumentException("Only .jpg, .jpeg, .png and .webp files are allowed.");

        if (file.Length > MaxFileSize)
            throw new ArgumentException("File size must not exceed 5 MB.");

        var uploadsFolder = Path.Combine(GetWebRoot(), ProfileImagesFolder);
        Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream, cancellationToken);

        return $"{ProfileImagesUrlPrefix}{fileName}";
    }

    private void DeleteFile(string? relativeUrl)
    {
        if (string.IsNullOrWhiteSpace(relativeUrl))
            return;

        var cleanPath = relativeUrl.Replace(ProfileImagesUrlPrefix, ProfileImagesFolder + "/");
        var filePath = Path.Combine(GetWebRoot(), cleanPath.TrimStart('/'));

        if (File.Exists(filePath))
            File.Delete(filePath);
    }

    private string GetWebRoot() =>
        _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
}
