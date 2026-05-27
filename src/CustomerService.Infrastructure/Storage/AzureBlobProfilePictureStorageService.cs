using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using CustomerService.Application.Abstractions;
using CustomerService.Application.Customers.ProfilePictureUploadUrl;
using Microsoft.Extensions.Options;

namespace CustomerService.Infrastructure.Storage;

public sealed class AzureBlobProfilePictureStorageService(
    BlobContainerClient container,
    IOptions<AzureBlobStorageOptions> options)
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
        var blob = container.GetBlobClient(blobName);

        if (!blob.CanGenerateSasUri)
        {
            throw new InvalidOperationException(
                "Azure Blob Storage client cannot generate SAS URLs with the configured credentials.");
        }

        var sas = new BlobSasBuilder(BlobSasPermissions.Create | BlobSasPermissions.Write, expiresAt)
        {
            BlobContainerName = container.Name,
            BlobName = blobName
        };

        return Task.FromResult(new ProfilePictureUploadTarget(
            blob.GenerateSasUri(sas),
            blob.Uri,
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
