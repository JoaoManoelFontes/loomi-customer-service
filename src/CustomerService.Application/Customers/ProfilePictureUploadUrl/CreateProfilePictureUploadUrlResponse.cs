namespace CustomerService.Application.Customers.ProfilePictureUploadUrl;

public sealed record CreateProfilePictureUploadUrlResponse(
    Uri UploadUrl,
    Uri BlobUrl,
    string BlobName,
    DateTimeOffset ExpiresAt,
    string HttpMethod,
    IReadOnlyDictionary<string, string> RequiredHeaders);
