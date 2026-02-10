namespace BITS.Signage.Domain.Entities;

/// <summary>
/// Represents a playlist of content items (assets or content groups) to be displayed.
/// Playlists follow draft/publish versioning and are scoped to tenant or venue.
/// When published, content groups are expanded into their constituent assets.
/// </summary>
public class Playlist
{
    public string PlaylistId { get; set; } = null!;
    public string TenantId { get; set; } = null!;
    public string ScopeType { get; set; } = null!; // TENANT or VENUE
    public string? ScopeVenueId { get; set; } // Non-null if ScopeType = VENUE
    public string Name { get; set; } = null!;

    // Versioning
    public int DraftVersion { get; set; } = 1;
    public int PublishedVersion { get; set; } = 0; // 0 = never published

    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public DateTime? DeletedAtUtc { get; set; }

    // Navigation properties
    public ICollection<PlaylistItem> DraftItems { get; set; } = [];
    public ICollection<PlaylistPublished> PublishedVersions { get; set; } = [];
}
