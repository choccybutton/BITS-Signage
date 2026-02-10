using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BITS.Signage.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase0_3_ContentPlaylistSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Assets",
                columns: table => new
                {
                    AssetId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TenantId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ScopeType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ScopeVenueId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    MediaType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ChecksumSha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: true),
                    WidthPixels = table.Column<int>(type: "integer", nullable: true),
                    HeightPixels = table.Column<int>(type: "integer", nullable: true),
                    CdnUrl = table.Column<string>(type: "text", nullable: false),
                    ThumbnailUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assets", x => x.AssetId);
                });

            migrationBuilder.CreateTable(
                name: "ContentGroups",
                columns: table => new
                {
                    ContentGroupId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TenantId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ScopeType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ScopeVenueId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    DraftVersion = table.Column<int>(type: "integer", nullable: false),
                    PublishedVersion = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentGroups", x => x.ContentGroupId);
                });

            migrationBuilder.CreateTable(
                name: "DeviceCommands",
                columns: table => new
                {
                    CommandId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DeviceId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ParametersJson = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IssuedByUserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IssuedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AcknowledgedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TimeoutAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResultJson = table.Column<string>(type: "text", nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceCommands", x => x.CommandId);
                    table.ForeignKey(
                        name: "FK_DeviceCommands_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "DeviceId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeviceCommands_Users_IssuedByUserId",
                        column: x => x.IssuedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DeviceEvents",
                columns: table => new
                {
                    DeviceEventId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DeviceId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EventTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PayloadJson = table.Column<string>(type: "text", nullable: true),
                    Severity = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceEvents", x => x.DeviceEventId);
                    table.ForeignKey(
                        name: "FK_DeviceEvents_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "DeviceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeviceStatuses",
                columns: table => new
                {
                    DeviceId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    VenueId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastHeartbeatAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsOnline = table.Column<bool>(type: "boolean", nullable: false),
                    CurrentPlaylistId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CurrentPlaylistVersion = table.Column<int>(type: "integer", nullable: true),
                    CurrentManifestEpoch = table.Column<long>(type: "bigint", nullable: true),
                    CachedBytesUsed = table.Column<long>(type: "bigint", nullable: false),
                    CachedAssetCount = table.Column<int>(type: "integer", nullable: false),
                    WifiSignalStrengthDbm = table.Column<int>(type: "integer", nullable: true),
                    BatteryLevelPercent = table.Column<int>(type: "integer", nullable: true),
                    FreeStorageBytesLocal = table.Column<long>(type: "bigint", nullable: true),
                    FreeDiskSpaceBytes = table.Column<long>(type: "bigint", nullable: true),
                    PlaybackErrorCount = table.Column<int>(type: "integer", nullable: false),
                    LastPlaybackErrorAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastPlaybackErrorMessage = table.Column<string>(type: "text", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceStatuses", x => x.DeviceId);
                    table.ForeignKey(
                        name: "FK_DeviceStatuses_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "DeviceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EditorPresences",
                columns: table => new
                {
                    PresenceId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TenantId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ResourceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ResourceId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EditorPresences", x => x.PresenceId);
                    table.ForeignKey(
                        name: "FK_EditorPresences_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Playlists",
                columns: table => new
                {
                    PlaylistId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TenantId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ScopeType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ScopeVenueId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    DraftVersion = table.Column<int>(type: "integer", nullable: false),
                    PublishedVersion = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Playlists", x => x.PlaylistId);
                });

            migrationBuilder.CreateTable(
                name: "PublishEvents",
                columns: table => new
                {
                    PublishEventId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TenantId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    VenueId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    PublishedByUserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PublishedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PropagatedPlaylistIds = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublishEvents", x => x.PublishEventId);
                    table.ForeignKey(
                        name: "FK_PublishEvents_Users_PublishedByUserId",
                        column: x => x.PublishedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VenueEpochs",
                columns: table => new
                {
                    VenueId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TenantId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ManifestEpoch = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VenueEpochs", x => x.VenueId);
                    table.ForeignKey(
                        name: "FK_VenueEpochs_Venues_VenueId",
                        column: x => x.VenueId,
                        principalTable: "Venues",
                        principalColumn: "VenueId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VenueScheduleSets",
                columns: table => new
                {
                    VenueScheduleSetId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TenantId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    VenueId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DraftVersion = table.Column<int>(type: "integer", nullable: false),
                    PublishedVersion = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VenueScheduleSets", x => x.VenueScheduleSetId);
                    table.ForeignKey(
                        name: "FK_VenueScheduleSets_Venues_VenueId",
                        column: x => x.VenueId,
                        principalTable: "Venues",
                        principalColumn: "VenueId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContentGroupItems",
                columns: table => new
                {
                    ContentGroupItemId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ContentGroupId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AssetId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ChildGroupId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: true),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentGroupItems", x => x.ContentGroupItemId);
                    table.ForeignKey(
                        name: "FK_ContentGroupItems_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "AssetId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ContentGroupItems_ContentGroups_ChildGroupId",
                        column: x => x.ChildGroupId,
                        principalTable: "ContentGroups",
                        principalColumn: "ContentGroupId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ContentGroupItems_ContentGroups_ContentGroupId",
                        column: x => x.ContentGroupId,
                        principalTable: "ContentGroups",
                        principalColumn: "ContentGroupId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContentGroupPublisheds",
                columns: table => new
                {
                    ContentGroupPublishedId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ContentGroupId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PublishedVersion = table.Column<int>(type: "integer", nullable: false),
                    PublishedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PublishedByUserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentGroupPublisheds", x => x.ContentGroupPublishedId);
                    table.ForeignKey(
                        name: "FK_ContentGroupPublisheds_ContentGroups_ContentGroupId",
                        column: x => x.ContentGroupId,
                        principalTable: "ContentGroups",
                        principalColumn: "ContentGroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContentGroupPublisheds_Users_PublishedByUserId",
                        column: x => x.PublishedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlaylistItems",
                columns: table => new
                {
                    PlaylistItemId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PlaylistId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AssetId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ContentGroupId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: true),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaylistItems", x => x.PlaylistItemId);
                    table.ForeignKey(
                        name: "FK_PlaylistItems_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "AssetId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PlaylistItems_ContentGroups_ContentGroupId",
                        column: x => x.ContentGroupId,
                        principalTable: "ContentGroups",
                        principalColumn: "ContentGroupId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PlaylistItems_Playlists_PlaylistId",
                        column: x => x.PlaylistId,
                        principalTable: "Playlists",
                        principalColumn: "PlaylistId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlaylistPublisheds",
                columns: table => new
                {
                    PlaylistPublishedId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PlaylistId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PublishedVersion = table.Column<int>(type: "integer", nullable: false),
                    PublishedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PublishedByUserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaylistPublisheds", x => x.PlaylistPublishedId);
                    table.ForeignKey(
                        name: "FK_PlaylistPublisheds_Playlists_PlaylistId",
                        column: x => x.PlaylistId,
                        principalTable: "Playlists",
                        principalColumn: "PlaylistId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlaylistPublisheds_Users_PublishedByUserId",
                        column: x => x.PublishedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SchedulePublisheds",
                columns: table => new
                {
                    SchedulePublishedId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    VenueScheduleSetId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    VenueId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PublishedVersion = table.Column<int>(type: "integer", nullable: false),
                    PublishedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PublishedByUserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchedulePublisheds", x => x.SchedulePublishedId);
                    table.ForeignKey(
                        name: "FK_SchedulePublisheds_Users_PublishedByUserId",
                        column: x => x.PublishedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SchedulePublisheds_VenueScheduleSets_VenueScheduleSetId",
                        column: x => x.VenueScheduleSetId,
                        principalTable: "VenueScheduleSets",
                        principalColumn: "VenueScheduleSetId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleSlots",
                columns: table => new
                {
                    SlotId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TenantId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    VenueScheduleSetId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    VenueId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TargetType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TargetId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PlaylistId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DaysOfWeekMask = table.Column<int>(type: "integer", nullable: false),
                    StartMinutes = table.Column<int>(type: "integer", nullable: false),
                    EndMinutes = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    ValidFromDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ValidToDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleSlots", x => x.SlotId);
                    table.ForeignKey(
                        name: "FK_ScheduleSlots_Playlists_PlaylistId",
                        column: x => x.PlaylistId,
                        principalTable: "Playlists",
                        principalColumn: "PlaylistId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScheduleSlots_VenueScheduleSets_VenueScheduleSetId",
                        column: x => x.VenueScheduleSetId,
                        principalTable: "VenueScheduleSets",
                        principalColumn: "VenueScheduleSetId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContentGroupPublishedItems",
                columns: table => new
                {
                    ContentGroupPublishedItemId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ContentGroupId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PublishedVersion = table.Column<int>(type: "integer", nullable: false),
                    AssetId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ChildGroupId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ChildGroupPublishedVersion = table.Column<int>(type: "integer", nullable: true),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: true),
                    ContentGroupPublishedId = table.Column<string>(type: "character varying(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentGroupPublishedItems", x => x.ContentGroupPublishedItemId);
                    table.ForeignKey(
                        name: "FK_ContentGroupPublishedItems_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "AssetId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ContentGroupPublishedItems_ContentGroupPublisheds_ContentGr~",
                        column: x => x.ContentGroupPublishedId,
                        principalTable: "ContentGroupPublisheds",
                        principalColumn: "ContentGroupPublishedId");
                    table.ForeignKey(
                        name: "FK_ContentGroupPublishedItems_ContentGroups_ChildGroupId",
                        column: x => x.ChildGroupId,
                        principalTable: "ContentGroups",
                        principalColumn: "ContentGroupId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ContentGroupPublishedItems_ContentGroups_ContentGroupId",
                        column: x => x.ContentGroupId,
                        principalTable: "ContentGroups",
                        principalColumn: "ContentGroupId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlaylistPublishedItems",
                columns: table => new
                {
                    PlaylistPublishedItemId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PlaylistId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PublishedVersion = table.Column<int>(type: "integer", nullable: false),
                    AssetId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: true),
                    SourceContentGroupId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SourceContentGroupVersion = table.Column<int>(type: "integer", nullable: true),
                    PlaylistPublishedId = table.Column<string>(type: "character varying(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaylistPublishedItems", x => x.PlaylistPublishedItemId);
                    table.ForeignKey(
                        name: "FK_PlaylistPublishedItems_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "AssetId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlaylistPublishedItems_ContentGroups_SourceContentGroupId",
                        column: x => x.SourceContentGroupId,
                        principalTable: "ContentGroups",
                        principalColumn: "ContentGroupId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PlaylistPublishedItems_PlaylistPublisheds_PlaylistPublished~",
                        column: x => x.PlaylistPublishedId,
                        principalTable: "PlaylistPublisheds",
                        principalColumn: "PlaylistPublishedId");
                    table.ForeignKey(
                        name: "FK_PlaylistPublishedItems_Playlists_PlaylistId",
                        column: x => x.PlaylistId,
                        principalTable: "Playlists",
                        principalColumn: "PlaylistId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SchedulePublishedSlots",
                columns: table => new
                {
                    SchedulePublishedSlotId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    VenueScheduleSetId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PublishedVersion = table.Column<int>(type: "integer", nullable: false),
                    TargetType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TargetId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PlaylistId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DaysOfWeekMask = table.Column<int>(type: "integer", nullable: false),
                    StartMinutes = table.Column<int>(type: "integer", nullable: false),
                    EndMinutes = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    ValidFromDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ValidToDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    SchedulePublishedId = table.Column<string>(type: "character varying(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchedulePublishedSlots", x => x.SchedulePublishedSlotId);
                    table.ForeignKey(
                        name: "FK_SchedulePublishedSlots_Playlists_PlaylistId",
                        column: x => x.PlaylistId,
                        principalTable: "Playlists",
                        principalColumn: "PlaylistId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SchedulePublishedSlots_SchedulePublisheds_SchedulePublished~",
                        column: x => x.SchedulePublishedId,
                        principalTable: "SchedulePublisheds",
                        principalColumn: "SchedulePublishedId");
                    table.ForeignKey(
                        name: "FK_SchedulePublishedSlots_VenueScheduleSets_VenueScheduleSetId",
                        column: x => x.VenueScheduleSetId,
                        principalTable: "VenueScheduleSets",
                        principalColumn: "VenueScheduleSetId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assets_DeletedAtUtc",
                table: "Assets",
                column: "DeletedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_ScopeVenueId",
                table: "Assets",
                column: "ScopeVenueId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_Status",
                table: "Assets",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_TenantId",
                table: "Assets",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentGroupItems_AssetId",
                table: "ContentGroupItems",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentGroupItems_ChildGroupId",
                table: "ContentGroupItems",
                column: "ChildGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentGroupItems_ContentGroupId",
                table: "ContentGroupItems",
                column: "ContentGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentGroupPublishedItems_AssetId",
                table: "ContentGroupPublishedItems",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentGroupPublishedItems_ChildGroupId",
                table: "ContentGroupPublishedItems",
                column: "ChildGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentGroupPublishedItems_ContentGroupId",
                table: "ContentGroupPublishedItems",
                column: "ContentGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentGroupPublishedItems_ContentGroupPublishedId",
                table: "ContentGroupPublishedItems",
                column: "ContentGroupPublishedId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentGroupPublisheds_ContentGroupId_PublishedVersion",
                table: "ContentGroupPublisheds",
                columns: new[] { "ContentGroupId", "PublishedVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContentGroupPublisheds_PublishedByUserId",
                table: "ContentGroupPublisheds",
                column: "PublishedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentGroups_DeletedAtUtc",
                table: "ContentGroups",
                column: "DeletedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_ContentGroups_ScopeVenueId",
                table: "ContentGroups",
                column: "ScopeVenueId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentGroups_TenantId",
                table: "ContentGroups",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceCommands_DeviceId",
                table: "DeviceCommands",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceCommands_IssuedAtUtc",
                table: "DeviceCommands",
                column: "IssuedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceCommands_IssuedByUserId",
                table: "DeviceCommands",
                column: "IssuedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceCommands_Status",
                table: "DeviceCommands",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceCommands_TimeoutAtUtc",
                table: "DeviceCommands",
                column: "TimeoutAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceEvents_CreatedAtUtc",
                table: "DeviceEvents",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceEvents_DeviceId",
                table: "DeviceEvents",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceEvents_EventType",
                table: "DeviceEvents",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceEvents_Severity",
                table: "DeviceEvents",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceStatuses_IsOnline",
                table: "DeviceStatuses",
                column: "IsOnline");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceStatuses_LastHeartbeatAtUtc",
                table: "DeviceStatuses",
                column: "LastHeartbeatAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceStatuses_VenueId",
                table: "DeviceStatuses",
                column: "VenueId");

            migrationBuilder.CreateIndex(
                name: "IX_EditorPresences_ExpiresAtUtc",
                table: "EditorPresences",
                column: "ExpiresAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_EditorPresences_TenantId_ResourceType_ResourceId",
                table: "EditorPresences",
                columns: new[] { "TenantId", "ResourceType", "ResourceId" });

            migrationBuilder.CreateIndex(
                name: "IX_EditorPresences_UserId",
                table: "EditorPresences",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistItems_AssetId",
                table: "PlaylistItems",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistItems_ContentGroupId",
                table: "PlaylistItems",
                column: "ContentGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistItems_PlaylistId",
                table: "PlaylistItems",
                column: "PlaylistId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistPublishedItems_AssetId",
                table: "PlaylistPublishedItems",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistPublishedItems_PlaylistId",
                table: "PlaylistPublishedItems",
                column: "PlaylistId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistPublishedItems_PlaylistPublishedId",
                table: "PlaylistPublishedItems",
                column: "PlaylistPublishedId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistPublishedItems_SourceContentGroupId",
                table: "PlaylistPublishedItems",
                column: "SourceContentGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistPublisheds_PlaylistId_PublishedVersion",
                table: "PlaylistPublisheds",
                columns: new[] { "PlaylistId", "PublishedVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistPublisheds_PublishedByUserId",
                table: "PlaylistPublisheds",
                column: "PublishedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_DeletedAtUtc",
                table: "Playlists",
                column: "DeletedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_ScopeVenueId",
                table: "Playlists",
                column: "ScopeVenueId");

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_TenantId",
                table: "Playlists",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PublishEvents_EntityId",
                table: "PublishEvents",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_PublishEvents_PublishedAtUtc",
                table: "PublishEvents",
                column: "PublishedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_PublishEvents_PublishedByUserId",
                table: "PublishEvents",
                column: "PublishedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PublishEvents_TenantId",
                table: "PublishEvents",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PublishEvents_Type",
                table: "PublishEvents",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_PublishEvents_VenueId",
                table: "PublishEvents",
                column: "VenueId");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulePublisheds_PublishedByUserId",
                table: "SchedulePublisheds",
                column: "PublishedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulePublisheds_VenueScheduleSetId_PublishedVersion",
                table: "SchedulePublisheds",
                columns: new[] { "VenueScheduleSetId", "PublishedVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SchedulePublishedSlots_PlaylistId",
                table: "SchedulePublishedSlots",
                column: "PlaylistId");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulePublishedSlots_SchedulePublishedId",
                table: "SchedulePublishedSlots",
                column: "SchedulePublishedId");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulePublishedSlots_VenueScheduleSetId_PublishedVersion",
                table: "SchedulePublishedSlots",
                columns: new[] { "VenueScheduleSetId", "PublishedVersion" });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleSlots_PlaylistId",
                table: "ScheduleSlots",
                column: "PlaylistId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleSlots_TargetId",
                table: "ScheduleSlots",
                column: "TargetId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleSlots_VenueId",
                table: "ScheduleSlots",
                column: "VenueId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleSlots_VenueId_DaysOfWeekMask_StartMinutes_EndMinutes",
                table: "ScheduleSlots",
                columns: new[] { "VenueId", "DaysOfWeekMask", "StartMinutes", "EndMinutes" });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleSlots_VenueScheduleSetId",
                table: "ScheduleSlots",
                column: "VenueScheduleSetId");

            migrationBuilder.CreateIndex(
                name: "IX_VenueScheduleSets_VenueId",
                table: "VenueScheduleSets",
                column: "VenueId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentGroupItems");

            migrationBuilder.DropTable(
                name: "ContentGroupPublishedItems");

            migrationBuilder.DropTable(
                name: "DeviceCommands");

            migrationBuilder.DropTable(
                name: "DeviceEvents");

            migrationBuilder.DropTable(
                name: "DeviceStatuses");

            migrationBuilder.DropTable(
                name: "EditorPresences");

            migrationBuilder.DropTable(
                name: "PlaylistItems");

            migrationBuilder.DropTable(
                name: "PlaylistPublishedItems");

            migrationBuilder.DropTable(
                name: "PublishEvents");

            migrationBuilder.DropTable(
                name: "SchedulePublishedSlots");

            migrationBuilder.DropTable(
                name: "ScheduleSlots");

            migrationBuilder.DropTable(
                name: "VenueEpochs");

            migrationBuilder.DropTable(
                name: "ContentGroupPublisheds");

            migrationBuilder.DropTable(
                name: "Assets");

            migrationBuilder.DropTable(
                name: "PlaylistPublisheds");

            migrationBuilder.DropTable(
                name: "SchedulePublisheds");

            migrationBuilder.DropTable(
                name: "ContentGroups");

            migrationBuilder.DropTable(
                name: "Playlists");

            migrationBuilder.DropTable(
                name: "VenueScheduleSets");
        }
    }
}
