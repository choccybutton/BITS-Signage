namespace BITS.Signage.Domain.Entities;

/// <summary>
/// Represents an item within a content group (draft version).
/// Each item is either an asset OR a child content group (XOR constraint).
/// Items have a sequence order and optional duration override.
/// </summary>
public class ContentGroupItem
{
    public string ContentGroupItemId { get; set; } = null!;
    public string ContentGroupId { get; set; } = null!;
    public string? AssetId { get; set; } // Nullable — null if this is a child group
    public string? ChildGroupId { get; set; } // Nullable — null if this is an asset
    // NOTE: CHECK constraint at DB level ensures exactly one of (AssetId, ChildGroupId) is non-null

    public int Sequence { get; set; } // Order within the group
    public int? DurationSeconds { get; set; } // Optional override; null means use asset's duration
    public bool Enabled { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    // Navigation properties
    public ContentGroup ContentGroup { get; set; } = null!;
    public Asset? Asset { get; set; }
    public ContentGroup? ChildGroup { get; set; }
}
