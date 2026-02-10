namespace BITS.Signage.Domain.Entities;

/// <summary>
/// Represents a published version of a content group.
/// Each publish creates a snapshot that can be referenced by playlists.
/// </summary>
public class ContentGroupPublished
{
    public string ContentGroupPublishedId { get; set; } = null!;
    public string ContentGroupId { get; set; } = null!;
    public int PublishedVersion { get; set; }
    public DateTime PublishedAtUtc { get; set; }
    public string PublishedByUserId { get; set; } = null!;

    // Navigation properties
    public ContentGroup ContentGroup { get; set; } = null!;
    public User PublishedByUser { get; set; } = null!;
    public ICollection<ContentGroupPublishedItem> Items { get; set; } = [];
}
