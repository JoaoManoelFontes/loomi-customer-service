using CustomerService.Application.Abstractions;
using CustomerService.Application.Common.Exceptions;
using FluentValidation;

namespace CustomerService.Application.Customers.ProfilePictureUploadUrl;

public sealed class CreateProfilePictureUploadUrlHandler(
    ICustomerRepository customers,
    IProfilePictureStorageService profilePictureStorage,
    IValidator<CreateProfilePictureUploadUrlRequest> validator)
{
    public async Task<CreateProfilePictureUploadUrlResponse> HandleAsync(
        Guid customerId,
        CreateProfilePictureUploadUrlRequest request,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        if (!await customers.ExistsAsync(customerId, cancellationToken))
        {
            throw new CustomerNotFoundException(customerId);
        }

        var uploadTarget = await profilePictureStorage.CreateUploadUrlAsync(
            customerId,
            request.FileName,
            request.ContentType,
            request.FileSizeInBytes,
            cancellationToken);

        return new CreateProfilePictureUploadUrlResponse(
            uploadTarget.UploadUrl,
            uploadTarget.BlobUrl,
            uploadTarget.BlobName,
            uploadTarget.ExpiresAt,
            uploadTarget.HttpMethod,
            uploadTarget.RequiredHeaders);
    }
}
