namespace BITS.Signage.Domain.Entities;

/// <summary>
/// Represents a role assignment of a user within a specific venue (e.g., VENUE_MANAGER, VENUE_EDITOR, VENUE_VIEWER).
/// </summary>
public class VenueUserRole
{
    public string VenueUserRoleId { get; set; } = null!;
    public string TenantId { get; set; } = null!;
    public string VenueId { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public string Role { get; set; } = null!; // VENUE_MANAGER, VENUE_EDITOR, VENUE_VIEWER
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    // Navigation properties
    public Venue Venue { get; set; } = null!;
    public User User { get; set; } = null!;
}
