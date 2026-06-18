using Medreserve.Features.Images;

public interface IImagesService
{
    Task<string> UploadProfilePhotoAsync(string userId, IFormFile file, CancellationToken cancellationToken = default);
    ProfileImageInfo? ResolveProfileImage(string? fileName);
}
