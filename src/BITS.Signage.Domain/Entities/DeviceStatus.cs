namespace BITS.Signage.Domain.Entities;

/// <summary>
/// Tracks the runtime status of a device.
/// Updated periodically via device heartbeats.
/// Stores playback stats, cache status, and network information.
/// </summary>
public class DeviceStatus
{
    public string DeviceId { get; set; } = null!;
    public string VenueId { get; set; } = null!;
    public DateTime LastHeartbeatAtUtc { get; set; }
    public bool IsOnline { get; set; }

    // Playback state
    public string? CurrentPlaylistId { get; set; }
    public int? CurrentPlaylistVersion { get; set; }
    public long? CurrentManifestEpoch { get; set; }

    // Cache stats
    public long CachedBytesUsed { get; set; } = 0;
    public int CachedAssetCount { get; set; } = 0;

    // Network / System stats
    public int? WifiSignalStrengthDbm { get; set; }
    public int? BatteryLevelPercent { get; set; }
    public long? FreeStorageBytesLocal { get; set; }
    public long? FreeDiskSpaceBytes { get; set; }

    // Playback metrics
    public int PlaybackErrorCount { get; set; } = 0;
    public DateTime? LastPlaybackErrorAtUtc { get; set; }
    public string? LastPlaybackErrorMessage { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    // Navigation properties
    public Device Device { get; set; } = null!;
}
