namespace BITS.Signage.Domain.Entities;

/// <summary>
/// Tracks which user is currently editing a specific resource (playlist, schedule, content group).
/// Used to prevent concurrent editing and notify other users of changes.
/// Expires automatically after inactivity (WebSocket disconnect or timeout).
/// </summary>
public class EditorPresence
{
    public string PresenceId { get; set; } = null!;
    public string TenantId { get; set; } = null!;
    public string ResourceType { get; set; } = null!; // PLAYLIST, SCHEDULE, CONTENT_GROUP
    public string ResourceId { get; set; } = null!; // playlistId, scheduleSetId, or contentGroupId
    public string UserId { get; set; } = null!;
    public DateTime ExpiresAtUtc { get; set; } // Auto-expires (heartbeat keeps extending)

    // Navigation properties
    public User User { get; set; } = null!;
}
