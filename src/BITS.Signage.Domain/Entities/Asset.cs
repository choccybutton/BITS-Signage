namespace BITS.Signage.Domain.Entities;

/// <summary>
/// Represents a media asset (video, image, document) that can be added to playlists and content groups.
/// Assets are scoped to either a tenant (shared) or a specific venue.
/// </summary>
public class Asset
{
    public string AssetId { get; set; } = null!;
    public string TenantId { get; set; } = null!;
    public string ScopeType { get; set; } = null!; // TENANT or VENUE
    public string? ScopeVenueId { get; set; } // Non-null if ScopeType = VENUE
    public string Type { get; set; } = null!; // VIDEO, IMAGE, DOCUMENT, WEBPAGE
    public string Status { get; set; } = "ACTIVE"; // ACTIVE, ARCHIVED, DELETED
    public string DisplayName { get; set; } = null!;

    // File metadata
    public string FileName { get; set; } = null!;
    public long FileSizeBytes { get; set; }
    public string MediaType { get; set; } = null!; // MIME type (video/mp4, image/jpeg, etc)
    public string ChecksumSha256 { get; set; } = null!; // For integrity verification

    // Media-specific metadata (JSON or nullable fields)
    public int? DurationSeconds { get; set; } // For videos and audio
    public int? WidthPixels { get; set; } // For images and videos
    public int? HeightPixels { get; set; } // For images and videos

    // Storage URLs
    public string CdnUrl { get; set; } = null!; // Full CDN URL for playback
    public string? ThumbnailUrl { get; set; } // Optional thumbnail URL

    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
}
