namespace BITS.Signage.Domain.Entities;

/// <summary>
/// Represents a published version of a playlist.
/// At publish time, all content groups are expanded into their constituent assets.
/// The resulting flat asset list is stored in PlaylistPublishedItem.
/// </summary>
public class PlaylistPublished
{
    public string PlaylistPublishedId { get; set; } = null!;
    public string PlaylistId { get; set; } = null!;
    public int PublishedVersion { get; set; }
    public DateTime PublishedAtUtc { get; set; }
    public string PublishedByUserId { get; set; } = null!;

    // Navigation properties
    public Playlist Playlist { get; set; } = null!;
    public User PublishedByUser { get; set; } = null!;
    public ICollection<PlaylistPublishedItem> Items { get; set; } = [];
}
