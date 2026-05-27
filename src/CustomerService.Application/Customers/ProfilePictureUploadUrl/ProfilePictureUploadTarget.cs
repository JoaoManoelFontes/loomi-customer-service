namespace CustomerService.Application.Customers.ProfilePictureUploadUrl;

public sealed record ProfilePictureUploadTarget(
    Uri UploadUrl,
    Uri BlobUrl,
    string BlobName,
    DateTimeOffset ExpiresAt,
    string HttpMethod,
    IReadOnlyDictionary<string, string> RequiredHeaders);
