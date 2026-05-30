using System.Net;
using System.Text.Json;
using Yeminus.Domain.Exceptions;

namespace Yeminus.Api.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message, errors) = exception switch
        {
            NotFoundException => (HttpStatusCode.NotFound, exception.Message, Array.Empty<string>()),
            DomainValidationException ve => (HttpStatusCode.BadRequest, "Validation failed.", ve.Errors.ToArray()),
            UnauthorizedException => (HttpStatusCode.Unauthorized, exception.Message, Array.Empty<string>()),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.", Array.Empty<string>())
        };

        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            message,
            errors
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
}
