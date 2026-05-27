using CustomerService.Application.Abstractions;
using CustomerService.Application.Common.Exceptions;
using CustomerService.Application.Customers.ProfilePictureUploadUrl;
using CustomerService.Domain.Entities;
using CustomerService.UnitTests.Application;
using FluentAssertions;

namespace CustomerService.UnitTests.Application.Customers;

public sealed class CreateProfilePictureUploadUrlHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithExistingCustomer_ShouldReturnUploadTarget()
    {
        var customer = new Customer(
            "Maria Silva",
            "maria.silva@example.com",
            "Rua A, 123",
            new BankingDetails("0001", "123456-7", 150.25m));
        var storage = new FakeProfilePictureStorageService();
        var handler = new CreateProfilePictureUploadUrlHandler(
            new FakeCustomerRepository(customer),
            storage,
            new CreateProfilePictureUploadUrlRequestValidator());

        var response = await handler.HandleAsync(
            customer.Id,
            new CreateProfilePictureUploadUrlRequest("profile.png", "image/png", 1024));

        response.UploadUrl.Should().Be(storage.Target.UploadUrl);
        response.BlobUrl.Should().Be(storage.Target.BlobUrl);
        response.BlobName.Should().Be(storage.Target.BlobName);
        storage.CustomerId.Should().Be(customer.Id);
    }

    [Fact]
    public async Task HandleAsync_WithMissingCustomer_ShouldThrowCustomerNotFound()
    {
        var handler = new CreateProfilePictureUploadUrlHandler(
            new FakeCustomerRepository(),
            new FakeProfilePictureStorageService(),
            new CreateProfilePictureUploadUrlRequestValidator());

        var act = () => handler.HandleAsync(
            Guid.NewGuid(),
            new CreateProfilePictureUploadUrlRequest("profile.png", "image/png", 1024));

        await act.Should().ThrowAsync<CustomerNotFoundException>();
    }

    private sealed class FakeProfilePictureStorageService : IProfilePictureStorageService
    {
        public ProfilePictureUploadTarget Target { get; } = new(
            new Uri("https://storage.example.com/upload"),
            new Uri("https://storage.example.com/profile.png"),
            "customers/customer-id/profile-pictures/profile.png",
            DateTimeOffset.UtcNow.AddMinutes(15),
            "PUT",
            new Dictionary<string, string> { ["x-ms-blob-type"] = "BlockBlob" });

        public Guid? CustomerId { get; private set; }

        public Task<ProfilePictureUploadTarget> CreateUploadUrlAsync(
            Guid customerId,
            string originalFileName,
            string contentType,
            long fileSizeInBytes,
            CancellationToken cancellationToken = default)
        {
            CustomerId = customerId;
            return Task.FromResult(Target);
        }
    }
}
