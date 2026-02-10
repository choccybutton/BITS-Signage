using BITS.Signage.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BITS.Signage.Infrastructure.Persistence;

public class BitsSignageDbContext : DbContext
{
    public BitsSignageDbContext(DbContextOptions<BitsSignageDbContext> options) : base(options)
    {
    }

    // Phase 0.2 - Core & Auth Tables
    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<Venue> Venues { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<TenantUserRole> TenantUserRoles { get; set; } = null!;
    public DbSet<VenueUserRole> VenueUserRoles { get; set; } = null!;
    public DbSet<Device> Devices { get; set; } = null!;
    public DbSet<DeviceGroup> DeviceGroups { get; set; } = null!;
    public DbSet<DevicePairing> DevicePairings { get; set; } = null!;
    public DbSet<DeviceAuthToken> DeviceAuthTokens { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Tenant
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(e => e.TenantId);
            entity.Property(e => e.TenantId).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.Property(e => e.DefaultTimezone).HasMaxLength(100).IsRequired();

            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.DeletedAtUtc);
        });

        // Configure Venue
        modelBuilder.Entity<Venue>(entity =>
        {
            entity.HasKey(e => e.VenueId);
            entity.Property(e => e.VenueId).HasMaxLength(50);
            entity.Property(e => e.TenantId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Timezone).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.FallbackPlaylistId).HasMaxLength(50);

            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.Venues)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.DeletedAtUtc);
        });

        // Configure User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.UserId).HasMaxLength(50);
            entity.Property(e => e.TenantId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.Property(e => e.DisplayName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();

            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.Users)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.DeletedAtUtc);
        });

        // Configure TenantUserRole
        modelBuilder.Entity<TenantUserRole>(entity =>
        {
            entity.HasKey(e => e.TenantUserRoleId);
            entity.Property(e => e.TenantUserRoleId).HasMaxLength(50);
            entity.Property(e => e.TenantId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.UserId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Role).HasMaxLength(50).IsRequired();

            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.UserRoles)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany(u => u.TenantRoles)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.TenantId, e.UserId }).IsUnique();
            entity.HasIndex(e => e.Role);
        });

        // Configure VenueUserRole
        modelBuilder.Entity<VenueUserRole>(entity =>
        {
            entity.HasKey(e => e.VenueUserRoleId);
            entity.Property(e => e.VenueUserRoleId).HasMaxLength(50);
            entity.Property(e => e.TenantId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.VenueId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.UserId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Role).HasMaxLength(50).IsRequired();

            entity.HasOne(e => e.Venue)
                .WithMany(v => v.UserRoles)
                .HasForeignKey(e => e.VenueId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany(u => u.VenueRoles)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.VenueId, e.UserId }).IsUnique();
            entity.HasIndex(e => e.Role);
        });

        // Configure Device
        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasKey(e => e.DeviceId);
            entity.Property(e => e.DeviceId).HasMaxLength(50);
            entity.Property(e => e.TenantId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.VenueId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
            entity.Property(e => e.PrimaryGroupId).HasMaxLength(50);
            entity.Property(e => e.Timezone).HasMaxLength(100).IsRequired();
            entity.Property(e => e.RunMode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Orientation).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.Property(e => e.AppVersion).HasMaxLength(50);

            entity.HasOne(e => e.Venue)
                .WithMany(v => v.Devices)
                .HasForeignKey(e => e.VenueId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.PrimaryGroup)
                .WithMany(g => g.Devices)
                .HasForeignKey(e => e.PrimaryGroupId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.VenueId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.DeletedAtUtc);
        });

        // Configure DeviceGroup
        modelBuilder.Entity<DeviceGroup>(entity =>
        {
            entity.HasKey(e => e.GroupId);
            entity.Property(e => e.GroupId).HasMaxLength(50);
            entity.Property(e => e.TenantId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.VenueId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();

            entity.HasOne(e => e.Venue)
                .WithMany(v => v.DeviceGroups)
                .HasForeignKey(e => e.VenueId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.VenueId);
        });

        // Configure DevicePairing
        modelBuilder.Entity<DevicePairing>(entity =>
        {
            entity.HasKey(e => e.PairingId);
            entity.Property(e => e.PairingId).HasMaxLength(50);
            entity.Property(e => e.PairingCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.DeviceHardwareId).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ClaimedByUserId).HasMaxLength(50);
            entity.Property(e => e.ClaimedTenantId).HasMaxLength(50);
            entity.Property(e => e.ClaimedVenueId).HasMaxLength(50);
            entity.Property(e => e.ResultingDeviceId).HasMaxLength(50);

            entity.HasOne(e => e.ClaimedByUser)
                .WithMany()
                .HasForeignKey(e => e.ClaimedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.ClaimedTenant)
                .WithMany()
                .HasForeignKey(e => e.ClaimedTenantId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.ClaimedVenue)
                .WithMany()
                .HasForeignKey(e => e.ClaimedVenueId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.ResultingDevice)
                .WithMany()
                .HasForeignKey(e => e.ResultingDeviceId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.PairingCode).IsUnique();
            entity.HasIndex(e => e.DeviceHardwareId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ExpiresAtUtc);
        });

        // Configure DeviceAuthToken
        modelBuilder.Entity<DeviceAuthToken>(entity =>
        {
            entity.HasKey(e => e.DeviceAuthTokenId);
            entity.Property(e => e.DeviceAuthTokenId).HasMaxLength(50);
            entity.Property(e => e.TenantId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.VenueId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.DeviceId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.TokenHash).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();

            entity.HasOne(e => e.Device)
                .WithMany(d => d.AuthTokens)
                .HasForeignKey(e => e.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.DeviceId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ExpiresAtUtc);
        });
    }
}
