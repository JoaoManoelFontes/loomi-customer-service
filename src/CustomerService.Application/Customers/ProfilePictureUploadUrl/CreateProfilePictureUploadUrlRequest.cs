namespace CustomerService.Application.Customers.ProfilePictureUploadUrl;

public sealed record CreateProfilePictureUploadUrlRequest(
    string FileName,
    string ContentType,
    long FileSizeInBytes);
