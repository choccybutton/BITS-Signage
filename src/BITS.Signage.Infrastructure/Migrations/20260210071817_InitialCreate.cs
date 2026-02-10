using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BITS.Signage.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    TenantId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DefaultTimezone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.TenantId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TenantId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Users_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Venues",
                columns: table => new
                {
                    VenueId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TenantId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Timezone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FallbackPlaylistId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Venues", x => x.VenueId);
                    table.ForeignKey(
                        name: "FK_Venues_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantUserRoles",
                columns: table => new
                {
                    TenantUserRoleId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TenantId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantUserRoles", x => x.TenantUserRoleId);
                    table.ForeignKey(
                        name: "FK_TenantUserRoles_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TenantUserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeviceGroups",
                columns: table => new
                {
                    GroupId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TenantId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    VenueId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceGroups", x => x.GroupId);
                    table.ForeignKey(
                        name: "FK_DeviceGroups_Venues_VenueId",
                        column: x => x.VenueId,
                        principalTable: "Venues",
                        principalColumn: "VenueId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VenueUserRoles",
                columns: table => new
                {
                    VenueUserRoleId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TenantId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    VenueId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VenueUserRoles", x => x.VenueUserRoleId);
                    table.ForeignKey(
                        name: "FK_VenueUserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VenueUserRoles_Venues_VenueId",
                        column: x => x.VenueId,
                        principalTable: "Venues",
                        principalColumn: "VenueId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    DeviceId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TenantId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    VenueId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PrimaryGroupId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Timezone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RunMode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LaunchOnBoot = table.Column<bool>(type: "boolean", nullable: false),
                    AutoRelaunch = table.Column<bool>(type: "boolean", nullable: false),
                    KeepAwake = table.Column<bool>(type: "boolean", nullable: false),
                    WatchdogReturnToForeground = table.Column<bool>(type: "boolean", nullable: false),
                    Orientation = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastSeenAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AppVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.DeviceId);
                    table.ForeignKey(
                        name: "FK_Devices_DeviceGroups_PrimaryGroupId",
                        column: x => x.PrimaryGroupId,
                        principalTable: "DeviceGroups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Devices_Venues_VenueId",
                        column: x => x.VenueId,
                        principalTable: "Venues",
                        principalColumn: "VenueId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeviceAuthTokens",
                columns: table => new
                {
                    DeviceAuthTokenId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TenantId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    VenueId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DeviceId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TokenHash = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IssuedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceAuthTokens", x => x.DeviceAuthTokenId);
                    table.ForeignKey(
                        name: "FK_DeviceAuthTokens_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "DeviceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DevicePairings",
                columns: table => new
                {
                    PairingId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PairingCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DeviceHardwareId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClaimedByUserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ClaimedTenantId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ClaimedVenueId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ResultingDeviceId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevicePairings", x => x.PairingId);
                    table.ForeignKey(
                        name: "FK_DevicePairings_Devices_ResultingDeviceId",
                        column: x => x.ResultingDeviceId,
                        principalTable: "Devices",
                        principalColumn: "DeviceId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DevicePairings_Tenants_ClaimedTenantId",
                        column: x => x.ClaimedTenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DevicePairings_Users_ClaimedByUserId",
                        column: x => x.ClaimedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DevicePairings_Venues_ClaimedVenueId",
                        column: x => x.ClaimedVenueId,
                        principalTable: "Venues",
                        principalColumn: "VenueId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceAuthTokens_DeviceId",
                table: "DeviceAuthTokens",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceAuthTokens_ExpiresAtUtc",
                table: "DeviceAuthTokens",
                column: "ExpiresAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceAuthTokens_Status",
                table: "DeviceAuthTokens",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceGroups_VenueId",
                table: "DeviceGroups",
                column: "VenueId");

            migrationBuilder.CreateIndex(
                name: "IX_DevicePairings_ClaimedByUserId",
                table: "DevicePairings",
                column: "ClaimedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DevicePairings_ClaimedTenantId",
                table: "DevicePairings",
                column: "ClaimedTenantId");

            migrationBuilder.CreateIndex(
                name: "IX_DevicePairings_ClaimedVenueId",
                table: "DevicePairings",
                column: "ClaimedVenueId");

            migrationBuilder.CreateIndex(
                name: "IX_DevicePairings_DeviceHardwareId",
                table: "DevicePairings",
                column: "DeviceHardwareId");

            migrationBuilder.CreateIndex(
                name: "IX_DevicePairings_ExpiresAtUtc",
                table: "DevicePairings",
                column: "ExpiresAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_DevicePairings_PairingCode",
                table: "DevicePairings",
                column: "PairingCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DevicePairings_ResultingDeviceId",
                table: "DevicePairings",
                column: "ResultingDeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_DevicePairings_Status",
                table: "DevicePairings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_DeletedAtUtc",
                table: "Devices",
                column: "DeletedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_PrimaryGroupId",
                table: "Devices",
                column: "PrimaryGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_Status",
                table: "Devices",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_TenantId",
                table: "Devices",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_VenueId",
                table: "Devices",
                column: "VenueId");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_DeletedAtUtc",
                table: "Tenants",
                column: "DeletedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Status",
                table: "Tenants",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TenantUserRoles_Role",
                table: "TenantUserRoles",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_TenantUserRoles_TenantId_UserId",
                table: "TenantUserRoles",
                columns: new[] { "TenantId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantUserRoles_UserId",
                table: "TenantUserRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_DeletedAtUtc",
                table: "Users",
                column: "DeletedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Status",
                table: "Users",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId",
                table: "Users",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Venues_DeletedAtUtc",
                table: "Venues",
                column: "DeletedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Venues_TenantId",
                table: "Venues",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_VenueUserRoles_Role",
                table: "VenueUserRoles",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_VenueUserRoles_UserId",
                table: "VenueUserRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VenueUserRoles_VenueId_UserId",
                table: "VenueUserRoles",
                columns: new[] { "VenueId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceAuthTokens");

            migrationBuilder.DropTable(
                name: "DevicePairings");

            migrationBuilder.DropTable(
                name: "TenantUserRoles");

            migrationBuilder.DropTable(
                name: "VenueUserRoles");

            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "DeviceGroups");

            migrationBuilder.DropTable(
                name: "Venues");

            migrationBuilder.DropTable(
                name: "Tenants");
        }
    }
}
