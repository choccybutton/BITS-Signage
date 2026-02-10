namespace BITS.Signage.Domain.Entities;

/// <summary>
/// Represents a grouping of devices within a venue for batch operations and group-level settings.
/// </summary>
public class DeviceGroup
{
    public string GroupId { get; set; } = null!;
    public string TenantId { get; set; } = null!;
    public string VenueId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    // Navigation properties
    public Venue Venue { get; set; } = null!;
    public ICollection<Device> Devices { get; set; } = [];
}
