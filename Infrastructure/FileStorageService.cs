namespace Medreserve.Infrastructure;

public interface IFileStorageService
{
    Task<string> SaveProfileImageAsync(IFormFile file, CancellationToken cancellationToken = default);
    bool DeleteFile(string? relativeUrl);
}

public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string ProfileImagesFolder = "images/profiles";

    public FileStorageService(IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor)
    {
        _environment = environment;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string> SaveProfileImageAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file.Length == 0)
            throw new ArgumentException("File is empty.");

        var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(file.FileName);
        if (!allowedExtensions.Contains(extension))
            throw new ArgumentException("Only .jpg, .jpeg, .png and .webp files are allowed.");

        if (file.Length > 5 * 1024 * 1024)
            throw new ArgumentException("File size must not exceed 5 MB.");

        var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var uploadsFolder = Path.Combine(webRoot, ProfileImagesFolder);
        Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream, cancellationToken);

        return $"/api/images/profiles/{fileName}";
    }

    public bool DeleteFile(string? relativeUrl)
    {
        if (string.IsNullOrWhiteSpace(relativeUrl)) return false;

        var cleanPath = relativeUrl.Replace("/api/images/profiles/", "/images/profiles/")
                                    .Replace("/images/profiles/", "/images/profiles/");
        var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var filePath = Path.Combine(webRoot, cleanPath.TrimStart('/'));

        if (!File.Exists(filePath)) return false;

        File.Delete(filePath);
        return true;
    }
}