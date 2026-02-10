namespace BITS.Signage.Domain.Entities;

/// <summary>
/// Audit log entry for every publish operation.
/// Tracks who published what and when, for compliance and troubleshooting.
/// Includes optional propagation info for content group auto-propagate scenarios.
/// </summary>
public class PublishEvent
{
    public string PublishEventId { get; set; } = null!;
    public string TenantId { get; set; } = null!;
    public string VenueId { get; set; } = null!;
    public string Type { get; set; } = null!; // PLAYLIST, SCHEDULE, CONTENT_GROUP
    public string EntityId { get; set; } = null!; // playlistId, scheduleSetId, or contentGroupId
    public int Version { get; set; } // Published version number
    public string PublishedByUserId { get; set; } = null!;
    public DateTime PublishedAtUtc { get; set; }

    // For content group auto-propagate: list of playlist IDs affected
    public string? PropagatedPlaylistIds { get; set; } // JSON array of playlist IDs auto-updated

    // Navigation properties
    public User PublishedByUser { get; set; } = null!;
}
