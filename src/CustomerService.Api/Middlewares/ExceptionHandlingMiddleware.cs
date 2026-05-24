using CustomerService.Application.Common.Exceptions;
using CustomerService.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace CustomerService.Api.Middlewares;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var problemDetails = exception switch
        {
            ValidationException validationException => CreateValidationProblem(context, validationException),
            InvalidCredentialsException => CreateProblem(context, StatusCodes.Status401Unauthorized, "Invalid credentials", exception.Message),
            DuplicateCpfException => CreateProblem(context, StatusCodes.Status409Conflict, "Duplicate CPF", exception.Message),
            DomainValidationException => CreateProblem(context, StatusCodes.Status400BadRequest, "Invalid request", exception.Message),
            _ => CreateProblem(context, StatusCodes.Status500InternalServerError, "Unexpected error", "An unexpected error occurred.")
        };

        if (problemDetails.Status == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception while processing request.");
        }

        context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(problemDetails);
    }

    private static ProblemDetails CreateProblem(HttpContext context, int status, string title, string detail)
    {
        return new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };
    }

    private static ValidationProblemDetails CreateValidationProblem(HttpContext context, ValidationException exception)
    {
        var errors = exception.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).ToArray());

        return new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation failed",
            Instance = context.Request.Path
        };
    }
}
