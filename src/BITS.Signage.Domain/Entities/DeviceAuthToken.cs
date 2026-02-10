namespace BITS.Signage.Domain.Entities;

/// <summary>
/// Represents a JWT token issued to a device for authenticating API calls from the player.
/// Multiple tokens can exist per device (rotated on renewal).
/// </summary>
public class DeviceAuthToken
{
    public string DeviceAuthTokenId { get; set; } = null!;
    public string TenantId { get; set; } = null!;
    public string VenueId { get; set; } = null!;
    public string DeviceId { get; set; } = null!;
    public string TokenHash { get; set; } = null!; // SHA256 hash of the actual JWT token (never store plaintext)
    public string Status { get; set; } = "ACTIVE"; // ACTIVE, REVOKED, EXPIRED
    public DateTime IssuedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    // Navigation properties
    public Device Device { get; set; } = null!;
}
