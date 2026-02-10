namespace BITS.Signage.Domain.Entities;

/// <summary>
/// Represents a published version of a venue's schedule.
/// Contains a snapshot of all schedule slots at the time of publish.
/// </summary>
public class SchedulePublished
{
    public string SchedulePublishedId { get; set; } = null!;
    public string VenueScheduleSetId { get; set; } = null!;
    public string VenueId { get; set; } = null!;
    public int PublishedVersion { get; set; }
    public DateTime PublishedAtUtc { get; set; }
    public string PublishedByUserId { get; set; } = null!;

    // Navigation properties
    public VenueScheduleSet VenueScheduleSet { get; set; } = null!;
    public User PublishedByUser { get; set; } = null!;
    public ICollection<SchedulePublishedSlot> Slots { get; set; } = [];
}
