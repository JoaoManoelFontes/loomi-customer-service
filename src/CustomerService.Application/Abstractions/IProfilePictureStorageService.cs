using CustomerService.Application.Customers.ProfilePictureUploadUrl;

namespace CustomerService.Application.Abstractions;

public interface IProfilePictureStorageService
{
    Task<ProfilePictureUploadTarget> CreateUploadUrlAsync(
        Guid customerId,
        string originalFileName,
        string contentType,
        long fileSizeInBytes,
        CancellationToken cancellationToken = default);
}
