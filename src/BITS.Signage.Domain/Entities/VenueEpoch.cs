namespace BITS.Signage.Domain.Entities;

/// <summary>
/// Tracks the manifest epoch for a venue.
/// When playlists or schedules are published, the epoch increments.
/// Devices poll and compare epochs to know when to fetch updated manifests.
/// </summary>
public class VenueEpoch
{
    public string VenueId { get; set; } = null!;
    public string TenantId { get; set; } = null!;
    public long ManifestEpoch { get; set; } = 0; // Incremented on each publish
    public DateTime UpdatedAtUtc { get; set; }

    // Navigation properties
    public Venue Venue { get; set; } = null!;
}
