namespace BITS.Signage.Domain.Entities;

/// <summary>
/// Represents a role assignment of a user within a tenant (e.g., TENANT_ADMIN, TENANT_EDITOR).
/// </summary>
public class TenantUserRole
{
    public string TenantUserRoleId { get; set; } = null!;
    public string TenantId { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public string Role { get; set; } = null!; // TENANT_ADMIN, TENANT_EDITOR, TENANT_VIEWER
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public User User { get; set; } = null!;
}
