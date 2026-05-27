using Asp.Versioning;
using CustomerService.Application.Customers.Exists;
using CustomerService.Application.Customers.GetCustomerDetails;
using CustomerService.Application.Customers.ProfilePictureUploadUrl;
using CustomerService.Application.Customers.UpdateBalance;
using CustomerService.Application.Customers.UpdateCustomer;
using CustomerService.Application.Users.CreateUser;
using CustomerService.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerService.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/customers")]
public sealed class CustomerController(
    CreateUserHandler createUserHandler,
    CustomerExistsHandler customerExistsHandler,
    GetCustomerDetailsHandler getCustomerDetailsHandler,
    CreateProfilePictureUploadUrlHandler createProfilePictureUploadUrlHandler,
    UpdateBalanceHandler updateBalanceHandler,
    UpdateCustomerHandler updateCustomerHandler) : ControllerBase
{
    private const string CustomerIdClaimType = "customer_id";

    [Authorize(Roles = nameof(UserRole.Customer))]
    [HttpGet]
    [ProducesResponseType(typeof(CustomerDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerDetailsResponse>> GetCurrent(
        CancellationToken cancellationToken)
    {
        if (!TryGetAuthenticatedCustomerId(out var customerId))
        {
            return Forbid();
        }

        var response = await getCustomerDetailsHandler.HandleAsync(customerId, cancellationToken);
        return Ok(response);
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpGet("{customerId:guid}")]
    [ProducesResponseType(typeof(CustomerDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerDetailsResponse>> GetById(
        Guid customerId,
        CancellationToken cancellationToken)
    {
        var response = await getCustomerDetailsHandler.HandleAsync(customerId, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{customerId:guid}/exists")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<bool>> Exists(
        Guid customerId,
        CancellationToken cancellationToken)
    {
        var exists = await customerExistsHandler.HandleAsync(customerId, cancellationToken);
        return Ok(exists);
    }

    [Authorize(Roles = nameof(UserRole.Customer))]
    [HttpPatch]
    [ProducesResponseType(typeof(UpdateCustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UpdateCustomerResponse>> UpdateCurrent(
        [FromBody] UpdateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetAuthenticatedCustomerId(out var customerId))
        {
            return Forbid();
        }

        var response = await updateCustomerHandler.HandleAsync(customerId, request, cancellationToken);
        return Ok(response);
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPatch("{customerId:guid}")]
    [ProducesResponseType(typeof(UpdateCustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UpdateCustomerResponse>> Update(
        Guid customerId,
        [FromBody] UpdateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var response = await updateCustomerHandler.HandleAsync(customerId, request, cancellationToken);
        return Ok(response);
    }

    [Authorize(Roles = nameof(UserRole.Customer))]
    [HttpPost("profile-picture/upload-url")]
    [ProducesResponseType(typeof(CreateProfilePictureUploadUrlResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CreateProfilePictureUploadUrlResponse>> CreateProfilePictureUploadUrl(
        [FromBody] CreateProfilePictureUploadUrlRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetAuthenticatedCustomerId(out var customerId))
        {
            return Forbid();
        }

        var response = await createProfilePictureUploadUrlHandler.HandleAsync(
            customerId,
            request,
            cancellationToken);

        return Ok(response);
    }

    [Authorize(Roles = nameof(UserRole.Customer))]
    [HttpPost("update-balance")]
    [ProducesResponseType(typeof(UpdateBalanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UpdateBalanceResponse>> UpdateBalance(
        [FromBody] UpdateBalanceRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetAuthenticatedCustomerId(out var senderId))
        {
            return Forbid();
        }

        var response = await updateBalanceHandler.HandleAsync(senderId, request, cancellationToken);
        return Ok(response);
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPost]
    [ProducesResponseType(typeof(CreateUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreateUserResponse>> Create(
        [FromBody] CreateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var response = await createUserHandler.HandleAsync(request, cancellationToken);
        return Created($"/api/v1/customers/{response.Customer.Id}", response);
    }

    private bool TryGetAuthenticatedCustomerId(out Guid customerId)
    {
        var value = User.FindFirst(CustomerIdClaimType)?.Value;
        return Guid.TryParse(value, out customerId);
    }
}
