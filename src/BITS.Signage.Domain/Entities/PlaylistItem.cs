namespace BITS.Signage.Domain.Entities;

/// <summary>
/// Represents an item within a playlist (draft version).
/// Each item is either an asset OR a content group (XOR constraint).
/// When the playlist is published, content groups are expanded into their assets.
/// </summary>
public class PlaylistItem
{
    public string PlaylistItemId { get; set; } = null!;
    public string PlaylistId { get; set; } = null!;
    public string? AssetId { get; set; } // Nullable — null if this is a content group reference
    public string? ContentGroupId { get; set; } // Nullable — null if this is a direct asset
    // NOTE: CHECK constraint at DB level ensures exactly one of (AssetId, ContentGroupId) is non-null

    public int Sequence { get; set; }
    public int? DurationSeconds { get; set; }
    public bool Enabled { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    // Navigation properties
    public Playlist Playlist { get; set; } = null!;
    public Asset? Asset { get; set; }
    public ContentGroup? ContentGroup { get; set; }
}
