namespace BITS.Signage.Domain.Entities;

/// <summary>
/// Represents a collection of schedule slots for a specific venue.
/// Venue schedules follow draft/publish versioning.
/// </summary>
public class VenueScheduleSet
{
    public string VenueScheduleSetId { get; set; } = null!;
    public string TenantId { get; set; } = null!;
    public string VenueId { get; set; } = null!;

    // Versioning
    public int DraftVersion { get; set; } = 1;
    public int PublishedVersion { get; set; } = 0; // 0 = never published

    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    // Navigation properties
    public Venue Venue { get; set; } = null!;
    public ICollection<ScheduleSlot> DraftSlots { get; set; } = [];
    public ICollection<SchedulePublished> PublishedVersions { get; set; } = [];
}
