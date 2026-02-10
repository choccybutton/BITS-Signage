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

    // Phase 0.3 - Content, Playlist, Schedule Tables
    public DbSet<Asset> Assets { get; set; } = null!;
    public DbSet<ContentGroup> ContentGroups { get; set; } = null!;
    public DbSet<ContentGroupItem> ContentGroupItems { get; set; } = null!;
    public DbSet<ContentGroupPublished> ContentGroupPublisheds { get; set; } = null!;
    public DbSet<ContentGroupPublishedItem> ContentGroupPublishedItems { get; set; } = null!;
    public DbSet<Playlist> Playlists { get; set; } = null!;
    public DbSet<PlaylistItem> PlaylistItems { get; set; } = null!;
    public DbSet<PlaylistPublished> PlaylistPublisheds { get; set; } = null!;
    public DbSet<PlaylistPublishedItem> PlaylistPublishedItems { get; set; } = null!;
    public DbSet<VenueScheduleSet> VenueScheduleSets { get; set; } = null!;
    public DbSet<ScheduleSlot> ScheduleSlots { get; set; } = null!;
    public DbSet<SchedulePublished> SchedulePublisheds { get; set; } = null!;
    public DbSet<SchedulePublishedSlot> SchedulePublishedSlots { get; set; } = null!;
    public DbSet<VenueEpoch> VenueEpochs { get; set; } = null!;
    public DbSet<DeviceStatus> DeviceStatuses { get; set; } = null!;
    public DbSet<DeviceEvent> DeviceEvents { get; set; } = null!;
    public DbSet<DeviceCommand> DeviceCommands { get; set; } = null!;
    public DbSet<PublishEvent> PublishEvents { get; set; } = null!;
    public DbSet<EditorPresence> EditorPresences { get; set; } = null!;

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

        // Configure Asset
        modelBuilder.Entity<Asset>(entity =>
        {
            entity.HasKey(e => e.AssetId);
            entity.Property(e => e.AssetId).HasMaxLength(50);
            entity.Property(e => e.TenantId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ScopeType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ScopeVenueId).HasMaxLength(50);
            entity.Property(e => e.Type).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.Property(e => e.DisplayName).HasMaxLength(500).IsRequired();
            entity.Property(e => e.FileName).HasMaxLength(500).IsRequired();
            entity.Property(e => e.MediaType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ChecksumSha256).HasMaxLength(64).IsRequired();
            entity.Property(e => e.CdnUrl).IsRequired();
            entity.Property(e => e.ThumbnailUrl).HasMaxLength(2000);

            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.ScopeVenueId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.DeletedAtUtc);
        });

        // Configure ContentGroup
        modelBuilder.Entity<ContentGroup>(entity =>
        {
            entity.HasKey(e => e.ContentGroupId);
            entity.Property(e => e.ContentGroupId).HasMaxLength(50);
            entity.Property(e => e.TenantId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ScopeType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ScopeVenueId).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();

            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.ScopeVenueId);
            entity.HasIndex(e => e.DeletedAtUtc);
        });

        // Configure ContentGroupItem
        modelBuilder.Entity<ContentGroupItem>(entity =>
        {
            entity.HasKey(e => e.ContentGroupItemId);
            entity.Property(e => e.ContentGroupItemId).HasMaxLength(50);
            entity.Property(e => e.ContentGroupId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.AssetId).HasMaxLength(50);
            entity.Property(e => e.ChildGroupId).HasMaxLength(50);

            entity.HasOne(e => e.ContentGroup)
                .WithMany(cg => cg.DraftItems)
                .HasForeignKey(e => e.ContentGroupId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Asset)
                .WithMany()
                .HasForeignKey(e => e.AssetId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.ChildGroup)
                .WithMany()
                .HasForeignKey(e => e.ChildGroupId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.ContentGroupId);
        });

        // Configure ContentGroupPublished
        modelBuilder.Entity<ContentGroupPublished>(entity =>
        {
            entity.HasKey(e => e.ContentGroupPublishedId);
            entity.Property(e => e.ContentGroupPublishedId).HasMaxLength(50);
            entity.Property(e => e.ContentGroupId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.PublishedByUserId).HasMaxLength(50).IsRequired();

            entity.HasOne(e => e.ContentGroup)
                .WithMany(cg => cg.PublishedVersions)
                .HasForeignKey(e => e.ContentGroupId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.PublishedByUser)
                .WithMany()
                .HasForeignKey(e => e.PublishedByUserId);

            entity.HasIndex(e => new { e.ContentGroupId, e.PublishedVersion }).IsUnique();
        });

        // Configure ContentGroupPublishedItem
        modelBuilder.Entity<ContentGroupPublishedItem>(entity =>
        {
            entity.HasKey(e => e.ContentGroupPublishedItemId);
            entity.Property(e => e.ContentGroupPublishedItemId).HasMaxLength(50);
            entity.Property(e => e.ContentGroupId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.AssetId).HasMaxLength(50);
            entity.Property(e => e.ChildGroupId).HasMaxLength(50);

            entity.HasOne(e => e.ContentGroup)
                .WithMany()
                .HasForeignKey(e => e.ContentGroupId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Asset)
                .WithMany()
                .HasForeignKey(e => e.AssetId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.ChildGroup)
                .WithMany()
                .HasForeignKey(e => e.ChildGroupId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure Playlist
        modelBuilder.Entity<Playlist>(entity =>
        {
            entity.HasKey(e => e.PlaylistId);
            entity.Property(e => e.PlaylistId).HasMaxLength(50);
            entity.Property(e => e.TenantId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ScopeType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ScopeVenueId).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();

            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.ScopeVenueId);
            entity.HasIndex(e => e.DeletedAtUtc);
        });

        // Configure PlaylistItem
        modelBuilder.Entity<PlaylistItem>(entity =>
        {
            entity.HasKey(e => e.PlaylistItemId);
            entity.Property(e => e.PlaylistItemId).HasMaxLength(50);
            entity.Property(e => e.PlaylistId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.AssetId).HasMaxLength(50);
            entity.Property(e => e.ContentGroupId).HasMaxLength(50);

            entity.HasOne(e => e.Playlist)
                .WithMany(p => p.DraftItems)
                .HasForeignKey(e => e.PlaylistId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Asset)
                .WithMany()
                .HasForeignKey(e => e.AssetId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.ContentGroup)
                .WithMany()
                .HasForeignKey(e => e.ContentGroupId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.PlaylistId);
        });

        // Configure PlaylistPublished
        modelBuilder.Entity<PlaylistPublished>(entity =>
        {
            entity.HasKey(e => e.PlaylistPublishedId);
            entity.Property(e => e.PlaylistPublishedId).HasMaxLength(50);
            entity.Property(e => e.PlaylistId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.PublishedByUserId).HasMaxLength(50).IsRequired();

            entity.HasOne(e => e.Playlist)
                .WithMany(p => p.PublishedVersions)
                .HasForeignKey(e => e.PlaylistId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.PublishedByUser)
                .WithMany()
                .HasForeignKey(e => e.PublishedByUserId);

            entity.HasIndex(e => new { e.PlaylistId, e.PublishedVersion }).IsUnique();
        });

        // Configure PlaylistPublishedItem
        modelBuilder.Entity<PlaylistPublishedItem>(entity =>
        {
            entity.HasKey(e => e.PlaylistPublishedItemId);
            entity.Property(e => e.PlaylistPublishedItemId).HasMaxLength(50);
            entity.Property(e => e.PlaylistId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.AssetId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.SourceContentGroupId).HasMaxLength(50);

            entity.HasOne(e => e.Playlist)
                .WithMany()
                .HasForeignKey(e => e.PlaylistId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Asset)
                .WithMany()
                .HasForeignKey(e => e.AssetId);

            entity.HasOne(e => e.SourceContentGroup)
                .WithMany()
                .HasForeignKey(e => e.SourceContentGroupId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure VenueScheduleSet
        modelBuilder.Entity<VenueScheduleSet>(entity =>
        {
            entity.HasKey(e => e.VenueScheduleSetId);
            entity.Property(e => e.VenueScheduleSetId).HasMaxLength(50);
            entity.Property(e => e.TenantId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.VenueId).HasMaxLength(50).IsRequired();

            entity.HasOne(e => e.Venue)
                .WithMany()
                .HasForeignKey(e => e.VenueId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.VenueId);
        });

        // Configure ScheduleSlot
        modelBuilder.Entity<ScheduleSlot>(entity =>
        {
            entity.HasKey(e => e.SlotId);
            entity.Property(e => e.SlotId).HasMaxLength(50);
            entity.Property(e => e.TenantId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.VenueScheduleSetId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.VenueId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.TargetType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.TargetId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.PlaylistId).HasMaxLength(50).IsRequired();

            entity.HasOne(e => e.VenueScheduleSet)
                .WithMany(ss => ss.DraftSlots)
                .HasForeignKey(e => e.VenueScheduleSetId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Playlist)
                .WithMany()
                .HasForeignKey(e => e.PlaylistId);

            entity.HasIndex(e => e.VenueId);
            entity.HasIndex(e => e.TargetId);
            entity.HasIndex(e => new { e.VenueId, e.DaysOfWeekMask, e.StartMinutes, e.EndMinutes });
        });

        // Configure SchedulePublished
        modelBuilder.Entity<SchedulePublished>(entity =>
        {
            entity.HasKey(e => e.SchedulePublishedId);
            entity.Property(e => e.SchedulePublishedId).HasMaxLength(50);
            entity.Property(e => e.VenueScheduleSetId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.VenueId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.PublishedByUserId).HasMaxLength(50).IsRequired();

            entity.HasOne(e => e.VenueScheduleSet)
                .WithMany(ss => ss.PublishedVersions)
                .HasForeignKey(e => e.VenueScheduleSetId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.PublishedByUser)
                .WithMany()
                .HasForeignKey(e => e.PublishedByUserId);

            entity.HasIndex(e => new { e.VenueScheduleSetId, e.PublishedVersion }).IsUnique();
        });

        // Configure SchedulePublishedSlot
        modelBuilder.Entity<SchedulePublishedSlot>(entity =>
        {
            entity.HasKey(e => e.SchedulePublishedSlotId);
            entity.Property(e => e.SchedulePublishedSlotId).HasMaxLength(50);
            entity.Property(e => e.VenueScheduleSetId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.TargetType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.TargetId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.PlaylistId).HasMaxLength(50).IsRequired();

            entity.HasOne(e => e.VenueScheduleSet)
                .WithMany()
                .HasForeignKey(e => e.VenueScheduleSetId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Playlist)
                .WithMany()
                .HasForeignKey(e => e.PlaylistId);

            entity.HasIndex(e => new { e.VenueScheduleSetId, e.PublishedVersion });
        });

        // Configure VenueEpoch
        modelBuilder.Entity<VenueEpoch>(entity =>
        {
            entity.HasKey(e => e.VenueId);
            entity.Property(e => e.VenueId).HasMaxLength(50);
            entity.Property(e => e.TenantId).HasMaxLength(50).IsRequired();

            entity.HasOne(e => e.Venue)
                .WithMany()
                .HasForeignKey(e => e.VenueId);
        });

        // Configure DeviceStatus
        modelBuilder.Entity<DeviceStatus>(entity =>
        {
            entity.HasKey(e => e.DeviceId);
            entity.Property(e => e.DeviceId).HasMaxLength(50);
            entity.Property(e => e.VenueId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.CurrentPlaylistId).HasMaxLength(50);

            entity.HasOne(e => e.Device)
                .WithMany()
                .HasForeignKey(e => e.DeviceId);

            entity.HasIndex(e => e.VenueId);
            entity.HasIndex(e => e.IsOnline);
            entity.HasIndex(e => e.LastHeartbeatAtUtc);
        });

        // Configure DeviceEvent
        modelBuilder.Entity<DeviceEvent>(entity =>
        {
            entity.HasKey(e => e.DeviceEventId);
            entity.Property(e => e.DeviceEventId).HasMaxLength(50);
            entity.Property(e => e.DeviceId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.EventType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Severity).HasMaxLength(50).IsRequired();

            entity.HasOne(e => e.Device)
                .WithMany()
                .HasForeignKey(e => e.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.DeviceId);
            entity.HasIndex(e => e.EventType);
            entity.HasIndex(e => e.CreatedAtUtc);
            entity.HasIndex(e => e.Severity);
        });

        // Configure DeviceCommand
        modelBuilder.Entity<DeviceCommand>(entity =>
        {
            entity.HasKey(e => e.CommandId);
            entity.Property(e => e.CommandId).HasMaxLength(50);
            entity.Property(e => e.DeviceId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Type).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.Property(e => e.IssuedByUserId).HasMaxLength(50);

            entity.HasOne(e => e.Device)
                .WithMany()
                .HasForeignKey(e => e.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.IssuedByUser)
                .WithMany()
                .HasForeignKey(e => e.IssuedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.DeviceId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.IssuedAtUtc);
            entity.HasIndex(e => e.TimeoutAtUtc);
        });

        // Configure PublishEvent
        modelBuilder.Entity<PublishEvent>(entity =>
        {
            entity.HasKey(e => e.PublishEventId);
            entity.Property(e => e.PublishEventId).HasMaxLength(50);
            entity.Property(e => e.TenantId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.VenueId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Type).HasMaxLength(50).IsRequired();
            entity.Property(e => e.EntityId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.PublishedByUserId).HasMaxLength(50).IsRequired();

            entity.HasOne(e => e.PublishedByUser)
                .WithMany()
                .HasForeignKey(e => e.PublishedByUserId);

            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.VenueId);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.EntityId);
            entity.HasIndex(e => e.PublishedAtUtc);
        });

        // Configure EditorPresence
        modelBuilder.Entity<EditorPresence>(entity =>
        {
            entity.HasKey(e => e.PresenceId);
            entity.Property(e => e.PresenceId).HasMaxLength(50);
            entity.Property(e => e.TenantId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ResourceType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ResourceId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.UserId).HasMaxLength(50).IsRequired();

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId);

            entity.HasIndex(e => new { e.TenantId, e.ResourceType, e.ResourceId });
            entity.HasIndex(e => e.ExpiresAtUtc);
        });
    }
}
