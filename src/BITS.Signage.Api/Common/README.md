# Phase 0.4 - API Foundation & Middleware

This folder contains the foundational cross-cutting concerns for the API, including authentication, authorization, error handling, rate limiting, and utility helpers.

## Middleware Pipeline

Middleware is registered in `Program.cs` in a specific order (critical):

```csharp
app.UseMiddleware<ErrorHandlingMiddleware>();       // 1. Catch all exceptions
app.UseMiddleware<ETagMiddleware>();                 // 2. Validate preconditions
app.UseAuthentication();                              // 3. Validate JWT tokens
app.UseMiddleware<TenantIsolationMiddleware>();      // 4. Extract tenant/user info
app.UseMiddleware<RateLimitingMiddleware>();         // 5. Rate limit requests
```

## Components

### RequestContext
Request-scoped service that holds tenant, user, and device information extracted from JWT tokens.
Injected into handlers and middleware to enforce tenant isolation.

```csharp
public class RequestContext
{
    public string TenantId { get; set; }              // From JWT 'tid' claim
    public string UserId { get; set; }                // From JWT 'sub' claim
    public string DeviceId { get; set; }              // From JWT 'did' claim (device tokens)
    public bool IsDeviceRequest { get; set; }         // Device vs user request
    public List<string> TenantRoles { get; set; }     // From 't_roles' claim
    public Dictionary<string, List<string>> VenueRoles { get; set; } // From 'v_roles' claim
    public string CorrelationId { get; set; }         // For request tracing
}
```

### ErrorHandlingMiddleware
Catches all unhandled exceptions and returns RFC 7807 Problem Details responses.

