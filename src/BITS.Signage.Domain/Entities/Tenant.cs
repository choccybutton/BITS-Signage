namespace BITS.Signage.Domain.Entities;

/// <summary>
/// Represents a tenant organization that owns venues and manages multiple users.
/// </summary>
public class Tenant
{
    public string TenantId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Status { get; set; } = "ACTIVE"; // ACTIVE, SUSPENDED, DELETED
    public string DefaultTimezone { get; set; } = "UTC";
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public DateTime? DeletedAtUtc { get; set; }

    // Navigation properties
    public ICollection<Venue> Venues { get; set; } = [];
    public ICollection<User> Users { get; set; } = [];
    public ICollection<TenantUserRole> UserRoles { get; set; } = [];
}
