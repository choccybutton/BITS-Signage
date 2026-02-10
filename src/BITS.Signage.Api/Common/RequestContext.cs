namespace BITS.Signage.Api.Common;

/// <summary>
/// Request-scoped context that stores tenant and user information extracted from JWT tokens.
/// Injected into handlers to enforce tenant isolation.
/// </summary>
public class RequestContext
{
    public string? TenantId { get; set; }
    public string? UserId { get; set; }
    public string? DeviceId { get; set; }
    public bool IsDeviceRequest { get; set; }
    public List<string> TenantRoles { get; set; } = [];
    public Dictionary<string, List<string>> VenueRoles { get; set; } = new(); // VenueId -> List of roles
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Returns true if user/device is authenticated.
    /// </summary>
    public bool IsAuthenticated => !string.IsNullOrEmpty(TenantId) && (!string.IsNullOrEmpty(UserId) || IsDeviceRequest);

    /// <summary>
    /// Returns true if user has any tenant-level role.
    /// </summary>
    public bool IsTenantAdmin => TenantRoles.Contains("TENANT_ADMIN");

    /// <summary>
    /// Returns true if user has a specific venue role.
    /// </summary>
    public bool HasVenueRole(string venueId, string role) =>
        VenueRoles.TryGetValue(venueId, out var roles) && roles.Contains(role);
}
