namespace BITS.Signage.Domain.Entities;

/// <summary>
/// Represents a reusable grouping of assets and/or other content groups.
/// Content groups follow draft/publish versioning and can be nested (max depth 3).
/// When a published content group is updated, the admin can choose to auto-propagate to referencing playlists.
/// </summary>
public class ContentGroup
{
    public string ContentGroupId { get; set; } = null!;
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
    public ICollection<ContentGroupItem> DraftItems { get; set; } = [];
    public ICollection<ContentGroupPublished> PublishedVersions { get; set; } = [];
}
