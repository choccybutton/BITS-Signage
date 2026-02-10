namespace BITS.Signage.Domain.Entities;

/// <summary>
/// Represents an item within a published playlist version (always an asset).
/// At publish time, content groups are expanded and stored here as individual assets.
/// Includes audit trail info about the source content group (if this asset came from a group).
/// </summary>
public class PlaylistPublishedItem
{
    public string PlaylistPublishedItemId { get; set; } = null!;
    public string PlaylistId { get; set; } = null!;
    public int PublishedVersion { get; set; }
    public string AssetId { get; set; } = null!; // Always populated (groups expanded at publish)
    public int Sequence { get; set; }
    public int? DurationSeconds { get; set; }

    // Audit trail: where this asset came from
    public string? SourceContentGroupId { get; set; } // Non-null if this came from a content group
    public int? SourceContentGroupVersion { get; set; } // Version of source group (for audit)

    // Navigation properties
    public Playlist Playlist { get; set; } = null!;
    public Asset Asset { get; set; } = null!;
    public ContentGroup? SourceContentGroup { get; set; }
}
