using CustomerService.Application.Customers.UpdateBalance;
using FluentAssertions;

namespace CustomerService.UnitTests.Application.Customers;

public sealed class UpdateBalanceRequestValidatorTests
{
    private readonly UpdateBalanceRequestValidator _validator = new();

    [Fact]
    public void Validate_WithValidRequest_ShouldPass()
    {
        var result = _validator.Validate(new UpdateBalanceRequest(Guid.NewGuid(), 10m));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyReceiverId_ShouldFail()
    {
        var result = _validator.Validate(new UpdateBalanceRequest(Guid.Empty, 10m));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(UpdateBalanceRequest.ReceiverId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithNonPositiveAmount_ShouldFail(decimal amount)
    {
        var result = _validator.Validate(new UpdateBalanceRequest(Guid.NewGuid(), amount));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(UpdateBalanceRequest.Amount));
    }
}
