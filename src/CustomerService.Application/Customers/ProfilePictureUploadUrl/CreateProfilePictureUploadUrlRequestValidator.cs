using FluentValidation;

namespace CustomerService.Application.Customers.ProfilePictureUploadUrl;

public sealed class CreateProfilePictureUploadUrlRequestValidator
    : AbstractValidator<CreateProfilePictureUploadUrlRequest>
{
    public const long MaxFileSizeInBytes = 5 * 1024 * 1024;

    private static readonly string[] AllowedContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];

    private static readonly string[] AllowedExtensions =
    [
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    ];

    public CreateProfilePictureUploadUrlRequestValidator()
    {
        RuleFor(request => request.FileName)
            .NotEmpty()
            .MaximumLength(255)
            .Must(HasAllowedExtension)
            .WithMessage("File name must end with .jpg, .jpeg, .png, or .webp.");

        RuleFor(request => request.ContentType)
            .NotEmpty()
            .Must(contentType => AllowedContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Content type must be image/jpeg, image/png, or image/webp.");

        RuleFor(request => request.FileSizeInBytes)
            .GreaterThan(0)
            .LessThanOrEqualTo(MaxFileSizeInBytes)
            .WithMessage($"File size must be between 1 byte and {MaxFileSizeInBytes} bytes.");
    }

    private static bool HasAllowedExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        return AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
    }
}
