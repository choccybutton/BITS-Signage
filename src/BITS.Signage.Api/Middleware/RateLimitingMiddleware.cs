using BITS.Signage.Api.Common;
using StackExchange.Redis;

namespace BITS.Signage.Api.Middleware;

/// <summary>
/// Rate limiting middleware using Redis counters.
/// Implements per-tenant, per-user, and per-device rate limits.
/// Returns 429 Too Many Requests with Retry-After header when limit exceeded.
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly IConnectionMultiplexer _redis;

    // Rate limits: requests per minute
    private const int TENANT_LIMIT = 10000;
    private const int USER_LIMIT = 1000;
    private const int DEVICE_LIMIT = 500;
    private const int WINDOW_SECONDS = 60;

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger, IConnectionMultiplexer redis)
    {
        _next = next;
        _logger = logger;
        _redis = redis;
    }

    public async Task InvokeAsync(HttpContext context, RequestContext requestContext)
    {
        // Skip rate limiting for health checks
        if (context.Request.Path.Value?.StartsWith("/health") == true)
        {
            await _next(context);
            return;
        }

        // Skip if not authenticated
        if (!requestContext.IsAuthenticated)
        {
            await _next(context);
            return;
        }

        var db = _redis.GetDatabase();

        try
        {
            // Check tenant limit
            if (!string.IsNullOrEmpty(requestContext.TenantId))
            {
                var (allowed, remaining, retryAfter) = await CheckRateLimit(
                    db,
                    $"ratelimit:tenant:{requestContext.TenantId}",
                    TENANT_LIMIT);

                if (!allowed)
                {
                    _logger.LogWarning(
                        "Tenant {TenantId} rate limit exceeded. Retry-After: {RetryAfter}",
                        requestContext.TenantId,
                        retryAfter);

                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.Response.Headers.Add("Retry-After", retryAfter.ToString());
                    context.Response.Headers.Add("X-RateLimit-Limit", TENANT_LIMIT.ToString());
                    context.Response.Headers.Add("X-RateLimit-Remaining", "0");
                    context.Response.Headers.Add("X-RateLimit-Reset", DateTimeOffset.UtcNow.AddSeconds(retryAfter).ToUnixTimeSeconds().ToString());

                    await context.Response.WriteAsJsonAsync(
                        ProblemResponse.Create(429, "Too Many Requests", $"Tenant rate limit exceeded. Retry after {retryAfter} seconds"));
                    return;
                }

                context.Response.Headers.Add("X-RateLimit-Remaining", remaining.ToString());
            }

            // Check user/device limit
            if (!string.IsNullOrEmpty(requestContext.UserId))
            {
                var (allowed, remaining, retryAfter) = await CheckRateLimit(
                    db,
                    $"ratelimit:user:{requestContext.UserId}",
                    USER_LIMIT);

                if (!allowed)
                {
                    _logger.LogWarning(
                        "User {UserId} rate limit exceeded. Retry-After: {RetryAfter}",
                        requestContext.UserId,
                        retryAfter);

                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.Response.Headers.Add("Retry-After", retryAfter.ToString());
                    await context.Response.WriteAsJsonAsync(
                        ProblemResponse.Create(429, "Too Many Requests", $"User rate limit exceeded. Retry after {retryAfter} seconds"));
                    return;
                }
            }
            else if (!string.IsNullOrEmpty(requestContext.DeviceId))
            {
                var (allowed, remaining, retryAfter) = await CheckRateLimit(
                    db,
                    $"ratelimit:device:{requestContext.DeviceId}",
                    DEVICE_LIMIT);

                if (!allowed)
                {
                    _logger.LogWarning(
                        "Device {DeviceId} rate limit exceeded. Retry-After: {RetryAfter}",
                        requestContext.DeviceId,
                        retryAfter);

                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.Response.Headers.Add("Retry-After", retryAfter.ToString());
                    await context.Response.WriteAsJsonAsync(
                        ProblemResponse.Create(429, "Too Many Requests", $"Device rate limit exceeded. Retry after {retryAfter} seconds"));
                    return;
                }
            }

            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking rate limit");
            // Continue on Redis error (fail open)
            await _next(context);
        }
    }

    /// <summary>
    /// Checks if a request is within rate limit.
    /// Returns (allowed, remaining_in_window, seconds_until_reset).
    /// </summary>
    private static async Task<(bool Allowed, long Remaining, int RetryAfter)> CheckRateLimit(
        IDatabase db,
        string key,
        long limit)
    {
        var current = await db.StringIncrementAsync(key);
        var ttl = await db.KeyTimeToLiveAsync(key);

        // Set expiration on first request in window
        if (current == 1)
        {
            await db.KeyExpireAsync(key, TimeSpan.FromSeconds(WINDOW_SECONDS));
        }

        if (current > limit)
        {
            var retryAfter = (int)(ttl?.TotalSeconds ?? WINDOW_SECONDS);
            return (false, 0, retryAfter);
        }

        return (true, limit - current, 0);
    }
}
