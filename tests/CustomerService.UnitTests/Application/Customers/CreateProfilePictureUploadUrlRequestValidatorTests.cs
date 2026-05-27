using CustomerService.Application.Customers.ProfilePictureUploadUrl;
using FluentAssertions;

namespace CustomerService.UnitTests.Application.Customers;

public sealed class CreateProfilePictureUploadUrlRequestValidatorTests
{
    private readonly CreateProfilePictureUploadUrlRequestValidator _validator = new();

    [Theory]
    [InlineData("profile.jpg", "image/jpeg")]
    [InlineData("profile.jpeg", "image/jpeg")]
    [InlineData("profile.png", "image/png")]
    [InlineData("profile.webp", "image/webp")]
    public void Validate_WithSupportedImageMetadata_ShouldBeValid(string fileName, string contentType)
    {
        var result = _validator.Validate(new CreateProfilePictureUploadUrlRequest(
            fileName,
            contentType,
            1024));

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "image/png", 1024)]
    [InlineData("profile.gif", "image/gif", 1024)]
    [InlineData("profile.png", "application/octet-stream", 1024)]
    [InlineData("profile.png", "image/png", 0)]
    [InlineData("profile.png", "image/png", CreateProfilePictureUploadUrlRequestValidator.MaxFileSizeInBytes + 1)]
    public void Validate_WithInvalidImageMetadata_ShouldBeInvalid(
        string fileName,
        string contentType,
        long fileSizeInBytes)
    {
        var result = _validator.Validate(new CreateProfilePictureUploadUrlRequest(
            fileName,
            contentType,
            fileSizeInBytes));

        result.IsValid.Should().BeFalse();
    }
}
