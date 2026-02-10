using BITS.Signage.Api.Common;

namespace BITS.Signage.Api.Middleware;

/// <summary>
/// Global error handling middleware that catches all exceptions and returns RFC 7807 Problem Details responses.
/// Logs errors for debugging and provides consistent error format to clients.
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestContext requestContext)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception, requestContext);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception, RequestContext requestContext)
    {
        context.Response.ContentType = "application/problem+json";

        var response = exception switch
        {
            UnauthorizedAccessException => ProblemResponse.Create(
                401, "Unauthorized", exception.Message),

            KeyNotFoundException => ProblemResponse.Create(
                404, "Not Found", exception.Message),

            ArgumentException => ProblemResponse.Create(
                400, "Bad Request", exception.Message),

            InvalidOperationException => ProblemResponse.Create(
                409, "Conflict", exception.Message),

            _ => ProblemResponse.Create(
                500, "Internal Server Error", "An unexpected error occurred")
        };

        response.Instance = context.Request.Path;
        response.TraceId = requestContext.CorrelationId;

        context.Response.StatusCode = response.Status;
        return context.Response.WriteAsJsonAsync(response);
    }
}
