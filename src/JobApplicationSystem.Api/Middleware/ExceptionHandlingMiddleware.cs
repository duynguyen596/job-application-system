using System;
using System.Net;
using System.Text.Json;
using JobApplicationSystem.Application.Exceptions;
using FluentValidation; 

namespace JobApplicationSystem.Api.Middleware;

/// <summary>
/// Middleware for handling exceptions globally, logging them, and returning standardized JSON error responses.
/// </summary>
public class ExceptionHandlingMiddleware : IMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred processing request {RequestMethod} {RequestPath}",
                context.Request.Method, context.Request.Path);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        HttpStatusCode statusCode = HttpStatusCode.InternalServerError; // Default to 500
        string message = "An internal server error has occurred.";
        object? details = null;

        switch (exception)
        {
            case NotFoundException notFoundEx:
                statusCode = HttpStatusCode.NotFound;
                message = notFoundEx.Message;
                break;

            case ValidationAppException appValidationEx: 
                statusCode = HttpStatusCode.BadRequest;
                message = appValidationEx.Message;
               
                // details = appValidationEx.Errors;
                break;

            case ValidationException fluentValidationEx: 
                statusCode = HttpStatusCode.BadRequest;
                message = "One or more validation errors occurred.";
                details = fluentValidationEx.Errors
                    .GroupBy(e => e.PropertyName, StringComparer.OrdinalIgnoreCase) 
                    .ToDictionary(
                        g => char.ToLowerInvariant(g.Key[0]) + g.Key.Substring(1), 
                        g => g.Select(e => e.ErrorMessage).ToArray() 
                    );
                break;

            case DuplicateApplicationException duplicateEx:
                statusCode = HttpStatusCode.Conflict; // 409 Conflict is appropriate for duplicates
                message = duplicateEx.Message;
                break;

            case UnauthorizedAccessException unauthorizedEx:
                // This usually maps to 403 Forbidden if authentication was successful but authorization failed.
                // If authentication itself failed, the auth middleware would typically return 401.
                statusCode = HttpStatusCode.Forbidden;
                message = "Access denied."; // Or use exception.Message if more specific info is safe
                break;

            // Add cases for other specific custom exceptions you define

            default:
                // For any other unhandled exception, keep InternalServerError
                // Optionally hide details in non-Development environments
                // if (!env.IsDevelopment()) { message = "An internal server error occurred."; }
                break;
        }

        context.Response.StatusCode = (int)statusCode;

        // Create the response object
        var errorResponse = new
        {
            StatusCode = (int)statusCode,
            Message = message,
            // Only include details if they are not null (e.g., for validation errors)
            Details = details
        };

        // Serialize the response object to JSON and write it to the response body
        var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // Use camelCase for JSON properties
            DefaultIgnoreCondition =
                System.Text.Json.Serialization.JsonIgnoreCondition
                    .WhenWritingNull // Don't include null properties (like 'details' if it's null)
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}