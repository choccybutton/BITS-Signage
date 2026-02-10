namespace BITS.Signage.Domain.Entities;

/// <summary>
/// Represents an item within a published content group version.
/// Stores the snapshot of an asset OR child group at the time of publish.
/// For nested groups, also records which published version of the child was used.
/// </summary>
public class ContentGroupPublishedItem
{
    public string ContentGroupPublishedItemId { get; set; } = null!;
    public string ContentGroupId { get; set; } = null!;
    public int PublishedVersion { get; set; }
    public string? AssetId { get; set; } // Nullable — null if this is a child group
    public string? ChildGroupId { get; set; } // Nullable — null if this is an asset
    public int? ChildGroupPublishedVersion { get; set; } // Version of child group used (if ChildGroupId is non-null)
    // NOTE: CHECK constraint at DB level ensures exactly one of (AssetId, ChildGroupId) is non-null

    public int Sequence { get; set; }
    public int? DurationSeconds { get; set; }

    // Navigation properties
    public ContentGroup ContentGroup { get; set; } = null!;
    public Asset? Asset { get; set; }
    public ContentGroup? ChildGroup { get; set; }
}
