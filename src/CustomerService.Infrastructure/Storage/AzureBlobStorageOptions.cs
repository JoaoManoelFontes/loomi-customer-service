namespace CustomerService.Infrastructure.Storage;

public sealed class AzureBlobStorageOptions
{
    public const string SectionName = "AzureBlobStorage";

    public string ConnectionString { get; init; } = string.Empty;

    public string ContainerName { get; init; } = "customer-profile-pictures";

    public int UploadUrlExpiresInMinutes { get; init; } = 15;
}