**Response:**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404,
  "detail": "Entity not found",
  "instance": "/v1/playlists/123",
  "traceId": "0HN03UFDF1L9G:00000001"
}
```

### TenantIsolationMiddleware
Extracts tenant, user, and device IDs from JWT tokens and enforces tenant isolation.

**JWT Claims:**
- `tid`: Tenant ID
- `sub`: User ID (for user tokens) or Device ID (for device tokens)
- `did`: Device ID (for device tokens)
- `typ`: Token type ("user" or "device")
- `t_roles`: Tenant roles (comma-separated)
- `v_roles`: Venue roles (JSON: `{"venueId": "role1,role2"}`)

**Behavior:**
- Extracts tenant ID from token
- Validates token for protected endpoints (returns 401 if missing)
- Allows public endpoints: `/health`, `/openapi`, `/.well-known`
- Allows unauthenticated endpoints: `/v1/auth/*`, `/v1/devices/pairing/start`

### ETagMiddleware
Validates ETag preconditions on draft-modifying operations.

**Rules:**
- Applies to: PUT, PATCH, DELETE on paths containing `/draft/`
- Returns 428 Precondition Required if `If-Match` header missing
- Returns 412 Precondition Failed if ETag doesn't match (application responsibility)

**Usage:**
```csharp
// Client sends
PUT /v1/tenants/T1/playlists/draft/P1 HTTP/1.1
If-Match: "version-hash"
```

### RateLimitingMiddleware
Rate limiting using Redis counters with configurable per-tenant, per-user, and per-device limits.

**Limits (per minute):**
- Tenant: 10,000 requests
- User: 1,000 requests
- Device: 500 requests
- Window: 60 seconds (sliding)

**Response Headers:**
```
X-RateLimit-Limit: 1000
X-RateLimit-Remaining: 950
X-RateLimit-Reset: 1644326400
Retry-After: 45
```

**Response (429 Too Many Requests):**
```json
{
  "type": "https://tools.ietf.org/html/rfc6585#section-4",
  "title": "Too Many Requests",
  "status": 429,
  "detail": "User rate limit exceeded. Retry after 45 seconds"
}
```

### ProblemResponse
RFC 7807 Problem Details helper for consistent error responses across the API.

**Usage:**
```csharp
// In middleware or handlers
return ProblemResponse.Create(404, "Not Found", "Playlist not found");

// With extensions
var response = ProblemResponse.Create(400, "Bad Request", "Invalid input");
response.Extensions = new Dictionary<string, object>
{
    ["errors"] = new[] { "Field 'name' is required" }
};
```

### IAuthorizationService
Service for evaluating authorization decisions based on roles.

**Usage:**
```csharp
// In handlers (injected)
[HttpPut("/v1/tenants/{tenantId}/playlists/{playlistId}")]
public async Task<IActionResult> UpdatePlaylist(
    string tenantId,
    string playlistId,
    RequestContext context,
    IAuthorizationService authz)
{
    authz.EnsureTenantIsolation(context, tenantId);
    authz.EnsureVenueRole(context, venueId, "VENUE_EDITOR");
    // ... proceed with update
}
```

### IJwtTokenService
Service for creating user and device tokens.

**Creating a user token:**
```csharp
var userToken = jwtService.CreateUserToken(
    userId: "U1",
    tenantId: "T1",
    tenantRoles: new[] { "TENANT_ADMIN" },
    venueRoles: new Dictionary<string, List<string>>
    {
        ["V1"] = new[] { "VENUE_EDITOR", "VENUE_MANAGER" }
    });
```

**Creating a device token:**
```csharp
var deviceToken = jwtService.CreateDeviceToken("D1", "V1", "T1");
```

**Creating a refresh token:**
```csharp
var refreshToken = jwtService.CreateRefreshToken();
// Store in database, validate on refresh endpoint
```

### PaginationHelper
Helper for cursor-based pagination.

**Query parameters:**
```
GET /v1/playlists?cursor=abc123&limit=50
```

**Usage:**
```csharp
var pagination = PaginationHelper.FromQuery(HttpContext.Request.Query);
// pagination.Cursor = "abc123"
// pagination.Limit = 50 (clamped to MAX_LIMIT of 200)

// Add response headers
pagination.AddResponseHeaders(HttpContext.Response, nextCursor: "xyz789");
```

**Response:**
```
X-Pagination-Limit: 50
X-Pagination-Cursor: xyz789
```

### FilteringHelper
Helper for parsing common filter parameters.

**Query parameters:**
```
GET /v1/assets?q=landscape&status=ACTIVE&scope=VENUE&type=VIDEO
```

**Usage:**
```csharp
var filters = FilteringHelper.FromQuery(HttpContext.Request.Query);
// filters.SearchQuery = "landscape"
// filters.Status = "ACTIVE"
// filters.Scope = "VENUE"
// filters.Type = "VIDEO"
// filters.HasFilters = true
```

## Configuration

### appsettings.Development.json

```json
{
  "Jwt": {
    "SecretKey": "your-secret-key-at-least-32-characters",
    "Issuer": "https://bits-signage.local",
    "Audience": "https://bits-signage.local"
  },
  "ConnectionStrings": {
    "PostgreSql": "Host=localhost;Port=5432;Database=bits_signage;Username=bits_user;Password=bits_password",
    "Redis": "localhost:6379"
  }
}
```

## Best Practices

1. **Always use RequestContext** — Never parse JWT claims directly; use the extracted RequestContext
2. **Use authorization service** — Check roles via IAuthorizationService, not custom logic
3. **Return problem responses** — Always return ProblemResponse for errors
4. **Handle tenant isolation** — Always validate tenant match before returning data
5. **Add correlation IDs** — Use RequestContext.CorrelationId for logging and debugging
6. **Rate limit gracefully** — Respect Retry-After headers in client implementations
7. **Validate ETags** — Include If-Match header on draft modifications
8. **Paginate large lists** — Use PaginationHelper for any list endpoint
9. **Filter consistently** — Use FilteringHelper and standard query parameter names

## Testing

When testing API endpoints:

1. **Include Authorization header:**
   ```
   Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
   ```

2. **Include If-Match for draft modifications:**
   ```
   If-Match: "current-version-hash"
   ```

3. **Check X-RateLimit headers** to monitor quota

4. **Inspect error responses** for ProblemResponse format

## Migration Path

Phase 0.4 establishes the middleware foundation. Subsequent phases will:
- Phase 1.1: Add authentication endpoints (login, refresh, user profile)
- Phase 1.2: Add tenant/venue management endpoints
- Phase 2.1: Add device pairing endpoints with device token issuance
