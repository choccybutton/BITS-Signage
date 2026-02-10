using BITS.Signage.Api.Common;

namespace BITS.Signage.Api.Services;

/// <summary>
/// Service for evaluating authorization decisions based on tenant and venue roles.
/// </summary>
public interface IAuthorizationService
{
    /// <summary>
    /// Checks if user has a specific tenant-level role.
    /// </summary>
    bool HasTenantRole(RequestContext context, string role);

    /// <summary>
    /// Checks if user has a specific venue-level role.
    /// </summary>
    bool HasVenueRole(RequestContext context, string venueId, string role);

    /// <summary>
    /// Checks if user has any of the specified roles.
    /// </summary>
    bool HasAnyRole(RequestContext context, params string[] roles);

    /// <summary>
    /// Ensures user has a specific tenant role (throws on failure).
    /// </summary>
    void EnsureTenantRole(RequestContext context, string role);

    /// <summary>
    /// Ensures user has a specific venue role (throws on failure).
    /// </summary>
    void EnsureVenueRole(RequestContext context, string venueId, string role);

    /// <summary>
    /// Ensures user is the same tenant (throws on failure).
    /// </summary>
    void EnsureTenantIsolation(RequestContext context, string targetTenantId);

    /// <summary>
    /// Ensures user has access to the venue (throws on failure).
    /// </summary>
    void EnsureVenueAccess(RequestContext context, string venueId);
}

public class AuthorizationService : IAuthorizationService
{
    public bool HasTenantRole(RequestContext context, string role)
    {
        return context.IsAuthenticated && context.TenantRoles.Contains(role);
    }

    public bool HasVenueRole(RequestContext context, string venueId, string role)
    {
        return context.IsAuthenticated && context.HasVenueRole(venueId, role);
    }

    public bool HasAnyRole(RequestContext context, params string[] roles)
    {
        if (!context.IsAuthenticated)
            return false;

        foreach (var role in roles)
        {
            if (context.TenantRoles.Contains(role))
                return true;

            foreach (var venueRoles in context.VenueRoles.Values)
            {
                if (venueRoles.Contains(role))
                    return true;
            }
        }

        return false;
    }

    public void EnsureTenantRole(RequestContext context, string role)
    {
        if (!HasTenantRole(context, role))
            throw new UnauthorizedAccessException($"User does not have required tenant role: {role}");
    }

    public void EnsureVenueRole(RequestContext context, string venueId, string role)
    {
        if (!HasVenueRole(context, venueId, role))
            throw new UnauthorizedAccessException($"User does not have required venue role: {role} for venue {venueId}");
    }

    public void EnsureTenantIsolation(RequestContext context, string targetTenantId)
    {
        if (string.IsNullOrEmpty(context.TenantId) || context.TenantId != targetTenantId)
            throw new UnauthorizedAccessException("Cross-tenant access denied");
    }

    public void EnsureVenueAccess(RequestContext context, string venueId)
    {
        // User can access venue if they have any role in it
        if (!context.VenueRoles.ContainsKey(venueId))
            throw new UnauthorizedAccessException($"User does not have access to venue {venueId}");
    }
}
