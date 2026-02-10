namespace BITS.Signage.Domain.Entities;

/// <summary>
/// Represents a time slot in a published schedule version.
/// Stores the snapshot of schedule rules at the time of publish.
/// </summary>
public class SchedulePublishedSlot
{
    public string SchedulePublishedSlotId { get; set; } = null!;
    public string VenueScheduleSetId { get; set; } = null!;
    public int PublishedVersion { get; set; }

    // Target and content
    public string TargetType { get; set; } = null!; // DEVICE, DEVICE_GROUP, VENUE
    public string TargetId { get; set; } = null!;
    public string PlaylistId { get; set; } = null!;

    // Time window
    public int DaysOfWeekMask { get; set; }
    public int StartMinutes { get; set; }
    public int EndMinutes { get; set; }
    public int Priority { get; set; }
    public DateOnly ValidFromDate { get; set; }
    public DateOnly? ValidToDate { get; set; }
    public bool Enabled { get; set; } = true;

    // Navigation properties
    public VenueScheduleSet VenueScheduleSet { get; set; } = null!;
    public Playlist Playlist { get; set; } = null!;
}
