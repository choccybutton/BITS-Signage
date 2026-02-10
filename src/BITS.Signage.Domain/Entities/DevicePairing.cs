namespace BITS.Signage.Domain.Entities;

/// <summary>
/// Represents a pairing session between a physical device and a venue.
/// Devices initiate pairing by providing a hardware ID and receiving a pairing code.
/// An admin user then claims the pairing for a specific venue.
/// </summary>
public class DevicePairing
{
    public string PairingId { get; set; } = null!;
    public string PairingCode { get; set; } = null!; // Unique, human-readable code (e.g., "ABC123")
    public string DeviceHardwareId { get; set; } = null!;
    public string Status { get; set; } = "PENDING"; // PENDING, CLAIMED, COMPLETED, EXPIRED, FAILED
    public DateTime ExpiresAtUtc { get; set; }
    public string? ClaimedByUserId { get; set; }
    public string? ClaimedTenantId { get; set; }
    public string? ClaimedVenueId { get; set; }
    public string? ResultingDeviceId { get; set; } // Set when pairing completes
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    // Navigation properties
    public User? ClaimedByUser { get; set; }
    public Tenant? ClaimedTenant { get; set; }
    public Venue? ClaimedVenue { get; set; }
    public Device? ResultingDevice { get; set; }
}
