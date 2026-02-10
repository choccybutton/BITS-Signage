namespace BITS.Signage.Domain.Entities;

/// <summary>
/// Represents a physical display device (Android player) that shows content in a venue.
/// </summary>
public class Device
{
    public string DeviceId { get; set; } = null!;
    public string TenantId { get; set; } = null!;
    public string VenueId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? PrimaryGroupId { get; set; }
    public string Timezone { get; set; } = "UTC";
    public string RunMode { get; set; } = "NORMAL"; // NORMAL, RECOVERY, MAINTENANCE
    public bool LaunchOnBoot { get; set; } = true;
    public bool AutoRelaunch { get; set; } = true;
    public bool KeepAwake { get; set; } = false;
    public bool WatchdogReturnToForeground { get; set; } = true;
    public string Orientation { get; set; } = "PORTRAIT"; // PORTRAIT, LANDSCAPE
    public string Status { get; set; } = "OFFLINE"; // ONLINE, OFFLINE, ERROR
    public DateTime? LastSeenAtUtc { get; set; }
    public string? AppVersion { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public DateTime? DeletedAtUtc { get; set; }

    // Navigation properties
    public Venue Venue { get; set; } = null!;
    public DeviceGroup? PrimaryGroup { get; set; }
    public ICollection<DeviceAuthToken> AuthTokens { get; set; } = [];
}
