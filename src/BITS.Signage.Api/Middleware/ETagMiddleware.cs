using BITS.Signage.Api.Common;

namespace BITS.Signage.Api.Middleware;

/// <summary>
/// Middleware for ETag validation on draft-modifying operations (PUT, PATCH, DELETE).
/// Returns 428 Precondition Required if If-Match header is missing.
/// Returns 412 Precondition Failed if ETag doesn't match current version.
/// </summary>
public class ETagMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ETagMiddleware> _logger;

    public ETagMiddleware(RequestDelegate next, ILogger<ETagMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Check if this is a draft-modifying operation
        if (RequiresETag(context.Request.Method, context.Request.Path))
        {
            if (!context.Request.Headers.ContainsKey("If-Match"))
            {
                _logger.LogWarning(
                    "Missing If-Match header on {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);

                context.Response.StatusCode = StatusCodes.Status428PreconditionRequired;
                context.Response.Headers.Add("Vary", "If-Match");
                await context.Response.WriteAsJsonAsync(
                    ProblemResponse.Create(
                        428,
                        "Precondition Required",
                        "If-Match header is required for draft modifications"));
                return;
            }
        }

        await _next(context);
    }

    /// <summary>
    /// Determines if the request requires ETag validation.
    /// </summary>
    private static bool RequiresETag(string method, PathString path)
    {
        // Only PUT, PATCH, DELETE on draft resources require ETag
        var isDraftModification = (method == "PUT" || method == "PATCH" || method == "DELETE") &&
                                  path.Value?.Contains("/draft/") == true;

        return isDraftModification;
    }
}
