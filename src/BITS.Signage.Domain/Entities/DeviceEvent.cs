namespace BITS.Signage.Domain.Entities;

/// <summary>
/// Represents an event logged by a device (playback start, error, crash, etc).
/// Used for diagnostics, analytics, and debugging.
/// </summary>
public class DeviceEvent
{
    public string DeviceEventId { get; set; } = null!;
    public string DeviceId { get; set; } = null!;
    public string EventType { get; set; } = null!; // PLAYBACK_START, PLAYBACK_END, ERROR, CRASH, MANIFEST_UPDATE, ASSET_DOWNLOADED, etc
    public DateTime EventTimeUtc { get; set; } // When the event occurred on the device
    public string? PayloadJson { get; set; } // Structured event data (JSON)
    public string Severity { get; set; } = "INFO"; // INFO, WARNING, ERROR, CRITICAL
    public DateTime CreatedAtUtc { get; set; } // When we received the event

    // Navigation properties
    public Device Device { get; set; } = null!;
}
