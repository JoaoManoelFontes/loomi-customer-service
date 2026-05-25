using Asp.Versioning;
using CustomerService.Application.Customers.GetCustomerDetails;
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
    GetCustomerDetailsHandler getCustomerDetailsHandler,
    UpdateCustomerHandler updateCustomerHandler) : ControllerBase
{
    [HttpGet("{customerId:guid}")]
    [ProducesResponseType(typeof(CustomerDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerDetailsResponse>> GetById(
        Guid customerId,
        CancellationToken cancellationToken)
    {
        var response = await getCustomerDetailsHandler.HandleAsync(customerId, cancellationToken);
        return Ok(response);
    }

    [HttpPatch("{customerId:guid}")]
    [ProducesResponseType(typeof(UpdateCustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UpdateCustomerResponse>> Update(
        Guid customerId,
        [FromBody] UpdateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var response = await updateCustomerHandler.HandleAsync(customerId, request, cancellationToken);
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
}
