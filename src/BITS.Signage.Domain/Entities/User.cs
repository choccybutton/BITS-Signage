namespace BITS.Signage.Domain.Entities;

/// <summary>
/// Represents a user who can log in and access one or more tenants/venues.
/// </summary>
public class User
{
    public string UserId { get; set; } = null!;
    public string TenantId { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string PasswordHash { get; set; } = null!; // Securely hashed password
    public string Status { get; set; } = "ACTIVE"; // ACTIVE, SUSPENDED, DELETED
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public DateTime? DeletedAtUtc { get; set; }

    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public ICollection<TenantUserRole> TenantRoles { get; set; } = [];
    public ICollection<VenueUserRole> VenueRoles { get; set; } = [];
}
