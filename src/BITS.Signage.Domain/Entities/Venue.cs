namespace BITS.Signage.Domain.Entities;

/// <summary>
/// Represents a physical location (venue) within a tenant that displays content on devices.
/// </summary>
public class Venue
{
    public string VenueId { get; set; } = null!;
    public string TenantId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Timezone { get; set; } = "UTC";
    public string? Address { get; set; }
    public string? FallbackPlaylistId { get; set; } // Points to a published playlist
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public DateTime? DeletedAtUtc { get; set; }

    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public ICollection<Device> Devices { get; set; } = [];
    public ICollection<DeviceGroup> DeviceGroups { get; set; } = [];
    public ICollection<VenueUserRole> UserRoles { get; set; } = [];
}
