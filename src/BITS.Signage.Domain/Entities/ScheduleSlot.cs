namespace BITS.Signage.Domain.Entities;

/// <summary>
/// Represents a time slot in a schedule that maps a playlist to a time window.
/// Slots define which playlist to display on devices at specific times.
/// </summary>
public class ScheduleSlot
{
    public string SlotId { get; set; } = null!;
    public string TenantId { get; set; } = null!;
    public string VenueScheduleSetId { get; set; } = null!;
    public string VenueId { get; set; } = null!;

    // Target and content
    public string TargetType { get; set; } = null!; // DEVICE, DEVICE_GROUP, VENUE (determines what gets scheduled)
    public string TargetId { get; set; } = null!; // deviceId, deviceGroupId, or empty for venue-wide
    public string PlaylistId { get; set; } = null!; // Published playlist to display

    // Time window
    public int DaysOfWeekMask { get; set; } // Bitmask: bit 0=Sunday, bit 1=Monday, ..., bit 6=Saturday
    public int StartMinutes { get; set; } // Minutes since midnight (0-1439)
    public int EndMinutes { get; set; } // Minutes since midnight (0-1439)

    // Priority and validity
    public int Priority { get; set; } // Higher number = higher priority when slots overlap
    public DateOnly ValidFromDate { get; set; } // Schedule valid from this date
    public DateOnly? ValidToDate { get; set; } // Schedule valid until this date (null = indefinite)
    public bool Enabled { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    // Navigation properties
    public VenueScheduleSet VenueScheduleSet { get; set; } = null!;
    public Playlist Playlist { get; set; } = null!;
}
