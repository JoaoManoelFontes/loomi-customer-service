using CustomerService.Application.Abstractions;
using CustomerService.Application.Customers.ProfilePictureUploadUrl;
using Microsoft.Extensions.Options;

namespace CustomerService.Infrastructure.Storage;

public sealed class LocalProfilePictureStorageService(IOptions<AzureBlobStorageOptions> options)
    : IProfilePictureStorageService
{
    private static readonly IReadOnlyDictionary<string, string> RequiredHeaders =
        new Dictionary<string, string>
        {
            ["x-ms-blob-type"] = "BlockBlob"
        };

    public Task<ProfilePictureUploadTarget> CreateUploadUrlAsync(
        Guid customerId,
        string originalFileName,
        string contentType,
        long fileSizeInBytes,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(options.Value.UploadUrlExpiresInMinutes);
        var blobName = BuildBlobName(customerId, originalFileName);
        var escapedBlobName = Uri.EscapeDataString(blobName);

        return Task.FromResult(new ProfilePictureUploadTarget(
            new Uri($"https://local.blob-storage.invalid/upload/{escapedBlobName}"),
            new Uri($"https://local.blob-storage.invalid/{escapedBlobName}"),
            blobName,
            expiresAt,
            HttpMethod.Put.Method,
            RequiredHeaders));
    }

    private static string BuildBlobName(Guid customerId, string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        return $"customers/{customerId:D}/profile-pictures/{Guid.NewGuid():N}{extension}";
    }
}
