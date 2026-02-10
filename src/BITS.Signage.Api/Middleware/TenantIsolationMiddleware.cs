using System.IdentityModel.Tokens.Jwt;
using BITS.Signage.Api.Common;

namespace BITS.Signage.Api.Middleware;

/// <summary>
/// Extracts tenant ID from JWT token claims and injects into RequestContext.
/// Ensures tenant isolation by validating all requests have a valid tenant claim.
/// </summary>
public class TenantIsolationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantIsolationMiddleware> _logger;

    public TenantIsolationMiddleware(RequestDelegate next, ILogger<TenantIsolationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestContext requestContext)
    {
        // Extract correlation ID for tracing
        requestContext.CorrelationId = context.Request.Headers.TryGetValue("X-Request-Id", out var correlationId)
            ? correlationId.ToString()
            : context.TraceIdentifier;

        // Skip tenant extraction for health checks and public endpoints
        if (IsPublicEndpoint(context.Request.Path))
        {
            await _next(context);
            return;
        }

        // Extract tenant ID from JWT token
        var tenantId = ExtractTenantIdFromToken(context);
        var userId = ExtractUserIdFromToken(context);
        var deviceId = ExtractDeviceIdFromToken(context);

        if (string.IsNullOrEmpty(tenantId))
        {
            _logger.LogWarning(
                "Request without valid tenant claim: Path={Path}, TraceId={TraceId}",
                context.Request.Path,
                requestContext.CorrelationId);

            // For unauthenticated endpoints, continue; for protected, return 401
            if (!IsUnauthenticatedEndpoint(context.Request.Path))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(
                    ProblemResponse.Create(401, "Unauthorized", "Missing or invalid tenant information"));
                return;
            }
        }
        else
        {
            requestContext.TenantId = tenantId;
            requestContext.UserId = userId;
            requestContext.DeviceId = deviceId;
            requestContext.IsDeviceRequest = !string.IsNullOrEmpty(deviceId) && string.IsNullOrEmpty(userId);

            // Extract roles from JWT
            ExtractRolesFromToken(context, requestContext);
        }

        await _next(context);
    }

    /// <summary>
    /// Extracts tenant ID from JWT 'tid' claim.
    /// </summary>
    private static string? ExtractTenantIdFromToken(HttpContext context)
    {
        var principal = context.User;
        if (principal?.Identity?.IsAuthenticated != true)
            return null;

        return principal.FindFirst("tid")?.Value ?? principal.FindFirst("sub")?.Value;
    }

    /// <summary>
    /// Extracts user ID from JWT 'sub' claim.
    /// </summary>
    private static string? ExtractUserIdFromToken(HttpContext context)
    {
        var principal = context.User;
        return principal?.FindFirst("sub")?.Value;
    }

    /// <summary>
    /// Extracts device ID from JWT 'did' claim (for device tokens).
    /// </summary>
    private static string? ExtractDeviceIdFromToken(HttpContext context)
    {
        var principal = context.User;
        return principal?.FindFirst("did")?.Value;
    }

    /// <summary>
    /// Extracts tenant and venue roles from JWT claims.
    /// </summary>
    private static void ExtractRolesFromToken(HttpContext context, RequestContext requestContext)
    {
        var principal = context.User;

        // Tenant-level roles (t_roles claim)
        var tenantRoles = principal?.FindFirst("t_roles")?.Value;
        if (!string.IsNullOrEmpty(tenantRoles))
        {
            requestContext.TenantRoles = tenantRoles.Split(',').ToList();
        }

        // Venue-level roles (v_roles claim, JSON format: {"venueId": "role1,role2"})
        var venueRoles = principal?.FindFirst("v_roles")?.Value;
        if (!string.IsNullOrEmpty(venueRoles))
        {
            try
            {
                var roles = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(venueRoles);
                if (roles != null)
                {
                    foreach (var kvp in roles)
                    {
                        requestContext.VenueRoles[kvp.Key] = kvp.Value.Split(',').ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log malformed roles but don't fail the request
                // (roles simply won't be populated)
            }
        }
    }

    /// <summary>
    /// Checks if endpoint is public (doesn't require authentication).
    /// </summary>
    private static bool IsPublicEndpoint(PathString path)
    {
        var pathValue = path.Value?.ToLower() ?? "";
        return pathValue.StartsWith("/health") ||
               pathValue.StartsWith("/openapi") ||
               pathValue.StartsWith("/.well-known");
    }

    /// <summary>
    /// Checks if endpoint allows unauthenticated access.
    /// </summary>
    private static bool IsUnauthenticatedEndpoint(PathString path)
    {
        var pathValue = path.Value?.ToLower() ?? "";
        return pathValue.StartsWith("/v1/auth/login") ||
               pathValue.StartsWith("/v1/auth/refresh") ||
               pathValue.StartsWith("/v1/devices/pairing/start");
    }
}
