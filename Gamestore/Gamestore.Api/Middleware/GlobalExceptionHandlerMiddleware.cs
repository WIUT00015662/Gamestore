using System.Net;
using System.Text.Json;
using Gamestore.Domain.Exceptions;

namespace Gamestore.Api.Middleware;

/// <summary>
/// Global exception handler middleware.
/// </summary>
public class GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger = logger;

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    /// <param name="context">HTTP context.</param>
    /// <returns>A task representing the operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var exceptionDetails = $"Type: {ex.GetType().FullName}, Message: {ex.Message}, InnerException: {ex.InnerException?.Message}, StackTrace: {ex.StackTrace}";
            _logger.LogError(ex, "An unhandled exception occurred. Details: {ExceptionDetails}", exceptionDetails);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var statusCode = exception switch
        {
            EntityNotFoundException => HttpStatusCode.NotFound,
            EntityAlreadyExistsException => HttpStatusCode.Conflict,
            ArgumentException => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.InternalServerError,
        };

        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            error = statusCode == HttpStatusCode.InternalServerError ? "An unexpected error occurred. Please try again later." : exception.Message,
            statusCode = (int)statusCode,
        };

        var jsonResponse = JsonSerializer.Serialize(response);
        return context.Response.WriteAsync(jsonResponse);
    }
}
